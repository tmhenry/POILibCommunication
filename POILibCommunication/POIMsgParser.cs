using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Net;

namespace POILibCommunication
{

    public class POIMsgParser
    {
        public POIMsgDelegateContainer Delegates { get; set; }
        public POIInitializeClientMsgCB initClientMsgDelegate { get; set; }


        public POIUser AssociatedUser { get; set; }

        public enum Privilege
        {
            Authentication = 0,
            Viewer,
            Commander
        }

        public enum ParserType
        {
            Control = 0,
            Data,
            RealTime
        }

        public Privilege privilegeLevel { get; set; }
        public ParserType Type { get; set; }

        public POIMsgParser(Privilege level = Privilege.Authentication)
        {
            privilegeLevel = level;
        }

        #region Serialize and deserialize

        private void serializeInt32(byte[] buffer, ref int offset, Int32 val)
        {
            Int32 temp = IPAddress.HostToNetworkOrder(val);
            Array.Copy(BitConverter.GetBytes(temp), 0, buffer, offset, sizeof(Int32));

            offset += sizeof(Int32);
        }

        private void deserializeInt32(byte[] buffer, ref int offset, ref Int32 val)
        {
            val = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, offset));

            offset += sizeof(Int32);
        }

        private void serializeByte(byte[] buffer, ref int offset, byte val)
        {
            buffer[offset] = val;
            offset += 1;
        }

        private void deserializeByte(byte[] buffer, ref int offset, ref byte val)
        {
            val = buffer[offset];
            offset += 1;
        }

        private void serializeFloat(byte[] buffer, ref int offset, float val)
        {
            Array.Copy(BitConverter.GetBytes(val), 0, buffer, offset, sizeof(float));
            offset += sizeof(float);
        }

        private void deserializeFloat(byte[] buffer, ref int offset, ref float val)
        {
            val = BitConverter.ToSingle(buffer, offset);
            offset += sizeof(float);
        }

        private void serializeDouble(byte [] buffer, ref int offset, double val)
        {
            Array.Copy(BitConverter.GetBytes(val), 0, buffer, offset, sizeof(double));
            offset += sizeof(double);
        }

        private void deserializeDouble(byte[] buffer, ref int offset, ref double val)
        {
            val = BitConverter.ToDouble(buffer, offset);
            offset += sizeof(double);
        }

        #endregion

        public void parsePacket(byte[] data)
        {
           
            //Get the byte pointer
            int offset = 0;
            byte cmdType = 0;
            deserializeByte(data, ref offset, ref cmdType);

            POIGlobalVar.POIDebugLog("Current type is: " + cmdType);

            //Get the timestamp of the packet


            switch(cmdType)
            {
                //Initialization:
                case POIMsgDefinition.POI_HELLO:
                    parseHelloMsg(data, offset);
                    break;
                case POIMsgDefinition.POI_WELCOME:
                    parseWelcomeMsg(data, offset);
                    break;

   

                case POIMsgDefinition.POI_PRESENTATION_CONTROL:
                    parsePresControlMsg(data, offset);
                    break;

                case POIMsgDefinition.POI_PRESENTATION_CONTENT:
                    parsePresContentMsg(data, offset);
                    break;

                case POIMsgDefinition.POI_USER_COMMENTS:
                    parseUserComments(data, offset);
                    break;

                case POIMsgDefinition.POI_WHITEBOARD_CONTROL:
                    parseWhiteboardCtrlMsg(data, offset);
                    break;

                case POIMsgDefinition.POI_SESSION_CONTROL:
                    parseSessionCtrlMsg(data, offset);
                    break;

                case POIMsgDefinition.POI_POINTER_CONTROL:
                    parsePointerCtrlMsg(data, offset);
                    break;

            }
                              
        }

        public byte[] composePacket(int type, byte[] parameters)
        {
            int packetLength = 1 + parameters.Length;
            byte[] myBytes = new byte[packetLength];

            int offset = 0;
            serializeByte(myBytes, ref offset, (byte)type);

            Array.Copy(parameters, 0, myBytes, offset, parameters.Length);

            return myBytes;
        }

        public byte[] composePacket(int type, byte[] parameters, byte[] data)
        {
            int packetLength = 1 + parameters.Length + data.Length;
            byte[] myBytes = new byte[packetLength];

            int offset = 0;
            serializeByte(myBytes, ref offset, (byte)type);

            Array.Copy(parameters, 0, myBytes, offset, parameters.Length);
            offset += parameters.Length;

            Array.Copy(data, 0, myBytes, offset, data.Length);

            return myBytes;
        }

        #region Broadcast msg parsing and composition methods

        private void parseBroadcastRequestMissingPacketMsg(byte[] buffer, int offset)
        {
            if (privilegeLevel < Privilege.Viewer) return;

            BroadcastRequestMissingPacketPar par = new BroadcastRequestMissingPacketPar();
            deserializeInt32(buffer, ref offset, ref par.frameNum);
            deserializeInt32(buffer, ref offset, ref par.seqNumStart);
            deserializeInt32(buffer, ref offset, ref par.seqNumEnd);

            Delegates.BroadcastCtrlHandler.broadcastRequestMissingPacketMsgReceived(ref par);
        }

        private void parseBroadcastBeginAckMsg(byte[] buffer, int offset)
        {
            if (privilegeLevel < Privilege.Viewer) return;

            BroadcastBeginAckPar par = new BroadcastBeginAckPar();
            deserializeInt32(buffer, ref offset, ref par.frameNum);

            Delegates.BroadcastCtrlHandler.broadcastBeginAckMsgReceived(ref par);
        }

        private void parseBroadcastEndAckMsg(byte[] buffer, int offset)
        {
            if (privilegeLevel < Privilege.Viewer) return;

            BroadcastEndAckPar par = new BroadcastEndAckPar();
            deserializeInt32(buffer, ref offset, ref par.frameNum);

            Delegates.BroadcastCtrlHandler.broadcastEndAckMsgReceived(ref par);
        }

        
        public byte[] getBroadcastBeginMsg(ref BroadcastBeginPar par)
        {
            byte[] parBytes = new byte[POIMsgDefinition.POI_MAXPARAMETERSSIZE];

            int offset = 0;

            serializeInt32(parBytes, ref offset, par.frameNum);
            serializeInt32(parBytes, ref offset, par.numPackets);
            serializeInt32(parBytes, ref offset, par.packetPayload);
            serializeInt32(parBytes, ref offset, par.lastPacketPayload);

            parBytes = parBytes.Take(offset).ToArray();
            return composePacket(POIMsgDefinition.POI_BROADCASTBEGIN, parBytes);
        }

        public byte[] getBroadcastEndMsg(ref BroadcastEndPar par)
        {
            byte[] parBytes = new byte[POIMsgDefinition.POI_MAXPARAMETERSSIZE];

            int offset = 0;
            serializeInt32(parBytes, ref offset, par.frameNum);

            parBytes = parBytes.Take(offset).ToArray();
            return composePacket(POIMsgDefinition.POI_BROADCASTEND, parBytes);
        }

        public byte[] getBroadcastCotentMsg(ref BroadcastContentPar par, byte[] data)
        {
            byte[] parBytes = new byte[POIMsgDefinition.POI_MAXPARAMETERSSIZE];

            int offset = 0;
            serializeInt32(parBytes, ref offset, par.frameNum);
            serializeInt32(parBytes, ref offset, par.seqNum);

            parBytes = parBytes.Take(offset).ToArray();
            return composePacket(POIMsgDefinition.POI_BROADCASTCONTENT, parBytes, data);
        }

        

        

        #endregion

        private void parseHelloMsg(byte[] buffer, int offset)
        {
            POIHelloMsg msg = new POIHelloMsg();
            msg.deserialize(buffer, ref offset);

            POIGlobalVar.POIDebugLog(@"Hello");

            try
            {
                initClientMsgDelegate.helloMsgReceived(msg, this as POITCPConnection);
            }
            catch
            {
                POIGlobalVar.POIDebugLog("Error in calling hello msg callback!");
            }
            
        }

        private void parseWelcomeMsg(byte[] buffer, int offset)
        {
            POIWelcomeMsg msg = new POIWelcomeMsg();
            msg.deserialize(buffer, ref offset);

            POIGlobalVar.POIDebugLog(@"Welcome");
        }

        private void parsePushMsg(byte[] buffer, int offset)
        {
            POIGlobalVar.POIDebugLog("Here in push!");
            if (privilegeLevel < Privilege.Viewer || Type != ParserType.Data) return;


            POIPushMsg msg = new POIPushMsg();
            msg.deserialize(buffer, ref offset);

            POIGlobalVar.POIDebugLog("Type: " + msg.Type);
        }

        private void parsePresControlMsg(byte[] buffer, int offset)
        {
            if (AssociatedUser != null && AssociatedUser.UserPrivilege >= POIUser.Privilege.Authentication)
            {
                POIPresCtrlMsg msg = new POIPresCtrlMsg();
                msg.deserialize(buffer, ref offset);

                try
                {
                    Delegates.PresCtrlHandler.presCtrlMsgReceived(msg);
                }
                catch
                {
                    POIGlobalVar.POIDebugLog("No proper handler registered for presentation control message.");
                }
            }
            else
            {
                POIGlobalVar.POIDebugLog("Not proper privilege for the user!");
            }

            
            
        }

        private void parsePresContentMsg(byte[] buffer, int offset)
        {
            POIPresentation presentation = new POIPresentation();
            presentation.deserialize(buffer, ref offset);
        }

        private void parseUserComments(byte[] buffer, int offset)
        {
            POIComment comment = new POIComment();
            comment.deserialize(buffer, ref offset);

            try
            {
                Delegates.CommentHandler.handleComment(comment);
            }
            catch
            {
                POIGlobalVar.POIDebugLog("No proper delegate for user comment!");
            }
        }

        private void parseWhiteboardCtrlMsg(byte[] buffer, int offset)
        {
            POIWhiteboardMsg msg = new POIWhiteboardMsg();
            msg.deserialize(buffer, ref offset);

            try
            {
                Delegates.WhiteboardCtrlHandler.whiteboardCtrlMsgReceived(msg);
            }
            catch
            {
                POIGlobalVar.POIDebugLog("No proper delegate for whiteboard control.");
            }
        }

        private void parseWhiteboardShow(byte[] buffer, int offset)
        {
            POIShowWhiteboardMsg msg = new POIShowWhiteboardMsg();
            msg.deserialize(buffer, ref offset);

            Delegates.WhiteboardCtrlHandler.showWhiteBoard();
        }

        private void parseWhiteboardHide(byte[] buffer, int offset)
        {
            POIHideWhiteboardMsg msg = new POIHideWhiteboardMsg();
            msg.deserialize(buffer, ref offset);

            Delegates.WhiteboardCtrlHandler.hideWhiteBoard();
        }

        private void parseSessionCtrlMsg(byte[] buffer, int offset)
        {
            POISessionMsg msg = new POISessionMsg();
            msg.deserialize(buffer, ref offset);

            try
            {
                Delegates.SessionHandler.sessionCtrlMsgReceived(msg, AssociatedUser);
            }
            catch
            {
                POIGlobalVar.POIDebugLog("No proper delegate for session control.");
            }
        }

        private void parsePointerCtrlMsg(byte[] buffer, int offset)
        {
            POIPointerMsg msg = new POIPointerMsg();
            msg.deserialize(buffer, ref offset);

            try
            {
                //Pass the data to the pointer handler
                Delegates.PointerHandler.pointerCtrlMsgReceived(msg);
            }
            catch
            {
                POIGlobalVar.POIDebugLog("No proper delegate for pointer control.");
            }
        }

        private void parseAudioContentMsg(byte[] buffer, int offset)
        {
            POIAudioContentMsg msg = new POIAudioContentMsg();
            msg.deserialize(buffer, ref offset);

            try
            {
                //Pass the data to the pointer handler
                Delegates.AudioContentHandler.audioContentMsgReceived(msg);
            }
            catch
            {
                POIGlobalVar.POIDebugLog("No proper delegate for audio content.");
            }
        }

    }
}
