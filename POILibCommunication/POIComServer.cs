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
                    //POIGlobalVar.SystemKernel.Handle_UserLeave(new POIUserEventArgs(user, new Point(0, 0)));
                    POIGlobalVar.SystemKernel.HandleUserLeave(user);
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
                //POIGlobalVar.SystemKernel.Handle_UserJoin(new POIUserEventArgs(user, new Point(0, 0)));
                POIGlobalVar.SystemKernel.HandleUserJoin(user);
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

                    connection.InitPayloadBufferForDataChannel();
                }
            }
            else //Drop the connection
            {
                connection.Disconnect();
            }
        }

        private void RemoveConnection(Socket mySocket)
        {
            
        }

    }

   

}
