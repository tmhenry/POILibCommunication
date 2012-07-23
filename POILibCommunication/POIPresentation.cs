using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using POILibCommunication;

namespace POILibCommunication
{
    public class POIPresentation: POISerializable
    {
        List<POISlide> slideList = new List<POISlide>();
        Int64 size;

        const int fieldSize = sizeof(int);

        public int Count { get { return slideList.Count; } }

        public Int64 Size
        {
            get { return size; }
        }

        public POIPresentation()
        {
            size = fieldSize;
        }

        public void ParsePresentationData(byte[] data)
        {
            //For testing purposes:
            String path = Directory.GetCurrentDirectory();

            for (int slideNum = 2; slideNum <= 6; slideNum++)
            {
                String fn = path + "/Slide" + slideNum + ".PNG";
                POISlide newSlide = new POISlide(fn);
                PushBack(newSlide);
            }

            List<int> duration = new List<int>();
            duration.Add(500);
            duration.Add(500);
            duration.Add(500);
            duration.Add(500);
            POISlide slide2 = new POISlide(path + "/test.wmv", duration);

            PushBack(slide2);
        }

        public void ParseIntoSlides(String slidesInfoJson)
        {
            
        }

        public POISlide SlideAtIndex(int index)
        {
            return slideList.ElementAt(index);
        }

        public void PushBack(POISlide slide)
        {
            slideList.Add(slide);
            size += slide.Size;
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            //Serialize number of slides
            serializeInt32(buffer, ref offset, slideList.Count);

            foreach (POISlide slide in slideList)
            {
                slide.serialize(buffer, ref offset);
            }
        }

        public override void deserialize(byte[] buffer, ref int offset)
        {
            size = 0;
            slideList = new List<POISlide>();

            //Deserialize the number of slides
            int numSlides = 0;
            deserializeInt32(buffer, ref offset, ref numSlides);
            size += sizeof(int);

            for (int i = 0; i < numSlides; i++)
            {
                POISlide slide = new POISlide();
                slide.deserialize(buffer, ref offset);

                size += slide.Size;
            }
        }
    }
}
