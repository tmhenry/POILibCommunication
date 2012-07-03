using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

using System.Windows.Media.Imaging;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.Windows;

namespace POILibCommunication
{
    public class POIComServer : POITCPConnectionCBDelegate
    {
        private static object broadCastLock = new object();

        static byte[] myBuffer = new byte[1400];
        Socket listener;

        int maxClientCount = 5;

        int TCPDataControlMsgSize = 2 * sizeof(Int32);

        public POIBroadcast BroadcastChannel { get; set; }
        public POIUDPReceiver UDPServer { get; set; }

        #region Callback function for TCP connections

        public void ConnectionEnded(POITCPConnection connection)
        {
            POIUser user;

            //If the user already exists
            if (POIGlobalVar.UserProfiles.ContainsKey(connection.Address))
            {
                //Set the control channel
                user = POIGlobalVar.UserProfiles[connection.Address];

                if (connection.Type == POIMsgParser.ParserType.Control)
                {
                    //Reset the user status
                    user.Status = POIUser.ConnectionStatus.Disconnected;

                    //Notify the kernel
                    POIGlobalVar.SystemKernel.Handle_UserLeave(new POIUserEventArgs(user, new Point(0, 0)));
                }
                else if (connection.Type == POIMsgParser.ParserType.Data)
                {
                    //To do: end the data connection
                }
            }
            else //User does not exists
            {
                Console.WriteLine(@"Error: no user existed!");
            }

            
        }

        public void ConnectionAuthenticated(POITCPConnection connection)
        {
            if (connection.Type == POIMsgParser.ParserType.Control)
            {
                CtrlChannelAuthenticated(connection);
            }
            else if (connection.Type == POIMsgParser.ParserType.Data)
            {
                DataChannelAuthenticated(connection);
            }
        }

        #endregion

        //Pass in a TextBlock for debug purposes
        public POIComServer(Socket myListener)
        {
            //Put the socket on the listener, accept connection asynchronously
            listener = myListener;
            listener.Listen(20);

            BroadcastChannel = new POIBroadcast();

            UDPServer = new POIUDPReceiver(listener.LocalEndPoint as IPEndPoint);

            StartAcceptNewClient();

            UDPServer.Start();
        }
       
        ~POIComServer()
        {
            listener.Close();
        }

        private void StartAcceptNewClient()
        {
            SocketAsyncEventArgs myArgs = new SocketAsyncEventArgs();
            myArgs.Completed += SocketAcceptCompleted;

            listener.AcceptAsync(myArgs);
        }

        private void SocketAcceptCompleted(object sender, SocketAsyncEventArgs e)
        {
            Console.WriteLine(@"Accepting");

            //Start authenticating the new connection
            POITCPConnection connection = new POITCPConnection(e.AcceptSocket);
            connection.connectionCBDelegate = this;

          
            //If there is still room for new clients, start another round of accepting.
            if (POIGlobalVar.UserProfiles.Count <= maxClientCount)
            {
                try
                {
                    StartAcceptNewClient();
                }
                catch (Exception error)
                {
                    Console.WriteLine(error.Message);
                }
            }
        }

        public void CtrlChannelAuthenticated(POITCPConnection connection)
        {
            POIUser user;

            //If the user already exists
            if (POIGlobalVar.UserProfiles.ContainsKey(connection.Address))
            {
                //Set the control channel
                user = POIGlobalVar.UserProfiles[connection.Address];
            }
            else //User does not exists
            {
                user = new POIUser();
                POIGlobalVar.UserProfiles.Add(connection.Address, user);
                user.UserID = connection.Address;
            }

            //Setup connection delegates
            connection.Delegates = user;

            user.CtrlChannel = connection;

            if (user.Status == POIUser.ConnectionStatus.Disconnected)
            {
                user.Status = POIUser.ConnectionStatus.Connected;
            }

            Console.WriteLine(@"Oh yeah");

            try
            {
                POIGlobalVar.SystemKernel.Handle_UserJoin(new POIUserEventArgs(user, new Point(0, 0)));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
        }

        public void DataChannelAuthenticated(POITCPConnection connection)
        {
            Console.WriteLine(@"Data connection authenticated!");

            //If the user already exists
            if (POIGlobalVar.UserProfiles.ContainsKey(connection.Address))
            {
                //If the control channel is already connected
                POIUser user = POIGlobalVar.UserProfiles[connection.Address];
                if (user.Status == POIUser.ConnectionStatus.Connected)
                {
                    connection.Delegates = user;
                    user.DataChannel = connection;
                }
            }
            else //Drop the connection
            {
                connection.Disconnect();
            }
        }

        /*
        private unsafe void TCPDataReceiving(object data)
        {
            Tuple<Socket, POIUser> dataTuple = (Tuple<Socket, POIUser>)data;
            
            //Convert the argument to a socket
            Socket mySocket = dataTuple.Item1;
            POIUser myUser = dataTuple.Item2;

            NetworkStream myStream = new NetworkStream(mySocket);
            
            string packet;
            String[] packetInfo;

            //Readin the first packet to decide the type of connection
            packet = ReadControlMsg(myStream);
            myStream.Close();

            Console.WriteLine(packet);

            if (packet == "TCP_Data")
            {
                //Set the tcp data channel
                myUser.SetConnection(ConType.TCP_DATA, mySocket);
                
                byte[] dataBuffer = new byte[1400];
                //Store the buffer information into the user
                myUser.InitTCPDataBuffer(mySocket, dataBuffer);

                mySocket.ReceiveTimeout = 10000;
                mySocket.BeginReceive(
                    dataBuffer,
                    0,
                    TCPDataControlMsgSize,
                    SocketFlags.None,
                    new AsyncCallback(ReadTCPDataCallback),
                    mySocket
                );
            }
            else if (packet == "TCP_Control")
            {
                //Set the tcp control channel
                myUser.SetConnection(ConType.TCP_CONTROL, mySocket);
                mySocket.ReceiveTimeout = 10000;
                mySocket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.NoDelay, true);


                SocketAsyncEventArgs controlArg = new SocketAsyncEventArgs();
                controlArg.SetBuffer(myUser.TCP_ControlBuffer, 0, 1400);
                controlArg.Completed += new EventHandler<SocketAsyncEventArgs>(ReadTCPControl_Completed);
                controlArg.UserToken = myUser;

                mySocket.ReceiveAsync(controlArg);

                
            }
            else
            {
                RemoveConnection(mySocket);
                mySocket.Close();
            }   

        }*/

        
        /*
        private void ReadTCPDataCallback(IAsyncResult myResult)
        {
            try
            {
                Socket mySocket = (Socket)myResult.AsyncState;
                int recv = mySocket.EndReceive(myResult);
                Console.WriteLine("Receive successfully!" + recv);

                //Get the user associated with the socket
                IPEndPoint remoteIPEP = mySocket.RemoteEndPoint as IPEndPoint;
                String remoteIP = IPAddress.Parse(remoteIPEP.Address.ToString()).ToString();
                POIUser myUser = POIGlobalVar.UserProfiles[remoteIP];

                Tuple<byte[], int> bufferInfo = myUser.GetTCPDataBuffer(mySocket);
                byte[] myBuffer = bufferInfo.Item1.Take(recv).ToArray();

                byte[] optionBytes = myBuffer.Take(sizeof(Int32)).ToArray();
                byte[] paramBytes = myBuffer.Skip(sizeof(Int32)).Take(sizeof(Int32)).ToArray();
                    
                //Get the option and the size
                int option = BitConverter.ToInt32(optionBytes, 0);
                int parameter = BitConverter.ToInt32(paramBytes, 0);
                //Console.WriteLine(option + " " + parameter);

                //Parse the received packet
                switch (option)
                {
                    case 0:
                        Console.WriteLine("PULL");
                        byte[] pullBuffer = POIGlobalVar.SystemKernel.Handle_UserPullFromTable(myUser);
                        int length = pullBuffer.Length;
                        Console.WriteLine(length);
                        byte[] headerBytes = GetBytesFromInt(length);
                        pullBuffer = headerBytes.Concat(pullBuffer).ToArray();
                        Console.WriteLine(BitConverter.ToInt32(headerBytes, 0));

                        if (pullBuffer != null)
                        {
                            mySocket.BeginSend(
                               pullBuffer,
                               0,
                               pullBuffer.Length,
                               SocketFlags.None,
                               new AsyncCallback(PullFromTableCallback),
                               mySocket
                            );
                        }
                        
                        break;
                    case 1:
                        Console.WriteLine("PUSH");
                        int dataSize = parameter;
                        byte[] dataBuffer = new byte[dataSize];

                        //Store the buffer information into the user
                        myUser.RemoveTCPDataBuffer(mySocket);
                        myUser.InitTCPDataBuffer(mySocket, dataBuffer);
                        mySocket.ReceiveTimeout = 10000;

                        //int size = Math.Min(1400, dataSize);
                        int size = dataSize;

                        mySocket.BeginReceive(
                           dataBuffer,
                           0,
                           size,
                           SocketFlags.None,
                           new AsyncCallback(PushToTableCallback),
                           mySocket
                        );
                        break;
                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private unsafe byte [] GetBytesFromInt(int myInt)
        {
            byte [] myBytes = new byte[sizeof(int)];
            fixed(byte* ptr = myBytes)
            {
                int* intPtr = (int *)ptr;
                *intPtr = myInt;
            }
            return myBytes;
        }

        private void PullFromTableCallback(IAsyncResult myResult)
        {
            Socket mySock = myResult.AsyncState as Socket;
            int send = mySock.EndSend(myResult);
            Console.WriteLine("Send successfully!" + send);

            //Get the user associated with the socket
            IPEndPoint remoteIPEP = mySock.RemoteEndPoint as IPEndPoint;
            String remoteIP = IPAddress.Parse(remoteIPEP.Address.ToString()).ToString();
            POIUser myUser = POIGlobalVar.UserProfiles[remoteIP];

            //Clear the data buffer for the current socket
            myUser.RemoveTCPDataBuffer(mySock);
            RemoveConnection(mySock);
            mySock.Close();
        }

        private void PushToTableCallback(IAsyncResult myResult)
        {
            Socket mySocket = (Socket)myResult.AsyncState;
            int recv = mySocket.EndReceive(myResult);

            //Get the user associated with the socket
            IPEndPoint remoteIPEP = mySocket.RemoteEndPoint as IPEndPoint;
            String remoteIP = IPAddress.Parse(remoteIPEP.Address.ToString()).ToString();
            POIUser myUser = POIGlobalVar.UserProfiles[remoteIP];

            Tuple<byte[], int> bufferInfo = myUser.GetTCPDataBuffer(mySocket);
            byte[] myBuffer = bufferInfo.Item1;
            int startIndex = bufferInfo.Item2;
            int dataSize = myBuffer.Length;

            startIndex += recv;
            if (dataSize > startIndex)
            {
                myUser.SetTCPDataBuffer(mySocket, myBuffer, startIndex);

                mySocket.ReceiveTimeout = 10000;

                //Console.WriteLine(startIndex + " " + dataSize);

                //int size = Math.Min(1400, dataSize - startIndex);
                int size = dataSize - startIndex;

                try
                {
                    mySocket.BeginReceive(
                        myBuffer,
                        startIndex,
                        size,
                        SocketFlags.None,
                        new AsyncCallback(PushToTableCallback),
                        mySocket
                    );
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                
            }
            else
            {
                //Serialize the buffer data into an image
                Console.WriteLine("Received successfully: " + dataSize);

                //Remember to remove this stupid condition!!
                if(dataSize > 100000)
                    POIGlobalVar.SystemKernel.Handle_UserPushToTable(myBuffer, dataSize);

                //Clear the data buffer for the current socket
                myUser.RemoveTCPDataBuffer(mySocket);
                RemoveConnection(mySocket);
                mySocket.Close();
            }
        }
        */
        private void RemoveConnection(Socket mySocket)
        {
            /*
            //handle the user leave or connection close
            IPEndPoint remoteIPEP = mySocket.RemoteEndPoint as IPEndPoint;
            String remoteIP = IPAddress.Parse(remoteIPEP.Address.ToString()).ToString();
            
            if (connectionCount.ContainsKey(remoteIP) && connectionCount[remoteIP] > 1)
            {
                connectionCount[remoteIP] = connectionCount[remoteIP] - 1;
            }
            else
            {
                //userLeave(this, new POIUserEventArgs(userCollection[remoteIP], new Point(0,0)));
                POIGlobalVar.SystemKernel.Handle_UserLeave(new POIUserEventArgs(userCollection[remoteIP], new Point(0, 0)));
                connectionCount.Remove(remoteIP);
                userCollection.Remove(remoteIP);
            }*/

        }

        private string ReadControlMsg(NetworkStream inputStream)
        {
            int recvedByteCount = 0;
            int maxControlMsgSize = 1000;
            char[] myBuff = new char[maxControlMsgSize];

            while (true)
            {
                //Read in a single character;
                int status = inputStream.ReadByte();
                //Check if the stream has been closed
                if (status == -1) return "";

                char curChar = (char)status;
                if (curChar == '\n')
                {
                    //myBuff[recvedByteCount] = '\0';
                    break;
                }
                else
                {
                    myBuff[recvedByteCount] = curChar;
                    recvedByteCount++;
                }
            }

            string controlMsg = new string(myBuff, 0, recvedByteCount);
            
            return controlMsg;
        }

    }

   

}
