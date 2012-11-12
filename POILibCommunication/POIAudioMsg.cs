using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace POILibCommunication
{
    public class POIAudioContentMsg : POIMessage
    {
        int size;
        byte[] audioBytes;

        public int Size { get { return audioBytes.Length + sizeof(int); } }
        public byte[] AudioBytes { get { return audioBytes; } }

        public override void deserialize(byte[] buffer, ref int offset)
        {
            //Deserialize the length
            int length = 0;
            deserializeInt32(buffer, ref offset, ref length);

            audioBytes = new byte[length];
            Array.Copy(buffer, audioBytes, length);
            offset += length;
        }

        public override void serialize(byte[] buffer, ref int offset)
        {
            serializeInt32(buffer, ref offset, audioBytes.Length);
            serializeByteArray(buffer, ref offset, audioBytes);
        }

        public override byte[] getPacket()
        {
            byte[] packet = new byte[Size];
            int offset = 0;
            serialize(packet, ref offset);

            return composePacket(POIMsgDefinition.POI_AUDIO_CONTENT, packet);
        }
    }
}
