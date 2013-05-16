using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace POILibCommunication
{
    public class POIMetadataContainer<T>: SortedList<T, POIMessage>
    {
        private int BinarySearch(T key)
        {
            List<T> keys = this.Keys as List<T>;
            int index= keys.BinarySearch(key);

            //Take the bitwise complement
            if (index < 0) index = ~index;

            return index;
        }

    }

    public class POIMetadataArchive
    {
        //Data members
        POIMetadataContainer<double> DataDict = new POIMetadataContainer<double>();

        Int64 size = 0;
        int presId;
        int sessionId;
        string archiveFn;

        public POIMetadataArchive(int pId, int sId)
        {
            presId = pId;
            sessionId = sId;

            archiveFn = Path.Combine(POIArchive.ArchiveHome, pId + "_" + sId + ".meta");
        }

        public void LogEvent(POIMessage message)
        {
            DataDict.Add(message.Timestamp, message);
        }

        public void WriteArchive()
        {
            FileStream fs = new FileStream(archiveFn, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);

            foreach (POIMessage message in DataDict.Values)
            {
                bw.Write(message.getPacket());
            }

            bw.Close();
        }

        public void ReadArchive()
        {
            //Clear the current container
            DataDict.Clear();

            //Read the file into memory
            FileStream fs = new FileStream(archiveFn, FileMode.Open);
            MemoryStream ms = new MemoryStream();
            ms.SetLength(fs.Length);
            fs.Read(ms.GetBuffer(), 0, (int)ms.Length);
            fs.Close();

            byte[] buffer = ms.GetBuffer();
            int offset = 0;
            byte msgTypeByte = 0;
            POIMessage curMsg = null;

            while (offset < ms.Length)
            {
                msgTypeByte = buffer[offset];
                offset++;

                curMsg = POIMessageFactory.Instance.CreateMessage(msgTypeByte);
                curMsg.deserialize(buffer, ref offset);
                LogEvent(curMsg);
            }

            ms.Close();
        }

    }
}
