using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;

namespace POILibCommunication
{
    abstract public class POISerializable
    {
        abstract public void serialize(byte[] buffer, ref int offset);
        abstract public void deserialize(byte[] buffer, ref int offset);

        #region Serialize and deserialize

        protected void serializeInt32(byte[] buffer, ref int offset, Int32 val)
        {
            Int32 temp = IPAddress.HostToNetworkOrder(val);
            Array.Copy(BitConverter.GetBytes(temp), 0, buffer, offset, sizeof(Int32));

            offset += sizeof(Int32);
        }

        protected void deserializeInt32(byte[] buffer, ref int offset, ref Int32 val)
        {
            val = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(buffer, offset));

            offset += sizeof(Int32);
        }

        protected void serializeByte(byte[] buffer, ref int offset, byte val)
        {
            buffer[offset] = val;
            offset += 1;
        }

        protected void deserializeByte(byte[] buffer, ref int offset, ref byte val)
        {
            val = buffer[offset];
            offset += 1;
        }

        protected void serializeFloat(byte[] buffer, ref int offset, float val)
        {
            int size = sizeof(float);
            int temp = IPAddress.HostToNetworkOrder((int)val);
          
            Array.Copy(BitConverter.GetBytes(temp), 0, buffer, offset, size);
            offset += size;
        }

        protected void deserializeFloat(byte[] buffer, ref int offset, ref float val)
        {
            int size = sizeof(float);
            int temp = BitConverter.ToInt32(buffer, offset);
            val = (float) IPAddress.NetworkToHostOrder(temp);

            offset += size;
        }

        protected void serializeDouble(byte[] buffer, ref int offset, double val)
        {
            Array.Copy(BitConverter.GetBytes(val), 0, buffer, offset, sizeof(double));
            offset += sizeof(double);
        }

        protected void deserializeDouble(byte[] buffer, ref int offset, ref double val)
        {
            val = BitConverter.ToDouble(buffer, offset);
            offset += sizeof(double);
        }

        #endregion
    }

    abstract public class POIMessage : POISerializable
    {
        public abstract byte[] getPacket();

        protected byte[] composePacket(int type, byte[] data)
        {

            int packetLength = 1 + data.Length;
            byte[] myBytes = new byte[packetLength];

            int offset = 0;
            serializeByte(myBytes, ref offset, (byte)type);
            Array.Copy(data, 0, myBytes, offset, data.Length);

            return myBytes;
        }
        
    }
}
