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
        Dictionary<int, int> DataIndexer = new Dictionary<int, int>();

        int presId;
        int sessionId;
        string archiveFn;
        string logFn;
        double audioTimeReference;
        double sessionTimeReference;
        

        //Properties
        public double AudioTimeReference
        {
            get { return audioTimeReference; }
            set { audioTimeReference = value; }
        }

        public double SessionTimeReference
        {
            get { return sessionTimeReference; }
            set { sessionTimeReference = value; }
        }

        public Dictionary<string, int> MetadataIndexer
        {
            get
            {
                Dictionary<string, int> dataToSerialize = DataIndexer.Keys.ToDictionary(p => p.ToString(), p => DataIndexer[p]);
                return dataToSerialize;
            }
        }

        public List<POIMessage> MetadataList
        {
            get
            {
                return DataDict.Values.ToList();
            }
        }

        //Constructor
        public POIMetadataArchive(int pId, int sId)
        {
            presId = pId;
            sessionId = sId;

            archiveFn = Path.Combine(POIArchive.ArchiveHome, pId + "_" + sId + ".meta");
            logFn = Path.Combine(POIArchive.ArchiveHome, pId + "_" + sId + ".txt");

            //Record the archive creation time as the time reference
            sessionTimeReference = POITimestamp.ConvertToUnixTimestamp(DateTime.Now);
        }



        //Functions
        public void LogEvent(POIMessage message)
        {
            if (message.MessageType == POIMsgDefinition.POI_POINTER_CONTROL)
            {
                POIPointerMsg ptrMsg = message as POIPointerMsg;
                message.setTimestampToDouble(ptrMsg.Timestamp);
                DataDict.Add(ptrMsg.Timestamp, ptrMsg);
            }
            else
                DataDict.Add(message.Timestamp, message);

            Console.WriteLine("Message with type " + message.MessageType + " and timestamp " + message.Timestamp);
        }

        public void LogEventAndUpdateEventIndexer(POIPresCtrlMsg message)
        {
            //The indexer is the index of the event causing the slide number change
            if (message.CtrlType == (int)PresCtrlType.Next)
            {
                if (!DataIndexer.ContainsKey(message.SlideIndex + 1))
                {
                    DataIndexer[message.SlideIndex + 1] = DataDict.Values.Count;
                }
            }
            else if(message.CtrlType == (int)PresCtrlType.Jump)
            {
                if (!DataIndexer.ContainsKey(message.SlideIndex))
                {
                    DataIndexer[message.SlideIndex] = DataDict.Values.Count;
                }
            }

            LogEvent(message);
        }

        public void WriteArchive()
        {
            FileStream fs = new FileStream(archiveFn, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);

            //Write the session timing reference
            bw.Write(sessionTimeReference);

            //Write the audio timing reference
            bw.Write(audioTimeReference);

            //Write the number of events
            bw.Write(DataDict.Count);

            foreach (POIMessage message in DataDict.Values)
            {
                byte[] data = message.getPacket();
                bw.Write(data);
            }

            //Write the number of indexer contents
            bw.Write(DataIndexer.Count);

            foreach (int key in DataIndexer.Keys)
            {
                bw.Write(key);
                bw.Write(DataIndexer[key]);
            }

            bw.Close();

            //Upload the archive to the content server
            POIContentServerHelper.uploadContent(presId, archiveFn);
        }

        public void ReadArchive()
        {
            //Clear the current container
            DataDict.Clear();
            DataIndexer.Clear();

            //Read the online into memory
            byte[] buffer = POIContentServerHelper.getMetaArchive(presId, sessionId);
            if (buffer == null)
            {
                Console.WriteLine("Cannot retrieve metadata archive!");
                return;
            }

            MemoryStream ms = new MemoryStream(buffer);
            BinaryReader br = new BinaryReader(ms);

            sessionTimeReference = br.ReadDouble();
            audioTimeReference = br.ReadDouble();
            
            //Read the number of messages
            int numMsgs = br.ReadInt32();
            int initialOffset = (int) br.BaseStream.Position;
            int offset = initialOffset;
            byte msgTypeByte = 0;
            POIMessage curMsg = null;

            for (int i = 0; i < numMsgs; i++)
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

            //Seek to the current offset within the buffer
            br.BaseStream.Seek(offset, SeekOrigin.Begin);
            
            //Read the number of indexer contents
            int numIndexer = br.ReadInt32();
            for (int i = 0; i < numIndexer; i++)
            {
                int key = br.ReadInt32();
                int val = br.ReadInt32();

                DataIndexer.Add(key, val);
            }
        }

    }
}
