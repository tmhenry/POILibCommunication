using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web.Script.Serialization;

namespace POILibCommunication
{
    public class POISessionMsg : POIMessage
    {
        //Data members
        Dictionary<string, string> info = new Dictionary<string, string>();
        bool sizeChanged = true;
        int size = 0;
        const int fieldSize = sizeof(Int32);

        //Properties
        public int CtrlType 
        {
            get { return Int32.Parse(info[@"CtrlType"]); }
            set 
            { 
                info[@"CtrlType"] = value.ToString();
                sizeChanged = true;
            }
        }

        public int ContentId
        {
            get { return Int32.Parse(info[@"ContentId"]); }
            set 
            { 
                info[@"ContentId"] = value.ToString();
                sizeChanged = true;
            }
        }

        public int SessionId
        {
            get { return Int32.Parse(info[@"SessionId"]); }
            set 
            { 
                info[@"SessionId"] = value.ToString();
                sizeChanged = true;
            }
        }

        public int Size 
        {
            get
            {
                if (sizeChanged) updateSize();
                return size;
            }
        }

        public POISessionMsg()
        {
            messageType = POIMsgDefinition.POI_SESSION_CONTROL;
            initInfoDictionary();
            updateSize();
        }

        public void initSessionStartMsg()
        {
            CtrlType = (int)SessionCtrlType.Start;
        }

        public void initSessionEndMsg()
        {
            CtrlType = (int)SessionCtrlType.End;
        }

        public void initSessionJoinMsg(int sessionId)
        {
            CtrlType = (int)SessionCtrlType.Join;
            SessionId = sessionId;
        }

        public void initSessionCreatedMsg(int sessionId)
        {
            CtrlType = (int)SessionCtrlType.Created;
            SessionId = sessionId;
        }

        private void initInfoDictionary()
        {
            info.Add(@"CtrlType", @"-1");
            info.Add(@"ContentId", @"-1");
            info.Add(@"SessionId", @"-1");
        }

        private void updateSize()
        {
            size = fieldSize;

            JavaScriptSerializer jsonParser = new JavaScriptSerializer();
            string infoString = jsonParser.Serialize(info);
            byte[] infoBytes = Encoding.UTF8.GetBytes(infoString);
            size += infoBytes.Length;

            sizeChanged = false;
        }

        public override void deserialize(byte[] buffer, ref int offset)
        {
            size = fieldSize;

            //Deserialize the info length
            int infoLength = 0;
            deserializeInt32(buffer, ref offset, ref infoLength);
            byte[] infoBytes = buffer.Skip(offset).Take(infoLength).ToArray();
            offset += infoLength;

            string infoString = Encoding.UTF8.GetString(infoBytes);
            JavaScriptSerializer jsonParser = new JavaScriptSerializer();
            info = jsonParser.Deserialize<Dictionary<string, string>>(infoString);
            size += infoLength;

            sizeChanged = false;
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            JavaScriptSerializer jsonParser = new JavaScriptSerializer();
            string infoString = jsonParser.Serialize(info);
            byte[] infoBytes = Encoding.UTF8.GetBytes(infoString);

            //Serialize the info length
            serializeInt32(buffer, ref offset, infoBytes.Length);
            serializeByteArray(buffer, ref offset, infoBytes);
        }

        public override byte[] getPacket()
        {
            byte[] packet = new byte[Size];
            int offset = 0;
            serialize(packet, ref offset);

            return composePacket(POIMsgDefinition.POI_SESSION_CONTROL, packet);
        }
    }

    
}
