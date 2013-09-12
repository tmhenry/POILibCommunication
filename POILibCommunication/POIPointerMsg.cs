using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POILibCommunication
{
    public class POIPointerMsg : POIMessage
    {
        float x;
        float y;
        double timestamp;

        PointerCtrlType type;
        int size = 2 * sizeof(float) + sizeof(int) + sizeof(double);

        public PointerCtrlType Type
        {
            get { return type; }
        }

        public float X { get { return x; } }
        public float Y { get { return y; } }
        public int Size { get { return size; } }
        public double Timestamp { get { return timestamp; } }

        //Constructor
        public POIPointerMsg()
        {
            messageType = POIMsgDefinition.POI_POINTER_CONTROL;
        }

        //Constructor
        public POIPointerMsg(PointerCtrlType myType, float myX, float myY, double time)
        {
            messageType = POIMsgDefinition.POI_POINTER_CONTROL;
            type = myType;
            x = myX;
            y = myY;
            timestamp = time;

            base.timestamp = time;
        }

        //Serializer
        public override void serialize(byte[] buffer, ref int offset)
        {
            serializeInt32(buffer, ref offset, (int)Type);
            serializeFloat(buffer, ref offset, x);
            serializeFloat(buffer, ref offset, y);
            serializeDouble(buffer, ref offset, timestamp);
        }

        //Deserializer
        public override void deserialize(byte[] buffer, ref int offset)
        {
            int typeInt = 0;
            deserializeInt32(buffer, ref offset, ref typeInt);
            type = (PointerCtrlType) typeInt;

            deserializeFloat(buffer, ref offset, ref x);
            deserializeFloat(buffer, ref offset, ref y);
            deserializeDouble(buffer, ref offset, ref timestamp);

            base.timestamp = timestamp;
        }

        public override byte[] getPacket()
        {
            byte[] packet = new byte[size];
            int offset = 0;
            serialize(packet, ref offset);

            return composePacket(POIMsgDefinition.POI_POINTER_CONTROL, packet);
        }
    }
}
