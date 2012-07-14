using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media;

namespace POILibCommunication
{
    public class POITextComment : POISerializable
    {
        int depth;
        OperationMode mode;
        int length;
        float x;
        float y;
        String msg;

        public enum OperationMode
        {
            CREATE = 0,
            REMOVE,
            TEXT_CHANGED,
            POPOUT,
            COLLAPSE
        }

        int size;
        int fieldSize;

        public int Depth { get { return depth; } }
        public OperationMode Mode { get { return mode; } }
        public float X { get { return x; } }
        public float Y { get { return y; } }
        public int Size { get { return size; } }
        public String Msg { get { return msg; } }

        public POITextComment() { }

        public POITextComment(int myDepth, OperationMode myMode)
        {
            depth = myDepth;
            mode = myMode;
        }

        public POITextComment(int myDepth, float myX, float myY, String myMsg)
        {
            depth = myDepth;
            x = myX;
            y = myY;

            mode = OperationMode.CREATE;

            msg = myMsg;
            System.Text.Encoding encoding = new System.Text.UTF8Encoding();
            length = encoding.GetByteCount(msg);

            fieldSize = 2 * sizeof(float) + 3 * sizeof(int);
            size = fieldSize + length;
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            System.Text.Encoding encoding = new System.Text.UTF8Encoding();
            byte[] stringdata = encoding.GetBytes(msg);

            serializeInt32(buffer, ref offset, depth);
            serializeInt32(buffer, ref offset, (int)mode);

            if (mode == OperationMode.CREATE)
            {
                serializeInt32(buffer, ref offset, length);
                serializeFloat(buffer, ref offset, x);
                serializeFloat(buffer, ref offset, y);

                Array.Copy(stringdata, 0, buffer, offset, length);
                offset += length;
            }
            else if(mode == OperationMode.TEXT_CHANGED)
            {
                serializeInt32(buffer, ref offset, length);

                Array.Copy(stringdata, 0, buffer, offset, length);
                offset += length;
            }
        }

        public override void deserialize(byte[] buffer, ref int offset)
        {
            //Deserialize each field
            deserializeInt32(buffer, ref offset, ref depth);
            
            int modeInt = 0;
            deserializeInt32(buffer, ref offset, ref modeInt);
            mode = (OperationMode)modeInt;

            if (mode == OperationMode.CREATE)
            {
                deserializeInt32(buffer, ref offset, ref length);
                deserializeFloat(buffer, ref offset, ref x);
                deserializeFloat(buffer, ref offset, ref y);

                byte[] stringData = buffer.Skip(offset).Take(length).ToArray();
                offset += length;

                System.Text.Encoding encoding = new System.Text.UTF8Encoding();
                msg = encoding.GetString(stringData);

                fieldSize = 2 * sizeof(float) + 3 * sizeof(int);
                size = fieldSize + length;
            }
            else if (mode == OperationMode.TEXT_CHANGED)
            {
                deserializeInt32(buffer, ref offset, ref length);
                byte[] stringData = buffer.Skip(offset).Take(length).ToArray();
                offset += length;

                System.Text.Encoding encoding = new System.Text.UTF8Encoding();
                msg = encoding.GetString(stringData);

                fieldSize = 3 * sizeof(int);
                size = fieldSize + length;
            }
        }

    }

    public class POIBeizerPathPoint : POISerializable
    {
        float x;
        float y;
        float time;

        static int size = 3 * sizeof(float);
        DateTime referenceTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        public float X { get { return x; } }
        public float Y { get { return y; } }
        public float Time { get { return time; } }
        static public int Size { get { return size; } }

        public POIBeizerPathPoint() { }

        public POIBeizerPathPoint(float myX, float myY)
        {
            x = myX;
            y = myY;
        }

        public POIBeizerPathPoint(float myX, float myY, DateTime timeStamp)
        {
            x = myX;
            y = myY;

            time = (float) (timeStamp - referenceTime).TotalMilliseconds;
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            serializeFloat(buffer, ref offset, x);
            serializeFloat(buffer, ref offset, y);
            serializeFloat(buffer, ref offset, time);
        }

        public override void deserialize(byte[] buffer, ref int offset)
        {
            deserializeFloat(buffer, ref offset, ref x);
            deserializeFloat(buffer, ref offset, ref y);
            deserializeFloat(buffer, ref offset, ref time);
        }
    }

    public class POIBeizerPath : POISerializable
    {
        int numPoints = 0;
        int depth;
        Color color;
        int shape;
        List<POIBeizerPathPoint> points = new List<POIBeizerPathPoint>();

        int size;
        int fieldSize = 4 * sizeof(int);

        public int Depth { get { return depth; } }
        public int NumPoints { get { return numPoints; } }
        public Color Color { get { return color; } }
        public int Shape { get { return shape; } }
        public int Size { get { return size; } }
        public List<POIBeizerPathPoint> Points { get { return points; } }

        public POIBeizerPath()
        {
            size = fieldSize;
        }

        public void insert(POIBeizerPathPoint point)
        {
            Points.Add(point);
            numPoints++;

            size += POIBeizerPathPoint.Size;
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            serializeInt32(buffer, ref offset, depth);
           

            serializeByte(buffer, ref offset, color.R);
            serializeByte(buffer, ref offset, color.G);
            serializeByte(buffer, ref offset, color.B);
            
            serializeInt32(buffer, ref offset, shape);
            serializeInt32(buffer, ref offset, numPoints);

            for (int i = 0; i < numPoints; i++)
            {
                points[i].serialize(buffer, ref offset);
            }
        }

        public override void deserialize(byte[] buffer, ref int offset)
        {
            deserializeInt32(buffer, ref offset, ref depth);
            

            byte r=0, g=0, b=0;
            deserializeByte(buffer, ref offset, ref r);
            deserializeByte(buffer, ref offset, ref g);
            deserializeByte(buffer, ref offset, ref b);
            color = Color.FromRgb(r, g, b);

            deserializeInt32(buffer, ref offset, ref shape);
            deserializeInt32(buffer, ref offset, ref numPoints);
            

            size = numPoints * POIBeizerPathPoint.Size + fieldSize;

            //Deserialize all the points
            points = new List<POIBeizerPathPoint>();

            for (int i = 0; i < numPoints; i++)
            {
                POIBeizerPathPoint point = new POIBeizerPathPoint();
                point.deserialize(buffer, ref offset);
                points.Add(point);
            }

        }

    }

    public class POIComment : POIMessage
    {
        int frameNum = 0;
        int numText = 0;
        int numBeizerPath = 0;

        List<POIBeizerPath> paths = new List<POIBeizerPath>();
        List<POITextComment> texts = new List<POITextComment>();

        int size;

        int fieldSize = 3 * sizeof(int);

        public int NumText { get { return numText; } }
        public int NumBeizerPath { get { return numBeizerPath; } }
        public List<POIBeizerPath> Paths { get { return paths; } }
        public List<POITextComment> Texts { get { return texts; } }
        public int Size { get { return size; } }

        public POIComment()
        {
            size = fieldSize;
        }

        public void insert(POIBeizerPath path)
        {
            paths.Add(path);
            numBeizerPath++;

            size += path.Size;
        }

        public void insert(POITextComment text)
        {
            texts.Add(text);
            numText++;

            size += text.Size;
        }

        public override byte[] getPacket()
        {
            byte[] packet = new byte[size];
            int offset = 0;
            serialize(packet, ref offset);

            return composePacket(POIMsgDefinition.POI_USER_COMMENTS, packet);
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            serializeInt32(buffer, ref offset, frameNum);
            serializeInt32(buffer, ref offset, numBeizerPath);
            serializeInt32(buffer, ref offset, numText);

            foreach (POIBeizerPath path in paths)
            {
                path.serialize(buffer, ref offset);
            }

            foreach (POITextComment text in texts)
            {
                text.serialize(buffer, ref offset);
            }
        }

        public override void deserialize(byte[] buffer, ref int offset)
        {
            size = fieldSize;

            deserializeInt32(buffer, ref offset, ref frameNum);
            deserializeInt32(buffer, ref offset, ref numBeizerPath);
            deserializeInt32(buffer, ref offset, ref numText);


            paths = new List<POIBeizerPath>();
            for (int i = 0; i < numBeizerPath; i++)
            {
                POIBeizerPath path = new POIBeizerPath();
                path.deserialize(buffer, ref offset);
                paths.Add(path);

                size += path.Size;
            }

            texts = new List<POITextComment>();
            for (int i = 0; i < numText; i++)
            {
                POITextComment text = new POITextComment();
                text.deserialize(buffer, ref offset);
                texts.Add(text);

                size += text.Size;
            }
        }

    }
}
