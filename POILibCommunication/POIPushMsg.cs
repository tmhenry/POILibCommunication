using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Web.Script.Serialization;

namespace POILibCommunication
{
    public class POIPushMsg : POIMessage
    {
        int type;
        int dataSize;
        byte[] data;

        public Dictionary<string, string> info = new Dictionary<string, string>();

        const int fieldSize = 3*sizeof(int);
        int size;

        public int Type { get { return type; } }
        public int DataSize { get { return dataSize; } }
        public int Size { get { return size; } }
        public byte[] Data { get { return data; } }

        public POIPushMsg() { }

        public POIPushMsg(int myType, byte [] myData)
        {
            dataSize = myData.Length;
            type = myType;
            data = myData;

            size = fieldSize + dataSize;
        }

        public override byte[] getPacket()
        {
            byte[] packet = new byte[size];
            int offset = 0;

            serialize(packet, ref offset);

            return composePacket(POIMsgDefinition.POI_PUSH, packet);
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            serializeInt32(buffer, ref offset, type);
            serializeInt32(buffer, ref offset, dataSize);

            Array.Copy(data, 0, buffer, offset, dataSize);
            offset += dataSize;

            //Parse the info dictionary into string
            JavaScriptSerializer jsonParser = new JavaScriptSerializer();
            string infoString = jsonParser.Serialize(info);
            byte[] infoBytes = Encoding.UTF8.GetBytes(infoString);

            //Serialize the info length
            serializeInt32(buffer, ref offset, infoBytes.Length);
            serializeByteArray(buffer, ref offset, infoBytes);
        }

        public override void deserialize(byte[] buffer, ref int offset)
        {
            deserializeInt32(buffer, ref offset, ref type);
            deserializeInt32(buffer, ref offset, ref dataSize);

            size = fieldSize + dataSize;

            try
            {
                data = new byte[dataSize];
                Array.Copy(buffer, offset, data, 0, dataSize);
                offset += dataSize;
            }
            catch (Exception e)
            {

            }

            //Deserialize the info length
            int infoLength = 0;
            deserializeInt32(buffer, ref offset, ref infoLength);
            byte[] infoBytes = buffer.Skip(offset).Take(infoLength).ToArray();
            offset += infoLength;

            string infoString = Encoding.UTF8.GetString(infoBytes);
            JavaScriptSerializer jsonParser = new JavaScriptSerializer();
            info = jsonParser.Deserialize<Dictionary<string, string>>(infoString);
            size += infoLength;
        }
    }
}
