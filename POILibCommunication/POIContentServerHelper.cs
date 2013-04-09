using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net;
using System.Net.Http;
using POILibCommunication;
using System.IO;

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
            String reqUrl = ContentServerHome + "content.php?"
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

        static public void uploadContent(int presId, string fileName)
        {
            String reqUrl = ContentServerHome + "store.php?" + "presId=" + presId;

            if (webClient == null) webClient = new WebClient();

            try
            {
                byte[] response = webClient.UploadFile(reqUrl, "POST", fileName);
                string str = ASCIIEncoding.UTF8.GetString(response);

                Console.WriteLine(":");
            }
            catch (WebException ex)
            {
                Console.WriteLine(ex);
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

            //For sending extra form data
            /*
            string formdataTemplate = "\r\n--" + boundary +
            "\r\nContent-Disposition: form-data; name=\"{0}\";\r\n\r\n{1}";

            foreach (string key in nvc.Keys)
            {
                string formitem = string.Format(formdataTemplate, key, nvc[key]);
                byte[] formitembytes = System.Text.Encoding.UTF8.GetBytes(formitem);
                memStream.Write(formitembytes, 0, formitembytes.Length);
            }*/


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
        }
        
    }
}
