using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POILibCommunication
{
    public abstract class POITouchData : POIMessage
    {
        float x;
        float y;
        double time;

        public float X { get { return x; } }
        public float Y { get { return y; } }
        public double Time { get { return time; } }

        const int fieldSize = 2 * sizeof(float) + sizeof(double);
        static int size = fieldSize;
        static public int Size { get { return size; } }

        public override void deserialize(byte[] buffer, ref int offset)
        {
            deserializeFloat(buffer, ref offset, ref x);
            deserializeFloat(buffer, ref offset, ref y);
            deserializeDouble(buffer, ref offset, ref time);
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            serializeFloat(buffer, ref offset, x);
            serializeFloat(buffer, ref offset, y);
            serializeDouble(buffer, ref offset, time);
        }
    }

    public class POITouchBegin : POITouchData
    {
        const int fieldSize = 0;
        static int size = POITouchData.Size + fieldSize;
        static new public int Size { get { return size; } }

        public override byte[] getPacket()
        {
            byte[] packet = new byte[size];
            int offset = 0;

            serialize(packet, ref offset);

            return composePacket(POIMsgDefinition.POI_TOUCHBEGIN, packet);
        }
    }

    public class POITouchEnd : POITouchData
    {
        int numOfTaps;

        const int fieldSize = sizeof(int);
        static int size = POITouchData.Size + fieldSize;
        static new public int Size { get { return size; } }

        public override void deserialize(byte[] buffer, ref int offset)
        {
            base.deserialize(buffer, ref offset);
            deserializeInt32(buffer, ref offset, ref numOfTaps);
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            base.serialize(buffer, ref offset);
            serializeInt32(buffer, ref offset, numOfTaps);
        }

        public override byte[] getPacket()
        {
            byte[] packet = new byte[size];
            int offset = 0;
            serialize(packet, ref offset);

            return composePacket(POIMsgDefinition.POI_TOUCHEND, packet);
        }
    }

    public class POITouchMove : POITouchData
    {
        const int fieldSize = 0;
        static int size = POITouchData.Size + fieldSize;
        static new public int Size { get { return size; } }

        public override byte[] getPacket()
        {
            byte[] packet = new byte[size];
            int offset = 0;

            serialize(packet, ref offset);

            return composePacket(POIMsgDefinition.POI_TOUCHMOVE, packet);
        }
    }

    public class POIScale : POIMessage
    {
        float scaleFactor;
        float velocity;
        double time;

        public float ScaleFactor { get { return scaleFactor; } }
        public float Velocity { get { return velocity; } }
        public double Time { get { return time; } }

        const int fieldSize = 2 * sizeof(float) + sizeof(double);
        static int size = fieldSize;
        static public int Size { get { return size; } }

        public override void deserialize(byte[] buffer, ref int offset)
        {
            deserializeFloat(buffer, ref offset, ref scaleFactor);
            deserializeFloat(buffer, ref offset, ref velocity);
            deserializeDouble(buffer, ref offset, ref time);
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            serializeFloat(buffer, ref offset, scaleFactor);
            serializeFloat(buffer, ref offset, velocity);
            serializeDouble(buffer, ref offset, time);
        }

        public override byte[] getPacket()
        {
            byte[] packet = new byte[size];
            int offset = 0;

            serialize(packet, ref offset);

            return composePacket(POIMsgDefinition.POI_SCALE, packet);
        }
    }

    public class POIRotate : POIMessage
    {
        float degree;
        float velocity;
        double time;

        public float Degree { get { return degree; } }
        public float Velocity { get { return velocity; } }
        public double Time { get { return time; } }

        const int fieldSize = 2 * sizeof(float) + sizeof(double);
        static int size = fieldSize;
        static public int Size { get { return size; } }

        public override void deserialize(byte[] buffer, ref int offset)
        {
            deserializeFloat(buffer, ref offset, ref degree);
            deserializeFloat(buffer, ref offset, ref velocity);
            deserializeDouble(buffer, ref offset, ref time);
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            serializeFloat(buffer, ref offset, degree);
            serializeFloat(buffer, ref offset, velocity);
            serializeDouble(buffer, ref offset, time);
        }

        public override byte[] getPacket()
        {
            byte[] packet = new byte[size];
            int offset = 0;

            serialize(packet, ref offset);

            return composePacket(POIMsgDefinition.POI_ROTATE, packet);
        }
    }
}
