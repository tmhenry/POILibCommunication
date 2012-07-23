using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

using POILibCommunication;

namespace POILibCommunication
{
    public enum SlideType
    {
        Static = 0,
        Animation
    }

    public enum SlideContentFormat
    {
        PNG = 0,
        JPEG,
        WMV
    }

    public class POISlide : POISerializable
    {
        protected Uri source;
        protected List<int> durationList;

        protected SlideContentFormat format;
        protected SlideType type;

        Int64 size;
        const int fieldSizeStatic = 3 * sizeof(int);
        const int fieldSizeAnimation = 4 * sizeof(int);

        public Int64 Size
        {
            get { return size; }
        }

        public Uri Source
        {
            get { return source; }
            set { source = value; }
        }

        public POISlide()
        {
            size = 0;
        }

        public POISlide(String uriName)
        {
            type = SlideType.Static;

            source = new Uri(uriName);
            durationList = new List<int>();

            size = fieldSizeStatic;

            FileInfo info = new FileInfo(uriName);
            size += info.Length;
        }

        public POISlide(String uriName, List<int> myDurationList)
        {
            type = SlideType.Animation;

            source = new Uri(uriName);
            durationList = myDurationList;

            size = fieldSizeAnimation + durationList.Count * sizeof(int);

            FileInfo info = new FileInfo(uriName);
            size += info.Length;
        }

        public int GetDurationAtIndex(int index)
        {
            if (index < durationList.Count && index >= 0)
            {
                return durationList[index];
            }
            else
            {
                return -1;
            }
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            //Serialize the slide type
            serializeInt32(buffer, ref offset, (int)type);

            //Serialize the duration list
            if (type == SlideType.Animation)
            {
                serializeInt32(buffer, ref offset, durationList.Count);

                foreach (Int32 duration in durationList)
                {
                    serializeInt32(buffer, ref offset, duration);
                }
            }

            //Serialize the content
            serializeInt32(buffer, ref offset, (int)format);

            try
            {
                byte[] data = File.ReadAllBytes(source.ToString());
                int dataSize = data.Length;

                serializeInt32(buffer, ref offset, dataSize);
                Array.Copy(data, 0, buffer, offset, dataSize);
                offset += dataSize;

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        public override void deserialize(byte[] buffer, ref int offset)
        {
            size = 0;

            int slideType = 0;
            deserializeInt32(buffer, ref offset, ref slideType);
            type = (SlideType)slideType;

            size += sizeof(int);

            //Deserialize the duration list if it is an animation slide
            if (type == SlideType.Animation)
            {
                durationList = new List<int>();

                Int32 numDuration = 0;
                deserializeInt32(buffer, ref offset, ref numDuration);
                size += sizeof(int) * (numDuration + 1);

                //Deserialize the duration list
                for (int i = 0; i < numDuration; i++)
                {
                    Int32 duration = 0;
                    deserializeInt32(buffer, ref offset, ref duration);
                    durationList.Add(duration);
                }
            }

            //Deserialize the format 
            int slideContentFormat = 0;
            deserializeInt32(buffer, ref offset, ref slideContentFormat);
            format = (SlideContentFormat)slideContentFormat;

            size += sizeof(int);

            //Deserialize and save the slides into hard-disk
            int dataSize = 0;
            deserializeInt32(buffer, ref offset, ref dataSize);
            size += sizeof(int) + dataSize;

            String uri = Directory.GetCurrentDirectory() + @"/";
            uri += @"test";

            switch (format)
            {
                case SlideContentFormat.JPEG:
                    uri += @".jpg";
                    break;

                case SlideContentFormat.PNG:
                    uri += @".png";
                    break;

                case SlideContentFormat.WMV:
                    uri += @".wmv";
                    break;

                default:
                    throw (new Exception { });
            }

            //Get the data size
            try
            {
                FileStream myStream = new FileStream(uri, FileMode.Create);
                myStream.Write(buffer, offset, dataSize);
                myStream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            //Increase the handled offset
            offset += dataSize;
        }
    }
}
