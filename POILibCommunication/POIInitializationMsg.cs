using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POILibCommunication
{
    public class POIHelloMsg : POIMessage
    {
        //Data members
        byte userType;
        byte connectionType;
        string userName;

        bool sizeChanged = true;

        //Properties
        public byte UserType { get { return userType; } }
        public byte ConnType { get { return connectionType; } }
        public string UserName { get { return userName; } }

        const int fieldSize = 2 * sizeof(byte) + sizeof(Int32);
        int size = fieldSize;
        public int Size 
        { 
            get 
            {
                if (sizeChanged) updateSize();
                return size; 
            } 
        }

        public POIHelloMsg() 
        {
            messageType = POIMsgDefinition.POI_HELLO;
        }
        public POIHelloMsg(int myUserType, int myConnType, string myUserName)
        {
            messageType = POIMsgDefinition.POI_HELLO;
            userType = (byte)myUserType;
            connectionType = (byte)myConnType;
            userName = myUserName;

            updateSize();
        }

        private void updateSize()
        {
            size = fieldSize;
            size += Encoding.UTF8.GetByteCount(userName);
            sizeChanged = false;
        }

        public override void deserialize(byte[] buffer, ref int offset)
        {
            size = fieldSize;

            deserializeByte(buffer, ref offset, ref userType);
            deserializeByte(buffer, ref offset, ref connectionType);

            int length = 0;
            deserializeInt32(buffer, ref offset, ref length);
            byte[] stringData = buffer.Skip(offset).Take(length).ToArray();
            offset += length;
            size += length;

            userName = Encoding.UTF8.GetString(stringData);

            sizeChanged = false;
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            serializeByte(buffer, ref offset, userType);
            serializeByte(buffer, ref offset, connectionType);

            byte[] stringData = Encoding.UTF8.GetBytes(userName);
            serializeInt32(buffer, ref offset, stringData.Length);
            serializeByteArray(buffer, ref offset, stringData);
        }

        public override byte[] getPacket()
        {
            byte[] packet = new byte[Size];
            int offset = 0;

            serialize(packet, ref offset);

            return composePacket(POIMsgDefinition.POI_HELLO, packet);
        }

    }

    public class POIWelcomeMsg : POIMessage
    {
        public enum WelcomeStatus
        {
            Failed = 0,
            CtrlChannelAuthenticated,
            DataChannelAuthenticated
        }

        WelcomeStatus status;

        public WelcomeStatus Status { get { return status; } }

        const int fieldSize = sizeof(int);
        static int size = fieldSize;
        static public int Size { get { return size; } }

        public POIWelcomeMsg() 
        {
            messageType = POIMsgDefinition.POI_WELCOME;
        }

        public POIWelcomeMsg(WelcomeStatus myStatus)
        {
            messageType = POIMsgDefinition.POI_WELCOME;
            status = myStatus;
        }

        public override void deserialize(byte[] buffer, ref int offset)
        {
            int statusInt = 0;
            deserializeInt32(buffer, ref offset, ref statusInt);
            status = (WelcomeStatus)statusInt;
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            serializeInt32(buffer, ref offset, (int)status);
        }

        public override byte[] getPacket()
        {
            byte[] packet = new byte[Size];
            int offset = 0;

            serialize(packet, ref offset);

            return composePacket(POIMsgDefinition.POI_WELCOME, packet);
        }
    }
}
