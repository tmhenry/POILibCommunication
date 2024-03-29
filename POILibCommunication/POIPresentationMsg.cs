﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POILibCommunication
{
    public class POIPresCtrlMsg : POIMessage
    {
        int ctrlType;
        int slideIndex;

        public int CtrlType { get { return ctrlType; } set { ctrlType = value; } }
        public int SlideIndex { get { return slideIndex; } set { slideIndex = value; } }

        const int fieldSize = 2 * sizeof(int);
        static int size = fieldSize + MetadataSize;
        static public int Size { get { return size; } }

        public POIPresCtrlMsg() 
        {
            messageType = POIMsgDefinition.POI_PRESENTATION_CONTROL;
        }

        public POIPresCtrlMsg(int myCtrlType, int mySlideIndex)
        {
            messageType = POIMsgDefinition.POI_PRESENTATION_CONTROL;
            ctrlType = myCtrlType;
            slideIndex = mySlideIndex;
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            base.serialize(buffer, ref offset);
            serializeInt32(buffer, ref offset, ctrlType);
            serializeInt32(buffer, ref offset, slideIndex);
        }

        public override void deserialize(byte[] buffer, ref int offset)
        {
            base.deserialize(buffer, ref offset);
            deserializeInt32(buffer, ref offset, ref ctrlType);
            deserializeInt32(buffer, ref offset, ref slideIndex);

            Console.WriteLine(ctrlType + " " + slideIndex);
        }

        public override byte[] getPacket()
        {
            byte[] packet = new byte[size];
            int offset = 0;

            serialize(packet, ref offset);

            //return packet;
            return composePacket(POIMsgDefinition.POI_PRESENTATION_CONTROL, packet);
        }
    }
}
