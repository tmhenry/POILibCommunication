using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;

namespace POILibCommunication
{
    public interface POITCPConnectionCBDelegate
    {
        void ConnectionEnded(POITCPConnection con);
        void ConnectionAuthenticated(POITCPConnection con);
    }

    public class POITCPConnection: POIMsgParser, POIInitializeClientMsgCB
    {
        public POITCPConnectionCBDelegate connectionCBDelegate { get; set; }

        Socket mySocket;
        public String Address { get; set; }

        const int maxCtrlPayloadSize = 2000;
        const int maxHeaderSize = sizeof(int);
        const long maxDataPayloadSize = 10000000;

        private byte[] TCP_ControlBuffer = new byte[1400];
        public int PayloadReceived = 0;
        public int HeaderReceived = 0;
        public int HeaderSize = maxHeaderSize;
        public int PayloadSize = 0;

        public byte[] Payload = new byte[maxCtrlPayloadSize];
        public byte[] Header = new byte[maxHeaderSize];

        

        public POITCPConnection(Socket sock)
        {
            mySocket = sock;
            
            //Get the IPAddress
            IPEndPoint remoteIPEP = mySocket.RemoteEndPoint as IPEndPoint;
            Address = remoteIPEP.Address.ToString();

            //msgParser.initClientMsgDelegate = this;
            initClientMsgDelegate = this;

            //Start receiving
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(TCP_ControlBuffer, 0, 1400);
            args.Completed += new EventHandler<SocketAsyncEventArgs>(Receive_Completed);

            mySocket.ReceiveAsync(args);
        }

        private void Receive_Completed(object sender, SocketAsyncEventArgs args)
        {
            //Get the token as a user object
            

            if (args.SocketError == SocketError.Success && args.BytesTransferred > 0)
            {

                int remainingBytes = args.BytesTransferred;
                int remainingHeaderBytes;
                int remainingPayloadBytes;
                int bytesProcessed;
                int offset = 0;

                //Console.WriteLine("TCP control Received bytes " + args.BytesTransferred);

                while (remainingBytes > 0)
                {
                    //Check if the header has been received
                    if (HeaderReceived < HeaderSize)
                    {
                        remainingHeaderBytes = HeaderSize - HeaderReceived;

                        if (remainingHeaderBytes > remainingBytes)
                        {
                            bytesProcessed = remainingBytes;

                            //Add bytes into the header
                            Array.Copy(TCP_ControlBuffer, offset, Header, HeaderReceived, bytesProcessed);

                            //Update state variables
                            HeaderReceived += bytesProcessed;
                            remainingBytes -= bytesProcessed;
                            offset += bytesProcessed;
                        }
                        else
                        {
                            bytesProcessed = remainingHeaderBytes;

                            //Add bytes into the header
                            Array.Copy(TCP_ControlBuffer, offset, Header, HeaderReceived, bytesProcessed);


                            HeaderReceived += bytesProcessed;
                            remainingBytes -= bytesProcessed;
                            offset += bytesProcessed;

                            //Header received completely, read the payload size
                            //Console.WriteLine("Receiving Header " + myUser.ctrlHeader);
                            PayloadSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(Header, 0));
                            PayloadReceived = 0;
                        }

                    }
                    else
                    {
                        remainingPayloadBytes = PayloadSize - PayloadReceived;

                        if (remainingPayloadBytes > remainingBytes)
                        {
                            bytesProcessed = remainingBytes;

                            //Add bytes into the payload
                            Array.Copy(TCP_ControlBuffer, offset, Payload, PayloadReceived, bytesProcessed);

                            PayloadReceived += bytesProcessed;
                            remainingBytes -= bytesProcessed;
                            offset += bytesProcessed;

                            //Console.WriteLine("Payload received: " + PayloadReceived);
                            //Console.WriteLine("remainingPayloadBytes: " + remainingPayloadBytes);
                        }
                        else
                        {
                            bytesProcessed = remainingPayloadBytes;

                            //Add bytes into the payload
                            Array.Copy(TCP_ControlBuffer, offset, Payload, PayloadReceived, bytesProcessed);


                            PayloadReceived += bytesProcessed;
                            remainingBytes -= bytesProcessed;
                            offset += bytesProcessed;

                            Console.WriteLine(PayloadReceived);

                            //Payload received completely
                            //Console.WriteLine("Here!");
                            HeaderReceived = 0;
                            //ParseTCPControlMsg(myUser, mySocket, myUser.ctrlPayload);

                            parsePacket(Payload);
                        }
                    }
                }

                //Start another round of async read
                mySocket.ReceiveAsync(args);

            }
            else
            {
                Console.WriteLine(args.SocketError);
                mySocket.Close();

                connectionCBDelegate.ConnectionEnded(this);
            }
        }

        public void SendData(byte[] data)
        {

            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.Completed += new EventHandler<SocketAsyncEventArgs>(Send_Completed);

            byte[] header = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(data.Length));
            byte[] dataToSent = header.Concat(data).ToArray();
            args.SetBuffer(dataToSent, 0, dataToSent.Length);

            mySocket.SendAsync(args);
        }

        private void Send_Completed(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success && args.BytesTransferred > 0)
            {
                //Console.WriteLine("Send successfully!" + e.BytesTransferred);
            }
            else
            {
                Console.WriteLine("TCP sending error!");
            }
        }

        public void Disconnect()
        {
            mySocket.Close();
        }

        public void InitPayloadBufferForDataChannel()
        {
            Payload = new byte[maxDataPayloadSize];
        }

        /*
         * Delegate functions for POI initialization protocol
         */
        public void helloMsgReceived(POIHelloMsg par)
        {
            //Notify data handler that authentication has been done
            //Proper CB functions are set here
            int userType = (int)par.UserType;
            int conType = (int)par.ConnType;

            
            if (conType == POIMsgDefinition.POI_CONTROL_CHANNEL)
            {
                //POIGlobalVar.SystemDataHandler.CtrlChannelAuthenticated(this);
                Type = POIMsgParser.ParserType.Control;
            }
            else if (conType == POIMsgDefinition.POI_DATA_CHANNEL)
            {
                //POIGlobalVar.SystemDataHandler.DataChannelAuthenticated(this);
                Type = POIMsgParser.ParserType.Data;
            }
            

            if (userType == POIMsgDefinition.POI_HELLOTYPE_COMMANDER)
            {
                privilegeLevel = POIMsgParser.Privilege.Commander;
            }
            else if (userType == POIMsgDefinition.POI_HELLOTYPE_VIEWER)
            {
                privilegeLevel = POIMsgParser.Privilege.Viewer;
            }

            //Call connection authenticated callback
            connectionCBDelegate.ConnectionAuthenticated(this);

            

            //Send back the welcome message
            POIWelcomeMsg welcomeMsg = new POIWelcomeMsg(1);
            SendData(welcomeMsg.getPacket());
            
        }
    }
}
