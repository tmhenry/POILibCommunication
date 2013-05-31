using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace POILibCommunication
{
    public class POIUDPReceiver
    {
        private IPEndPoint localEP;
        private Socket myUdpSock;
        private EndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);

        private byte[] buffer = new byte[1500];

        enum DataType { CONTROL, GESTURE, TOUCH, MOTION, KEYBOARD };

        public POIUDPReceiver(IPEndPoint myEP)
        {
            localEP = myEP;
            myUdpSock = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            myUdpSock.Bind(localEP);
        }

        public void Start()
        {
                       
            SocketAsyncEventArgs args = new SocketAsyncEventArgs();
            args.SetBuffer(buffer, 0, 1500);
            args.RemoteEndPoint = remoteEP;
            args.Completed += new EventHandler<SocketAsyncEventArgs>(Read_Completed);
            myUdpSock.ReceiveFromAsync(args);
            
        }

        private void Read_Completed(object sender, SocketAsyncEventArgs args)
        {
            //POIGlobalVar.POIDebugLog("Receiving UDP data");

            if (args.SocketError == SocketError.Success && args.BytesTransferred > 0)
            {
                string remoteIP = (args.RemoteEndPoint as IPEndPoint).Address.ToString();

                if (POIGlobalVar.UserProfiles.ContainsKey(remoteIP))
                {
                    //Only parse message that are from a valid user
                    POIUser curUser = POIGlobalVar.UserProfiles[remoteIP];
                    if (curUser.Status == POIUser.ConnectionStatus.Connected)
                    {
                        //If the socket is constructed
                        if (curUser.UdpChannel == null)
                        {
                            //Set the new UDP connection for real time control
                            curUser.UdpChannel = myUdpSock;
                            curUser.UDPEndPoint = args.RemoteEndPoint as IPEndPoint;
                        }

                        byte[] data = new byte[args.BytesTransferred];
                        Array.Copy(buffer, data, args.BytesTransferred);

                        ParsingData(data, curUser);
                        //receivedCalled = true;
                    }

                }

            }
            else
            {
                POIGlobalVar.POIDebugLog(args.SocketError);
            }

            //Make sure the receiving is continous
            myUdpSock.ReceiveFromAsync(args);
            

        }

        private void ParsingData(byte[] data, POIUser user)
        {
            //Start parsing data
            DataType myType = (DataType)data[0];
            byte[] packet = data.Skip(1).ToArray();

            //DataParsers myParser = user.myParser;
            POIRealtimeMsgCB dataHandler = user.RealtimeCtrlHandler;

            switch (myType)
            {
                case DataType.GESTURE:
                    GestureDataParser(packet, dataHandler);
                    break;
                case DataType.MOTION:
                    //myParser.MotionDataParser(packet);
                    break;
                case DataType.TOUCH:
                    TouchDataParser(packet, dataHandler);
                    break;
                case DataType.KEYBOARD:
                    //myParser.KeyboardParser(packet);
                    break;
            }
                    
        }

        public unsafe void GestureDataParser(byte[] bufferArray, POIRealtimeMsgCB cb)
        {

            fixed (byte* bytePointer = bufferArray)
            {
                byte* gestureData = bytePointer;
                GestureType* dataType = (GestureType*)gestureData;
                if (*dataType == GestureType.SWIPE)
                {

                    //Console.Write("Swipe\n");
                    gestureData += sizeof(GestureType);
                    SwipeGestureData* swipeData = (SwipeGestureData*)gestureData;

                    cb.Handle_Swipe(ref *swipeData);
                }
                else if (*dataType == GestureType.PINCH)
                {
                    //Console.Write("Pinch\n");
                    gestureData += sizeof(GestureType);

                    PinchGestureData* pinchData = (PinchGestureData*)gestureData;
                    cb.Handle_Scale(ref *pinchData);

                }
                else if (*dataType == GestureType.ROTATION)
                {
                    //Console.Write("Rotation\n");
                    gestureData += sizeof(GestureType);

                    RotationGestureData* rotationData = (RotationGestureData*)gestureData;
                    cb.Handle_Rotate(ref *rotationData);
                }
                else if (*dataType == GestureType.SCROLLING)
                {
                    //Console.Write("Scrolling\n");

                    gestureData += sizeof(GestureType);
                    ScrollingGestureData* scrollingData = (ScrollingGestureData*)gestureData;
                    //ScrollngParser(scrollingData);
                }

            }
        }

        public unsafe void TouchDataParser(byte[] bufferArray,POIRealtimeMsgCB cb)
        {
            fixed (byte* bytePointer = bufferArray)
            {
                byte* touchData = bytePointer;
                TouchPhase* touchPhase = (TouchPhase*)touchData;
                if (*touchPhase == TouchPhase.TOUCH_BEGIN)
                {
                    //Console.Write("Begin\n");
                    touchData += sizeof(TouchPhase);
                    //TouchBeginParser(touchData);
                    cb.Handle_TouchBegin(touchData);
                }
                else if (*touchPhase == TouchPhase.TOUCH_MOVE)
                {
                    //Console.Write("Move\n");
                    touchData += sizeof(TouchPhase);
                    //TouchMoveParser(touchData);
                    cb.Handle_TouchMove(touchData);
                }

                else if (*touchPhase == TouchPhase.TOUCH_END)
                {
                    //Console.Write("End\n");
                    touchData += sizeof(TouchPhase);
                    //TouchEndParser(touchData);
                    cb.Handle_TouchEnd(touchData);
                }

            }

        }
    }
}
