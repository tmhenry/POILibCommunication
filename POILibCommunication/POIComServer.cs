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
    public class POIComServer : POIInitializeClientMsgCB, POITCPConnectionCBDelegate
    {
        private static object broadCastLock = new object();

        static byte[] myBuffer = new byte[1400];
        Socket listener;

        int maxClientCount = POIGlobalVar.MaxMobileClientCount;

        public POIBroadcast BroadcastChannel { get; set; }
        public POIUDPReceiver UDPServer { get; set; }

        #region Callback function for TCP connections

        public void ConnectionEnded(POITCPConnection connection)
        {
            POIUser user = connection.AssociatedUser;

            if (user != null)
            {
                if (connection.Type == POIMsgParser.ParserType.Control)
                {
                    POIGlobalVar.POIDebugLog("Connection ended by user!");

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
                POIGlobalVar.POIDebugLog(@"Error: no user existed!");
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
            POIGlobalVar.POIDebugLog(@"Accepting");

            //Start authenticating the new connection
            POITCPConnection connection = new POITCPConnection(e.AcceptSocket);
            
            connection.initClientMsgDelegate = this;
            connection.connectionCBDelegate = this;
            connection.StartReceiving();

          
            //If there is still room for new clients, start another round of accepting.
            if (POIGlobalVar.UserProfiles.Count <= maxClientCount)
            {
                try
                {
                    StartAcceptNewClient();
                }
                catch (Exception error)
                {
                    POIGlobalVar.POIDebugLog(error.Message);
                }
            }
        }

        //Initialize the client
        public void helloMsgReceived(POIHelloMsg par, POITCPConnection connection)
        {
            //Notify data handler that authentication has been done
            //Proper CB functions are set here
            int userType = (int)par.UserType;
            int conType = (int)par.ConnType;
            string userName = par.UserName;

            //Handle the authentication

            POIWelcomeMsg.WelcomeStatus status = POIWelcomeMsg.WelcomeStatus.Failed;

            if (conType == POIMsgDefinition.POI_CONTROL_CHANNEL)
            {
                //Check if user already exists, create if not exists
                POIUser user;
                if (POIGlobalVar.UserProfiles.ContainsKey(userName))
                {
                    user = POIGlobalVar.UserProfiles[userName];
                }
                else
                {
                    user = new POIUser();
                    POIGlobalVar.UserProfiles[userName] = user;
                }

                //Initialize the user parameters
                user.UserID = userName;
                user.UserPrivilege = (POIUser.Privilege)userType;
                user.CtrlChannel = connection;

                if (user.Status == POIUser.ConnectionStatus.Disconnected)
                {
                    user.Status = POIUser.ConnectionStatus.Connected;
                }
                
                connection.Type = POIMsgParser.ParserType.Control;
                connection.Delegates = user;
                connection.AssociatedUser = user;

                try
                {
                    POIGlobalVar.SystemKernel.HandleUserJoin(user);
                }
                catch (Exception e)
                {
                    POIGlobalVar.POIDebugLog(e.Message);
                }

                status = POIWelcomeMsg.WelcomeStatus.CtrlChannelAuthenticated;
            }
            else if (conType == POIMsgDefinition.POI_DATA_CHANNEL)
            {
                if (POIGlobalVar.UserProfiles.ContainsKey(userName))
                {
                    POIUser user = POIGlobalVar.UserProfiles[userName];
                    if (user.Status == POIUser.ConnectionStatus.Connected)
                    {
                        user.DataChannel = connection;

                        connection.InitPayloadBufferForDataChannel();
                        connection.Type = POIMsgParser.ParserType.Data;
                        connection.Delegates = user;
                        connection.AssociatedUser = user;

                        status = POIWelcomeMsg.WelcomeStatus.DataChannelAuthenticated;
                    }
                }
            }

            //Send back the welcome message
            POIWelcomeMsg welcomeMsg = new POIWelcomeMsg(status);
            connection.SendData(welcomeMsg.getPacket());
        }


        private void RemoveConnection(Socket mySocket)
        {
            
        }

    }

   

}
