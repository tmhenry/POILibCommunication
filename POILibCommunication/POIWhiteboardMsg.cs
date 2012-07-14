using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POILibCommunication
{
    public class POIShowWhiteboardMsg : POIMessage
    {
        public bool IsWhiteBackground 
        {
            get { return slide == -1; }
        }

        int slide;
        int size;

        const int fieldSize = sizeof(int);
            
        public int Slide { get { return slide; } }
        public int Size { get { return size; } }

        public POIShowWhiteboardMsg() 
        {
            SetWhiteBackground();
            size = fieldSize;
        }
        public POIShowWhiteboardMsg(int mySlide)
        {
            slide = mySlide;
            size = fieldSize;
        }

        public void SetWhiteBackground()
        {
            slide = -1;
        }

        public override void deserialize(byte[] buffer, ref int offset)
        {
            deserializeInt32(buffer, ref offset, ref slide);
            size = fieldSize;
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            serializeInt32(buffer, ref offset, slide);
        }

        public override byte[] getPacket()
        {
            byte[] packet = new byte[size];
            int offset = 0;

            serialize(packet, ref offset);

            return composePacket(POIMsgDefinition.POI_WHITEBOARD_SHOW, packet);
        }
    }

    public class POIHideWhiteboardMsg : POIMessage
    {
        public override void deserialize(byte[] buffer, ref int offset)
        {
            //Do nothing
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            //Do nothing
        }

        public override byte[] getPacket()
        {
            byte[] packet = new byte[0];

            return composePacket(POIMsgDefinition.POI_WHITEBOARD_HIDE, packet);
        }
    }
}
