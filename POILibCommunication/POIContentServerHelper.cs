using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Http;
using POILibCommunication;
using System.IO;

using System.Threading.Tasks;

namespace POILibCommunication
{
    public class POIContentServerHelper
    {
        //static WebClient webClient;

        public enum FileType
        {
            Static = 0,
            Animation,
            AnimationCover,
            PresInfo,
            PresCover,
            MetaArchive
        };

        static public String ContentServerHome
        {
            get { return POIGlobalVar.ContentServerHome; }
        }

        static public async Task<byte[]> getPresInfo(int presId)
        {
            return await getContent(presId, FileType.PresInfo, 0, 0);
        }

        static public async Task<byte[]> getStaticSlide(int presId, int slideIndex)
        {
            return await getContent(presId, FileType.Static, slideIndex, 0);
        }

        static public async Task<byte[]> getAnimationSlide(int presId, int slideIndex)
        {
            return await getContent(presId, FileType.Animation, slideIndex, 0);
        }

        static public async Task<byte[]> getAnimationSlideCover(int presId, int slideIndex)
        {
            return await getContent(presId, FileType.AnimationCover, slideIndex, 0);
        }

        static public async Task<byte[]> getPresCover(int presId)
        {
            return await getContent(presId, FileType.PresCover, 0, 0);
        }

        static public async Task<byte[]> getMetaArchive(int presId, int sessionId)
        {
            return await getContent(presId, FileType.MetaArchive, 0, sessionId);
        }

        static private async Task<byte[]> getContent(int presId, FileType fileType, int slideIndex, int sessionId)
        {
            String reqUrl = ContentServerHome + "content.php?"
                            + "presId=" + presId + "&"
                            + "fileType=" + (int)fileType + "&"
                            + "slideIndex=" + slideIndex + "&"
                            + "sessionId=" + sessionId;

            //if(webClient == null) webClient = new WebClient();
            using (WebClient webClient = new WebClient())
            {
                POIGlobalVar.POIDebugLog("Getting content!");
                webClient.Proxy = null;

                try
                {
                    return await webClient.DownloadDataTaskAsync(reqUrl);
                }
                catch (WebException ex)
                {
                    POIGlobalVar.POIDebugLog(ex);
                    return null;
                }
            }
            
        }

        static public async Task uploadContent(int presId, string fileName)
        {
            String reqUrl = ContentServerHome + "store.php?" + "presId=" + presId;

            //if (webClient == null) webClient = new WebClient();
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    byte[] response = await webClient.UploadFileTaskAsync(reqUrl, "POST", fileName);
                    string str = ASCIIEncoding.UTF8.GetString(response);

                    POIGlobalVar.POIDebugLog(str);
                }
                catch (WebException ex)
                {
                    POIGlobalVar.POIDebugLog(ex);
                }
            }
        }

        /*
        static public string getAudioSyncReference(int presId, int sessionId)
        {
            String reqUrl = "http://192.168.0.100/3/audio/timequery/query.php?pid=1";
            
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    return webClient.DownloadString(reqUrl);
                }
                catch (WebException ex)
                {
                    POIGlobalVar.POIDebugLog(ex);
                    return null;
                }
            }
        }

        public static void UploadFilesToRemoteUrl(string url, string fileName)
        {
            string boundary = "----------------------------" +
            DateTime.Now.Ticks.ToString("x");


            HttpWebRequest httpWebRequest2 = (HttpWebRequest)WebRequest.Create(url);
            httpWebRequest2.ContentType = "multipart/form-data; boundary=" +
            boundary;
            httpWebRequest2.Method = "POST";
            httpWebRequest2.KeepAlive = true;
            httpWebRequest2.Credentials =
            System.Net.CredentialCache.DefaultCredentials;



            Stream memStream = new System.IO.MemoryStream();

            byte[] boundarybytes = System.Text.Encoding.ASCII.GetBytes("\r\n--" + boundary + "\r\n");

            


            memStream.Write(boundarybytes, 0, boundarybytes.Length);

            string headerTemplate = "Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"\r\n Content-Type: application/octet-stream\r\n\r\n";

            //string header = string.Format(headerTemplate, "file" + i, files[i]);
            string header = string.Format(headerTemplate, "poi_file", Path.GetFileName(fileName));
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(header);
            memStream.Write(headerbytes, 0, headerbytes.Length);


            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            byte[] buffer = new byte[1024];

            int bytesRead = 0;

            while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) != 0)
            {
                memStream.Write(buffer, 0, bytesRead);

            }
            memStream.Write(boundarybytes, 0, boundarybytes.Length);
            fileStream.Close();
            

            httpWebRequest2.ContentLength = memStream.Length;

            Stream requestStream = httpWebRequest2.GetRequestStream();

            memStream.Position = 0;
            byte[] tempBuffer = new byte[memStream.Length];
            memStream.Read(tempBuffer, 0, tempBuffer.Length);
            memStream.Close();
            requestStream.Write(tempBuffer, 0, tempBuffer.Length);
            requestStream.Close();


            WebResponse webResponse2 = httpWebRequest2.GetResponse();

            Stream stream2 = webResponse2.GetResponseStream();
            StreamReader reader2 = new StreamReader(stream2);

            String response = reader2.ReadToEnd();
            

            httpWebRequest2 = null;
            webResponse2 = null;
        }*/
        
    }
}
