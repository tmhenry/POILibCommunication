using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Http;
using POILibCommunication;

namespace POILibCommunication
{
    public class POIContentServerHelper
    {
        static WebClient webClient;

        public enum FileType
        {
            Static = 0,
            Animation,
            AnimationCover,
            PresInfo
        };

        static public String ContentServerHome
        {
            get { return POIGlobalVar.ContentServerHome; }
        }

        static public byte[] getPresInfo(int presId)
        {
            return getContent(presId, FileType.PresInfo, 0);
        }

        static public byte[] getStaticSlide(int presId, int slideIndex)
        {
            return getContent(presId, FileType.Static, slideIndex);
        }

        static public byte[] getAnimationSlide(int presId, int slideIndex)
        {
            return getContent(presId, FileType.Animation, slideIndex);
        }

        static public byte[] getAnimationSlideCover(int presId, int slideIndex)
        {
            return getContent(presId, FileType.AnimationCover, slideIndex);
        }

        static private byte[] getContent(int presId, FileType fileType, int slideIndex)
        {
            String reqUrl = ContentServerHome + "?"
                            + "presId=" + presId + "&"
                            + "fileType=" + (int)fileType + "&"
                            + "slideIndex=" + slideIndex;

            if(webClient == null) webClient = new WebClient();

            try
            {
                return webClient.DownloadData(reqUrl);
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex);
                return null;
            }
            
        }
        
    }
}
