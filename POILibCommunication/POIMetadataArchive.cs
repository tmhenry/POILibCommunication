using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Threading.Tasks;

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

    public class POIMetadataArchive : POIMessage
    {
        //Data members
        POIMetadataContainer<Double> DataDict = new POIMetadataContainer<Double>();
        Dictionary<int, int> DataIndexer = new Dictionary<int, int>();
        List<int> SnapshotIndexer = new List<int>();
        Dictionary<int, List<POIComment>> commentRepo = new Dictionary<int, List<POIComment>>();

        int presId;
        int sessionId;
        string archiveFn;
        string logFn;
        double audioTimeReference;
        double sessionTimeReference;

        int size;
        int numComments = 0;

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

        public List<int> SnapshotList
        {
            get
            {
                return SnapshotIndexer;
            }
        }

        public Dictionary<string, List<POIComment>> CommentRepo
        {
            get
            {
                Dictionary<string, List<POIComment>> dataToSerialize = commentRepo.Keys.ToDictionary(p => p.ToString(), p => commentRepo[p]);
                return dataToSerialize;
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
            audioTimeReference = sessionTimeReference;

            //Initialize an empty metadata archive
            DataDict = new POIMetadataContainer<double>();
            DataIndexer = new Dictionary<int, int>();
            SnapshotIndexer = new List<int>();
            numComments = 0;

            size = 6 * sizeof(int) + 2 * sizeof(double);
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
            else if (message.MessageType == POIMsgDefinition.POI_USER_COMMENTS)
            {
                POIComment comment = message as POIComment;
                if (comment.Mode == 1)
                {
                    DataDict.Add(message.Timestamp, message);
                }
                else
                {
                    //Add to the comment repo according to the slide index
                    if (!commentRepo.ContainsKey(comment.FrameNum))
                    {
                        commentRepo[comment.FrameNum] = new List<POIComment>();
                    }

                    commentRepo[comment.FrameNum].Add(comment);
                }
            }
            else
                DataDict.Add(message.Timestamp, message);

            //POIGlobalVar.POIDebugLog("Message with type " + message.MessageType + " and timestamp " + message.Timestamp);
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

        public void LogEventAndUpdateSnapshotIndexer(POIWhiteboardMsg message)
        {
            if (message.CtrlType == (int)WBCtrlType.Save)
            {
                //Add the event into the whiteboard indexer
                SnapshotIndexer.Add(DataDict.Values.Count);
            }

            LogEvent(message);
        }

        private void updateTimeReference()
        {
            //Update the audio time reference
            if (audioTimeReference == 0)
            {
                audioTimeReference = sessionTimeReference;
            }

            //Update the session timereference
            if (sessionTimeReference >= audioTimeReference)
            {
                sessionTimeReference = audioTimeReference;
            }
            else
            {
                //Choose the minimum time of the first event and audioTimeReference
                if (DataDict.Keys.Count > 0)
                {
                    sessionTimeReference = Math.Min(audioTimeReference, DataDict.Keys[0]);
                }
                else
                {
                    sessionTimeReference = audioTimeReference;
                }
                
            }

            //Update the event log for the first slide
            if (DataIndexer.ContainsKey(0))
            {
                DataIndexer.Remove(0);
            }
        }

        public async Task WriteArchive()
        {
            POIGlobalVar.POIDebugLog(archiveFn);
            FileStream fs = new FileStream(archiveFn, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);

            //Update the time reference to remove unnecessary waiting time
            try
            {
                updateTimeReference();
            }
            catch (Exception e)
            {
                POIGlobalVar.POIDebugLog(e);
            }

            int subSize = 6 * sizeof(int) + 2 * sizeof(double);
            subSize += 2 * sizeof(int) * DataIndexer.Count;
            subSize += sizeof(int) * SnapshotIndexer.Count;

            byte[] buffer = new byte[subSize];
            int offset = 0;

            serializeInt32(buffer, ref offset, presId);
            serializeInt32(buffer, ref offset, sessionId);

            serializeDouble(buffer, ref offset, sessionTimeReference);
            serializeDouble(buffer, ref offset, audioTimeReference);

            //Write the metadata indexer data
            serializeInt32(buffer, ref offset, DataIndexer.Count);

            foreach (int key in DataIndexer.Keys)
            {
                serializeInt32(buffer, ref offset, key);
                serializeInt32(buffer, ref offset, DataIndexer[key]);
            }

            //Write the snapshot indexer data
            serializeInt32(buffer, ref offset, SnapshotIndexer.Count);

            foreach (int sIndex in SnapshotIndexer)
            {
                serializeInt32(buffer, ref offset, sIndex);
            }

            serializeInt32(buffer, ref offset, DataDict.Count);

            numComments = 0;
            foreach (List<POIComment> cmtList in commentRepo.Values)
            {
                numComments += cmtList.Count;
            }

            serializeInt32(buffer, ref offset, numComments);

            bw.Write(buffer);

            foreach (POIMessage message in DataDict.Values)
            {
                byte[] data = message.getPacket();
                bw.Write(data);
            }

            foreach (List<POIComment> cmtList in commentRepo.Values)
            {
                foreach (POIComment comment in cmtList)
                {
                    byte[] data = comment.getPacket();
                    bw.Write(data);
                }
            }

            bw.Close();

            //Upload the archive to the content server
            await POIContentServerHelper.uploadContent(presId, archiveFn);
        }

        public async Task ReadArchive()
        {
            //Read the online archive into memory
            byte[] buffer = await POIContentServerHelper.getMetaArchive(presId, sessionId);
            if (buffer == null)
            {
                POIGlobalVar.POIDebugLog("Cannot retrieve metadata archive!");
                return;
            }

            size = buffer.Length;
            int offset = 0;
            deserialize(buffer, ref offset);
        }

        

        public override void serialize(byte[] buffer, ref int offset)
        {
            serializeInt32(buffer, ref offset, presId);
            serializeInt32(buffer, ref offset, sessionId);

            serializeDouble(buffer, ref offset, sessionTimeReference);
            serializeDouble(buffer, ref offset, audioTimeReference);

            serializeInt32(buffer, ref offset, DataIndexer.Count);

            foreach (int key in DataIndexer.Keys)
            {
                serializeInt32(buffer, ref offset, key);
                serializeInt32(buffer, ref offset, DataIndexer[key]);
            }

            serializeInt32(buffer, ref offset, SnapshotIndexer.Count);

            foreach (int sIndex in SnapshotIndexer)
            {
                serializeInt32(buffer, ref offset, sIndex);
            }

            serializeInt32(buffer, ref offset, DataDict.Count);
            serializeInt32(buffer, ref offset, numComments);

            foreach (POIMessage message in DataDict.Values)
            {
                byte[] data = message.getPacket();
                serializeByteArray(buffer, ref offset, data);
            }

            foreach (List<POIComment> cmtList in commentRepo.Values)
            {
                foreach (POIComment comment in cmtList)
                {
                    byte[] data = comment.getPacket();
                    serializeByteArray(buffer, ref offset, data);
                }
            }

        }

        public override void deserialize(byte[] buffer, ref int offset)
        {
            //Clear the current container
            DataDict.Clear();
            DataIndexer.Clear();
            SnapshotIndexer.Clear();

            deserializeInt32(buffer, ref offset, ref presId);
            deserializeInt32(buffer, ref offset, ref sessionId);

            deserializeDouble(buffer, ref offset, ref sessionTimeReference);
            deserializeDouble(buffer, ref offset, ref audioTimeReference);

            int numIndexer = 0;
            deserializeInt32(buffer, ref offset, ref numIndexer);
            int key = 0, val = 0;

            for (int i = 0; i < numIndexer; i++)
            {
                deserializeInt32(buffer, ref offset, ref key);
                deserializeInt32(buffer, ref offset, ref val);

                DataIndexer.Add(key, val);
            }

            int numSnapshots = 0;
            deserializeInt32(buffer, ref offset, ref numSnapshots);
            int sIndex = 0;

            for (int i = 0; i < numSnapshots; i++)
            {
                deserializeInt32(buffer, ref offset, ref sIndex);

                SnapshotIndexer.Add(sIndex);
            }

            int numMsgs = 0;
            deserializeInt32(buffer, ref offset, ref numMsgs);
            deserializeInt32(buffer, ref offset, ref numComments);

            byte msgTypeByte = 0;
            POIMessage curMsg = null;

            //Deserialize the messages
            for (int i = 0; i < numMsgs; i++)
            {
                try
                {
                    deserializeByte(buffer, ref offset, ref msgTypeByte);
                    curMsg = POIMessageFactory.Instance.CreateMessage(msgTypeByte);

                    curMsg.deserialize(buffer, ref offset);
                }
                catch (Exception e)
                {
                    POIGlobalVar.POIDebugLog("WTF");
                }

                LogEvent(curMsg);
            }

            //Deserialize the comment
            for (int i = 0; i < numComments; i++)
            {
                try
                {
                    deserializeByte(buffer, ref offset, ref msgTypeByte);
                    curMsg = POIMessageFactory.Instance.CreateMessage(msgTypeByte);

                    curMsg.deserialize(buffer, ref offset);
                }
                catch (Exception e)
                {
                    POIGlobalVar.POIDebugLog("WTF");
                }

                LogEvent(curMsg);
            }
        }

        public override byte[] getPacket()
        {
            byte[] packet = new byte[size];
            int offset = 0;
            serialize(packet, ref offset);

            return composePacket(POIMsgDefinition.POI_METADATA_ARCHIVE, packet);
        }

    }
}
