using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;

using System.Timers;
using System.Net;
using System.Net.Sockets;

namespace POILibCommunication
{
    public enum ConType {TCP_CONTROL, TCP_DATA, UDP};

    public class POIUser : POIMsgDelegateContainer, POIPushPullClientMsgCB
    {
        
        #region Data members

        public POITCPConnection CtrlChannel { get; set; }
        public POITCPConnection DataChannel { get; set; }
        public Socket UdpChannel { get; set; }

        private String secret = null;

        //DataParsers
        //public DataParsers myParser;
        //public POIMsgParser ctrlParser;

        private string ID;

        //Variables for UDP connection
        private IPEndPoint __UDPEndPoint;

        #endregion

        #region properties

        public enum ConnectionStatus
        {
            Disconnected,
            Connected
        }

        public ConnectionStatus Status { get; set; }

        public string UserID { get { return ID; } set { ID = value; } }

        public IPEndPoint UDPEndPoint
        {
            get { return __UDPEndPoint; }
            set { __UDPEndPoint = value; }
        }

        public enum Privilege
        {
            Authentication = 0,
            Viewer,
            Commander
        }

        public Privilege UserPrivilege { get; set; }

        #endregion

        

        #region Control parser callback functions

        

        public void pullMsgReceived(ref PullPar par)
        {
 
        }

        public void pushMsgReceived(ref PushPar par, byte[] data)
        {

        }

        #endregion

        //public Image IconImage;
       

        public POIUser()
        {
            Status = ConnectionStatus.Disconnected;
        }

        

        public void SendData(byte[] myData, ConType channelType)
        {
            switch(channelType)
            {
                case ConType.UDP:
                    if (UdpChannel != null)
                    {
                        UdpChannel.SendTo(myData, UDPEndPoint);
                    }
                    break;

                case ConType.TCP_CONTROL:
                    CtrlChannel.SendData(myData);
                    break;

                case ConType.TCP_DATA:
                    DataChannel.SendData(myData);
                    break;

                default:
                    break;
            }
        }
    }

    
}
