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
        POIMetadataContainer<Double> DataDict = new POIMetadataContainer<Double>();

        int presId;
        int sessionId;
        string archiveFn;
        string logFn;

        StreamWriter sw;

        public POIMetadataArchive(int pId, int sId)
        {
            presId = pId;
            sessionId = sId;

            archiveFn = Path.Combine(POIArchive.ArchiveHome, pId + "_" + sId + ".meta");
            logFn = Path.Combine(POIArchive.ArchiveHome, pId + "_" + sId + ".txt");

            FileStream fs = new FileStream(logFn, FileMode.Create);
            sw = new StreamWriter(fs);
        }

        

        public void LogEvent(POIMessage message)
        {
            DataDict.Add(message.Timestamp, message);
            sw.WriteLine(message.Timestamp + " : " + message.MessageType);
        }

        public Dictionary<string, POIMessage> MetadataList
        {
            get
            {
                Dictionary<string, POIMessage> dataToSerialize = DataDict.Keys.ToDictionary(p => p.ToString(), p => DataDict[p]);
                return dataToSerialize;
            }
        }

        public void WriteArchive()
        {
            FileStream fs = new FileStream(archiveFn, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);

            foreach (POIMessage message in DataDict.Values)
            {
                byte[] data = message.getPacket();
                bw.Write(data);
            }

            bw.Close();

            //Upload the archive to the content server
            POIContentServerHelper.uploadContent(presId, archiveFn);

            sw.Close();
            POIContentServerHelper.uploadContent(presId, logFn);
        }

        public void ReadArchive()
        {
            //Clear the current container
            DataDict.Clear();

            //Read the online into memory
            byte[] buffer = POIContentServerHelper.getMetaArchive(presId, sessionId);
            if (buffer == null)
            {
                Console.WriteLine("Cannot retrieve metadata archive!");
                return;
            }

            int offset = 0;
            byte msgTypeByte = 0;
            POIMessage curMsg = null;

            while (offset < buffer.Length)
            {
                
                try
                {
                    msgTypeByte = buffer[offset];
                    offset++;

                    curMsg = POIMessageFactory.Instance.CreateMessage(msgTypeByte);

                    curMsg.deserialize(buffer, ref offset);
                    Console.WriteLine(offset);
                    
                }
                catch (Exception e)
                {
                    Console.WriteLine("WTF");
                }
                
                LogEvent(curMsg);
            }
        }

    }
}
