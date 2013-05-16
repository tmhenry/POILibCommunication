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

    public class POIWhiteboardMsg: POIMessage
    {
        public bool IsWhiteBackground
        {
            get { return slide == -1; }
        }

        int slide;
        int ctrlType;
        
        public int Slide { get { return slide; } }
        public int CtrlType { get { return ctrlType; } }
        
        
        int size = fieldSize + MetadataSize;
        const int fieldSize = 2 * sizeof(int);
        public int Size { get { return size; } }

        public void SetWhiteBackground()
        {
            slide = -1;
        }

        public POIWhiteboardMsg() 
        {
            messageType = POIMsgDefinition.POI_WHITEBOARD_CONTROL;
            SetWhiteBackground();
            
        }
        public POIWhiteboardMsg(int myCtrlType, int mySlideIndex)
        {
            messageType = POIMsgDefinition.POI_WHITEBOARD_CONTROL;
            slide = mySlideIndex;
            ctrlType = myCtrlType;
        }

        public override void deserialize(byte[] buffer, ref int offset)
        {
            base.deserialize(buffer, ref offset);
            deserializeInt32(buffer, ref offset, ref ctrlType);
            deserializeInt32(buffer, ref offset, ref slide);
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            base.serialize(buffer, ref offset);
            serializeInt32(buffer, ref offset, ctrlType);
            serializeInt32(buffer, ref offset, slide);
        }

        public override byte[] getPacket()
        {
            byte[] packet = new byte[size];
            int offset = 0;

            serialize(packet, ref offset);

            //return packet;
            return composePacket(POIMsgDefinition.POI_WHITEBOARD_CONTROL, packet);
        }
    }
}
