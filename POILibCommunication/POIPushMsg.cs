using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POILibCommunication
{
    public class POIPushMsg : POIMessage
    {
        int type;
        int dataSize;
        byte[] data;

        const int fieldSize = 2*sizeof(int);
        int size;

        public int Type { get { return type; } }
        public int DataSize { get { return dataSize; } }
        public int Size { get { return size; } }

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
        }

        public override void deserialize(byte[] buffer, ref int offset)
        {
            deserializeInt32(buffer, ref offset, ref type);
            deserializeInt32(buffer, ref offset, ref dataSize);

            size = fieldSize + dataSize;

            try
            {
                Array.Copy(buffer, offset, data, 0, dataSize);
                offset += dataSize;
            }
            catch (Exception e)
            {

            }
        }
    }
}
