using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace POILibCommunication
{
    //Implement the broadcast channel as a state machine
    public class POIBroadcast
    {
        //Support for UDP broadcast
        Socket broadCastChannel;
        IPAddress broadCastAddr;
        IPAddress localAddr;

        int curBroadcastSeqNum = 0;
        List<byte[]> curBroadcastFrame;
        int payloadSizeUDP = 1000;
        int curFrameLastPktSize = 1000;

        int numBroadcastBeginAcked = 0;
        int numBroadcastEndAcked = 0;
        byte[] beginMsg;
        byte[] endMsg;

        int broadCastPort = 5198;

        enum BroadcastState
        {
            Idle,
            WaitForBeginAck,
            Broadcasting,
            WaitForEndAck
        }

        BroadcastState myBroadcastState = BroadcastState.Idle;

        POIMsgParser msgParser = new POIMsgParser();

        public POIBroadcast()
        {
            //Find current broadcast address
            broadCastAddr = IPAddress.Parse("192.168.1.255");
            IPAddress[] localAddresses = Dns.GetHostAddresses(Dns.GetHostName());
            foreach (IPAddress ip in localAddresses)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {

                    //Find local address
                    localAddr = ip;
                    POIGlobalVar.POIDebugLog(ip.ToString());

                    //Find broadcast address
                    byte[] bcBytes = IPAddress.Broadcast.GetAddressBytes();
                    Array.Copy(localAddr.GetAddressBytes(), bcBytes, 3);
                    broadCastAddr = new IPAddress(bcBytes);
                    POIGlobalVar.POIDebugLog(broadCastAddr.ToString());
                }
            }

            //Initialize a broadcast channel using UDP
            broadCastChannel = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            broadCastChannel.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
        }

        public byte[] getCurBroadcastFramePacket(int seqNum)
        {
            return curBroadcastFrame[seqNum];
        }

        private void PrepareBroadcastFrame(byte[] msg)
        {
            curBroadcastSeqNum++;

            // Some testing cases
            //System.Text.ASCIIEncoding encoding = new System.Text.ASCIIEncoding();
            //msg = encoding.GetBytes(@"what is the problem of you?");
            //payloadSizeUDP = 2;

            int pktPayloadSize = payloadSizeUDP;
            int numPkt = (int)Math.Ceiling((double)msg.Length / pktPayloadSize);
            curBroadcastFrame = new List<byte[]>(numPkt);
            curFrameLastPktSize = msg.Length - (numPkt - 1) * payloadSizeUDP;

            int seqNum;

            for (seqNum = 0; seqNum < numPkt; seqNum++)
            {
                byte[] curMsg;
                if (seqNum < numPkt - 1)
                {
                    curMsg = msg.Skip(seqNum * pktPayloadSize).Take(pktPayloadSize).ToArray();
                }
                else
                {
                    curMsg = msg.Skip(seqNum * pktPayloadSize).Take(msg.Length - seqNum * pktPayloadSize).ToArray();
                }

                BroadcastContentPar par = new BroadcastContentPar();
                par.frameNum = curBroadcastSeqNum;
                par.seqNum = seqNum;

                byte[] newMsg = msgParser.getBroadcastCotentMsg(ref par, curMsg);
                curBroadcastFrame.Add(newMsg);

            }

        }

        public unsafe void BroadCastMsg(byte[] msg)
        {
            POIGlobalVar.POIDebugLog("Start broadcasting " + msg.Length + " bytes");

            PrepareBroadcastFrame(msg);

            numBroadcastBeginAcked = 0;
            numBroadcastEndAcked = 0;

            //Get the begin and end message
            BroadcastBeginPar beginPar = new BroadcastBeginPar();
            beginPar.frameNum = curBroadcastSeqNum;
            beginPar.lastPacketPayload = curFrameLastPktSize;
            beginPar.numPackets = curBroadcastFrame.Count;
            beginPar.packetPayload = payloadSizeUDP;

            BroadcastEndPar endPar = new BroadcastEndPar();
            endPar.frameNum = curBroadcastSeqNum;

            beginMsg = msgParser.getBroadcastBeginMsg(ref beginPar);
            endMsg = msgParser.getBroadcastEndMsg(ref endPar);

            
            //Send out the begin message
            foreach (POIUser u in POIGlobalVar.UserProfiles.Values)
            {
                u.BroadcastCtrlHandler.resetStateVariables();
                u.SendData(beginMsg, ConType.TCP_CONTROL);
            }

            myBroadcastState = BroadcastState.WaitForBeginAck;
        }

        public void broadcastBeginAcked(POIUser u)
        {
            if (CheckEveryoneAckedBroadcastBegin())
            {
                SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();

                sendArgs.RemoteEndPoint = new IPEndPoint(broadCastAddr, broadCastPort);
                sendArgs.SetBuffer(curBroadcastFrame[0], 0, curBroadcastFrame[0].Length);
                sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(broadcastContentPacketCompleted);
                sendArgs.UserToken = new Tuple<int, int>(0, 0);
                broadCastChannel.SendToAsync(sendArgs);

                myBroadcastState = BroadcastState.Broadcasting;
            }
        }

        private void broadcastContentPacketCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {

                int seqNum = (e.UserToken as Tuple<int, int>).Item1;
                int bcCount = (e.UserToken as Tuple<int, int>).Item2;

                if (bcCount < 0)
                {
                    bcCount++;
                }
                else
                {
                    bcCount = 0;
                    seqNum++;
                }

                if (seqNum == 10) seqNum++;

                if (seqNum < curBroadcastFrame.Count) //Start to send normal msg
                {
                    e.UserToken = new Tuple<int, int>(seqNum, bcCount);
                    e.SetBuffer(curBroadcastFrame[seqNum], 0, curBroadcastFrame[seqNum].Length);
                    broadCastChannel.SendToAsync(e);
                }
                else
                {
                    Thread.Sleep(100);

                    //Broadcast end messages
                    SocketAsyncEventArgs sendArgs = new SocketAsyncEventArgs();
                    sendArgs.RemoteEndPoint = new IPEndPoint(broadCastAddr, broadCastPort);
                    sendArgs.SetBuffer(endMsg, 0, endMsg.Length);
                    sendArgs.Completed += new EventHandler<SocketAsyncEventArgs>(broadcastEndPacketCompleted);
                    broadCastChannel.SendToAsync(sendArgs);
                }
            }

        }

        private void broadcastEndPacketCompleted(object sender, SocketAsyncEventArgs e)
        {
            if (e.SocketError == SocketError.Success && e.BytesTransferred > 0)
            {
                if (!CheckEveryoneAckedBroadcastEnd())
                {
                    //Wait for some time
                    Thread.Sleep(100);

                    //Keep waiting for ack from everyone
                    broadCastChannel.SendToAsync(e);
                }
            }
        }

        public void broadcastEndAcked(POIUser u)
        {
            if (CheckEveryoneAckedBroadcastEnd())
            {
                myBroadcastState = BroadcastState.Idle;
            }
        }

        public bool CheckEveryoneAckedBroadcastBegin()
        {
            foreach (POIUser u in POIGlobalVar.UserProfiles.Values)
            {
                if (u.BroadcastCtrlHandler.getStateVariables().Item1 == false)
                {
                    return false;
                }
            }
            return true;
        }

        public bool CheckEveryoneAckedBroadcastEnd()
        {
            foreach (POIUser u in POIGlobalVar.UserProfiles.Values)
            {
                if (u.BroadcastCtrlHandler.getStateVariables().Item2 == false)
                {
                    return false;
                }
            }
            return true;
        }
    }
}
