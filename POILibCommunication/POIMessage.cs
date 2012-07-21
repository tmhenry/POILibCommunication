using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.Net;

namespace POILibCommunication
{
    abstract public class POISerializable
    {
        abstract public void serialize(byte[] buffer, ref int offset);
        abstract public void deserialize(byte[] buffer, ref int offset);
        [StructLayout(LayoutKind.Explicit)]
        struct doubleUnion
        {
            [System.Runtime.InteropServices.FieldOffset(0)]
            public double sv;
            [System.Runtime.InteropServices.FieldOffset(0)]
            public int u1;
            [System.Runtime.InteropServices.FieldOffset(4)]
            public int u2;
        };

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
            int temp = BitConverter.ToInt32(BitConverter.GetBytes(val), 0);
            temp = IPAddress.HostToNetworkOrder(temp);
          
            Array.Copy(BitConverter.GetBytes(temp), 0, buffer, offset, size);
            offset += size;
        }

        protected void deserializeFloat(byte[] buffer, ref int offset, ref float val)
        {
            int temp = BitConverter.ToInt32(buffer, offset);
            temp = IPAddress.NetworkToHostOrder(temp);
            val = BitConverter.ToSingle(BitConverter.GetBytes(temp), 0);

            offset += sizeof(float);
        }

        protected void serializeDouble(byte[] buffer, ref int offset, double val)
        {
            doubleUnion temp = new doubleUnion();
            doubleUnion result = new doubleUnion();
            temp.sv = val;

            result.u1 = IPAddress.HostToNetworkOrder(temp.u2);
            result.u2 = IPAddress.HostToNetworkOrder(temp.u1);

            Array.Copy(BitConverter.GetBytes(result.sv), 0, buffer, offset, sizeof(double));
            offset += sizeof(double);
        }

        protected void deserializeDouble(byte[] buffer, ref int offset, ref double val)
        {
            double doubleValue = BitConverter.ToDouble(buffer,offset);
            doubleUnion tmp = new doubleUnion();
            doubleUnion result = new doubleUnion();

            tmp.sv = doubleValue;

            result.u1 = IPAddress.NetworkToHostOrder(tmp.u2);
            result.u2 = IPAddress.NetworkToHostOrder(tmp.u1);

            val = result.sv;

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
