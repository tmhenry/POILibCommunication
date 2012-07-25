using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using POILibCommunication;
using System.Web.Script.Serialization;

namespace POILibCommunication
{
    public class POIPresentation: POIMessage
    {
        Dictionary<int, POISlide> slideList = new Dictionary<int, POISlide>();
        Dictionary<string, string> info = new Dictionary<string, string>();
        int presId = 3;
        Int64 size;
        const int fieldSize = 3 * sizeof(int);
        bool sizeChanged = false;
        

        
        //Properties
        public int Count { get { return slideList.Count; } }
        public Int64 Size
        {
            get 
            {
                if (sizeChanged)
                {
                    UpdateSize();
                }
                return size;
            }
        }
        public String BasePath { get; set; }

        //Functions
        public void UpdateSize()
        {
            size = fieldSize;
            JavaScriptSerializer jsonParser = new JavaScriptSerializer();
            string infoString = jsonParser.Serialize(info);
            byte[] infoBytes = Encoding.UTF8.GetBytes(infoString);
            size += infoBytes.Length;

            foreach (POISlide slide in slideList.Values)
            {
                size += slide.Size;
            }

            sizeChanged = false;
        }

        public POIPresentation()
        {
            sizeChanged = true;
            info[@"name"] = @"CS152";
        }

        public void LoadPresentationFromStorage()
        {
            String path = Directory.GetCurrentDirectory();
            BasePath = path + @"/" + presId;

            //A fake presentation info
            info[@"name"] = @"Test";
            info[@"presentor"] = @"Gary Chan";
            for (int i = 0; i < 5; i++)
            {
                POISlide slide = new POIStaticSlide(i, this);
                Insert(slide);
            }

            List<int> duration = new List<int>();
            duration.Add(500);
            duration.Add(500);
            duration.Add(500);
            duration.Add(500);
            POISlide slide2 = new POIAnimationSlide(duration, 6, this);

            Insert(slide2);
        }

        public void ParseIntoSlides(String slidesInfoJson)
        {
            
        }

        public POISlide SlideAtIndex(int index)
        {
            if (slideList.ContainsKey(index))
            {
                return slideList[index];
            }
            else
            {
                return null;
            }
        }

        public void Update(POIPresentation pres)
        {
            if (presId == pres.presId)
            {
                foreach (POISlide slide in pres.slideList.Values)
                {
                    Insert(slide);
                }

                //Update the information for the presentation
                foreach (string key in pres.info.Keys)
                {
                    info[key] = pres.info[key];
                }

                sizeChanged = true;
            }
            else
            {
                Console.WriteLine("Input presentation has a different id.");
            }
        }

        public void Insert(POISlide slide)
        {
            slideList[slide.Index] = slide;
            sizeChanged = true;
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            //Serialize number of slides
            serializeInt32(buffer, ref offset, presId);

            //Parse the info dictionary into string
            JavaScriptSerializer jsonParser = new JavaScriptSerializer();
            string infoString = jsonParser.Serialize(info);
            byte[] infoBytes = Encoding.UTF8.GetBytes(infoString);

            //Serialize the info length
            serializeInt32(buffer, ref offset, infoBytes.Length);
            serializeByteArray(buffer, ref offset, infoBytes);
            

            serializeInt32(buffer, ref offset, slideList.Count);

            foreach (POISlide slide in slideList.Values)
            {
                slide.serialize(buffer, ref offset);
            }
        }

        public override void deserialize(byte[] buffer, ref int offset)
        {
            size = fieldSize;
            slideList = new Dictionary<int, POISlide>();

            deserializeInt32(buffer, ref offset, ref presId);

            //Deserialize the info length
            int infoLength = 0;
            deserializeInt32(buffer, ref offset, ref infoLength);
            byte[] infoBytes = buffer.Skip(offset).Take(infoLength).ToArray();
            offset += infoLength;

            string infoString = Encoding.UTF8.GetString(infoBytes);
            JavaScriptSerializer jsonParser = new JavaScriptSerializer();
            info = jsonParser.Deserialize<Dictionary<string, string>>(infoString);
            size += infoLength;

            //Deserialize the number of slides
            int numSlides = 0;
            deserializeInt32(buffer, ref offset, ref numSlides);

            for (int i = 0; i < numSlides; i++)
            {
                POISlide slide = new POISlide(this);
                slide.deserialize(buffer, ref offset);

                Insert(slide);
                size += slide.Size;
            }

            sizeChanged = false;
        }

        public override byte[] getPacket()
        {
            byte[] packet = new byte[Size];
            int offset = 0;
            serialize(packet, ref offset);

            return composePacket(POIMsgDefinition.POI_PRESENTATION_CONTENT, packet);
        }
    }
}
