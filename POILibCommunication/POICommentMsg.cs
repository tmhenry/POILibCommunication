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

        //For audio comments
        byte[] audioBytes = new byte[0];
        int audioLength;

        public enum OperationMode
        {
            CREATE = 0,
            POP,
            COLLAPSE,
            POSITOIN_CHANGED,
            CONTENT_CHANGED,
            DELETE,
            AUDIO_CHANGED
        }

        int size;
        int fieldSize;

        public int Depth { get { return depth; } set { depth = value; } }
        public OperationMode Mode { get { return mode; } set { mode = value; } }
        public float X { get { return x; } set { x = value; } }
        public float Y { get { return y; } set { y = value; } }
        public int Size { get { return size; } }
        public String Msg { get { return msg; } set { msg = value; } }

        public POITextComment() { }

        public POITextComment(int myDepth, OperationMode myMode)
        {
            depth = myDepth;
            mode = myMode;

            fieldSize = 2 * sizeof(int);
            size = fieldSize;
        }

        public POITextComment(int myDepth, byte[] myAudioBytes)
        {
            depth = myDepth;
            mode = OperationMode.AUDIO_CHANGED;

            audioLength = myAudioBytes.Length;
            audioBytes = new byte[audioLength];
            Array.Copy(myAudioBytes, audioBytes, audioLength);

            msg = "";

            calculateSize();
        }

        public void calculateSize()
        {
            System.Text.Encoding encoding = new System.Text.UTF8Encoding();
            length = encoding.GetByteCount(msg);

            if (audioBytes == null) audioLength = 0;
            else  audioLength = audioBytes.Length;

            switch (mode)
            {
                case OperationMode.CREATE:
                    fieldSize = 2 * sizeof(float) + 4 * sizeof(int);
                    size = fieldSize + length + audioLength;
                    break;

                case OperationMode.CONTENT_CHANGED:
                    fieldSize = 3 * sizeof(int);
                    size = fieldSize + length;
                    break;

                case OperationMode.AUDIO_CHANGED:
                    fieldSize = 3 * sizeof(int);
                    size = fieldSize + audioLength;
                    break;

                case OperationMode.POSITOIN_CHANGED:
                    fieldSize = 2 * sizeof(int) + 2 * sizeof(float);
                    size = fieldSize;
                    break;
            }
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
            audioLength = 0;

            fieldSize = 2 * sizeof(float) + 4 * sizeof(int);
            size = fieldSize + length + audioLength;
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            System.Text.Encoding encoding = new System.Text.UTF8Encoding();
            byte[] stringdata = encoding.GetBytes(msg);

            serializeInt32(buffer, ref offset, depth);
            serializeInt32(buffer, ref offset, (int)mode);

            if (mode == OperationMode.CREATE)
            {
                serializeFloat(buffer, ref offset, x);
                serializeFloat(buffer, ref offset, y);
                serializeInt32(buffer, ref offset, length);

                Array.Copy(stringdata, 0, buffer, offset, length);
                offset += length;

                //Serialize the audio byte length
                audioLength = audioBytes.Length;
                serializeInt32(buffer, ref offset, audioLength);
                serializeByteArray(buffer, ref offset, audioBytes);
            }
            else if(mode == OperationMode.CONTENT_CHANGED)
            {
                serializeInt32(buffer, ref offset, length);

                Array.Copy(stringdata, 0, buffer, offset, length);
                offset += length;
            }
            else if (mode == OperationMode.POSITOIN_CHANGED)
            {
                serializeFloat(buffer, ref offset, x);
                serializeFloat(buffer, ref offset, y);
            }
            else if (mode == OperationMode.AUDIO_CHANGED)
            {
                //Serialize the audio byte length
                audioLength = audioBytes.Length;
                serializeInt32(buffer, ref offset, audioLength);
                serializeByteArray(buffer, ref offset, audioBytes);
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
                deserializeFloat(buffer, ref offset, ref x);
                deserializeFloat(buffer, ref offset, ref y);
                deserializeInt32(buffer, ref offset, ref length);

                byte[] stringData = buffer.Skip(offset).Take(length).ToArray();
                offset += length;

                System.Text.Encoding encoding = new System.Text.UTF8Encoding();
                msg = encoding.GetString(stringData);

                deserializeInt32(buffer, ref offset, ref audioLength);
                audioBytes = new byte[audioLength];
                Array.Copy(buffer, offset, audioBytes, 0, audioLength);
                offset += audioLength;

                fieldSize = 2 * sizeof(float) + 4 * sizeof(int);
                size = fieldSize + length + audioLength;
            }
            else if (mode == OperationMode.CONTENT_CHANGED)
            {
                deserializeInt32(buffer, ref offset, ref length);
                byte[] stringData = buffer.Skip(offset).Take(length).ToArray();
                offset += length;

                System.Text.Encoding encoding = new System.Text.UTF8Encoding();
                msg = encoding.GetString(stringData);

                fieldSize = 3 * sizeof(int);
                size = fieldSize + length;
            }
            else if (mode == OperationMode.POSITOIN_CHANGED)
            {
                deserializeFloat(buffer, ref offset, ref x);
                deserializeFloat(buffer, ref offset, ref y);

                fieldSize = 2 * sizeof(int) + 2 * sizeof(float);
                size = fieldSize;
            }
            else if (mode == OperationMode.AUDIO_CHANGED)
            {
                deserializeInt32(buffer, ref offset, ref audioLength);
                audioBytes = new byte[audioLength];
                Array.Copy(buffer, offset, audioBytes, 0, audioLength);
                offset += audioLength;

                fieldSize = 3 * sizeof(int);
                size = fieldSize + audioLength;
            }
        }

    }

    public class POIBeizerPathPoint : POISerializable
    {
        float x;
        float y;
        double time;

        static int size = 2 * sizeof(float) + sizeof(double);
        DateTime referenceTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);

        public float X { get { return x; } set { x = value; } }
        public float Y { get { return y; } set { y = value; } }
        public double Time { get { return time; } }
        static public int Size { get { return size; } }

        public POIBeizerPathPoint() { }

        public POIBeizerPathPoint(float myX, float myY)
        {
            x = myX;
            y = myY;

            time = (float)(DateTime.Now - referenceTime).TotalMilliseconds;
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

            //To do: process the time
            //serializeFloat(buffer, ref offset, time);
            serializeDouble(buffer, ref offset, time);
        }

        public override void deserialize(byte[] buffer, ref int offset)
        {
            deserializeFloat(buffer, ref offset, ref x);
            deserializeFloat(buffer, ref offset, ref y);

            //To do: process the time
            //deserializeFloat(buffer, ref offset, ref time);
            deserializeDouble(buffer, ref offset, ref time);
        }
    }

    public class POIBeizerPath : POISerializable
    {
        int numPoints = 0;
        int depth;
        Color color;
        int shape;
        int mode;
        List<POIBeizerPathPoint> points = new List<POIBeizerPathPoint>();

        int size;
        int fieldSize = 4 * sizeof(int) + 3 * sizeof(byte);

        public int Depth { get { return depth; } set { depth = value; } }
        public int NumPoints { get { return numPoints; } set { numPoints = value; } }
        public Color Color { get { return color; } set { color = value; } }
        public int Shape { get { return shape; } set { shape = value; } }
        public int Size { get { return size; } }
        public OperationMode Mode { get { return (OperationMode)mode; } }
        public List<POIBeizerPathPoint> Points { get { return points; } set { points = value; } }

        public enum OperationMode
        {
            Realtime = 0,
            All
        }

        public void calculateSize()
        {
            size = numPoints * POIBeizerPathPoint.Size + fieldSize;
        }

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

        public void setMode(OperationMode newMode)
        {
            mode = (int)newMode;
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            serializeInt32(buffer, ref offset, depth);
           

            serializeByte(buffer, ref offset, color.R);
            serializeByte(buffer, ref offset, color.G);
            serializeByte(buffer, ref offset, color.B);
            
            serializeInt32(buffer, ref offset, shape);
            serializeInt32(buffer, ref offset, mode);
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
            deserializeInt32(buffer, ref offset, ref mode);
            deserializeInt32(buffer, ref offset, ref numPoints);
            

            size = numPoints * POIBeizerPathPoint.Size + fieldSize;

            //Deserialize all the points
            points = new List<POIBeizerPathPoint>();

            for (int i = 0; i < numPoints; i++)
            {
                POIBeizerPathPoint point = new POIBeizerPathPoint();
                point.deserialize(buffer, ref offset);
                points.Add(point);
                //POIGlobalVar.POIDebugLog("Point is" + point.X + " " + point.Y);
                //POIGlobalVar.POIDebugLog("time is" + point.Time);
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

        public int FrameNum { get { return frameNum; } set { frameNum = value; } }
        public int NumText { get { return numText; } set { numText = value; } }
        public int NumBeizerPath { get { return numBeizerPath; } set { numBeizerPath = value; } }
        public List<POIBeizerPath> Paths { get { return paths; } set { paths = value; } }
        public List<POITextComment> Texts { get { return texts; } set { texts = value; } }
        public int Size { get { return size; } }

        public POIComment()
        {
            size = fieldSize;
            messageType = POIMsgDefinition.POI_USER_COMMENTS;
        }

        public void calculateSize()
        {
            size = fieldSize + MetadataSize;
            //size = fieldSize;

            foreach (POIBeizerPath path in paths)
            {
                path.calculateSize();
                size += path.Size;
            }

            foreach (POITextComment text in texts)
            {
                text.calculateSize();
                size += text.Size;
            }

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

            //POIGlobalVar.POIDebugLog("Size is " + size);

            //return packet;
            return composePacket(POIMsgDefinition.POI_USER_COMMENTS, packet);
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            base.serialize(buffer, ref offset);
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
            size = fieldSize + MetadataSize;
            //size = fieldSize;

            base.deserialize(buffer, ref offset);
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

            

            //POIGlobalVar.POIDebugLog("Size is " + size);
        }

    }
}
