using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POILibCommunication
{
    public class POIPresCtrlMsg : POIMessage
    {
        int ctrlType;
        int slideIndex;

        public int CtrlType { get { return ctrlType; } }
        public int SlideIndex { get { return slideIndex; } }

        const int fieldSize = 2 * sizeof(int);
        static int size = fieldSize;
        static public int Size { get { return size; } }

        public POIPresCtrlMsg() { }
        public POIPresCtrlMsg(int myCtrlType, int mySlideIndex)
        {
            ctrlType = myCtrlType;
            slideIndex = mySlideIndex;
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            serializeInt32(buffer, ref offset, ctrlType);
            serializeInt32(buffer, ref offset, slideIndex);
        }

        public override void deserialize(byte[] buffer, ref int offset)
        {
            Console.WriteLine("I'm here!");
            deserializeInt32(buffer, ref offset, ref ctrlType);
            deserializeInt32(buffer, ref offset, ref slideIndex);

            Console.WriteLine(ctrlType + " " + slideIndex);
        }

        public override byte[] getPacket()
        {
            byte[] packet = new byte[size];
            int offset = 0;

            serialize(packet, ref offset);

            return composePacket(POIMsgDefinition.POI_PRESENTATION_CONTROL, packet);
        }
    }
}
