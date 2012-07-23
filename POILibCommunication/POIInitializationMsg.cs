using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POILibCommunication
{
    public class POIHelloMsg : POIMessage
    {
        byte userType;
        byte connectionType;

        public byte UserType { get { return userType; } }
        public byte ConnType { get { return connectionType; } }

        const int fieldSize = 2 * sizeof(byte);
        static int size = fieldSize;
        static public int Size { get { return size; } }

        public POIHelloMsg() { }
        public POIHelloMsg(int myUserType, int myConnType)
        {
            userType = (byte)myUserType;
            connectionType = (byte)myConnType;
        }

        public override void deserialize(byte[] buffer, ref int offset)
        {
            deserializeByte(buffer, ref offset, ref userType);
            deserializeByte(buffer, ref offset, ref connectionType);
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            serializeByte(buffer, ref offset, userType);
            serializeByte(buffer, ref offset, connectionType);
        }

        public override byte[] getPacket()
        {
            byte[] packet = new byte[size];
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

        public POIWelcomeMsg() { }
        public POIWelcomeMsg(WelcomeStatus myStatus)
        {
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
            byte[] packet = new byte[size];
            int offset = 0;

            serialize(packet, ref offset);

            return composePacket(POIMsgDefinition.POI_WELCOME, packet);
        }
    }
}
