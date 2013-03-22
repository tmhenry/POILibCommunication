using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Net.Sockets;
using System.Web.Script.Serialization;
using System.Net;
using System.IO;

namespace POILibCommunication
{
    public class POIWebService
    {
        private string _name;
        private string _description;

        private string dnsServerHost;
        private Socket mySocket;
        private IPEndPoint localEP;

        private int servicePort = 81;
        //public static String DNSServer = @"http://pipeofinsight.elasticbeanstalk.com/dnsServer/interface.php";
        public static String DNSServer = @"http://192.168.0.143/dnsServer/interface.php";

        public Socket ServiceSocket
        {
            get { return mySocket; }
            set { mySocket = value; }
        }

        private enum RequestType
        {
            Publish = 0,
            Remove,
            ResolveByServerName,
            ResolveByUserRight,
            RequestRight,
            Unknown
        }

        public POIWebService(string name, string desc, string img)
        {
            _name = name;
            _description = desc;

            localEP = new IPEndPoint(IPAddress.Any, servicePort);

            mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            mySocket.Bind(localEP);
            localEP = (IPEndPoint)mySocket.LocalEndPoint;

            dnsServerHost = DNSServer;
        }

        public void sendRequest(string msg)
        {
            HttpWebRequest myRequest = (HttpWebRequest)WebRequest.Create(dnsServerHost);
            myRequest.Method = "POST";
            myRequest.Proxy = null;

            
            //Prepare the post data
            byte[] postBytes = Encoding.UTF8.GetBytes(msg);
            myRequest.ContentType = "application/x-www-form-urlencoded";
            myRequest.ContentLength = postBytes.Length;
            Stream dataStream = myRequest.GetRequestStream();
            dataStream.Write(postBytes, 0, postBytes.Length);
            dataStream.Close();
            
            WebResponse myResponse = myRequest.GetResponse();
            StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
            string responseString = sr.ReadToEnd();
            sr.Close();
            myResponse.Close();
            
            Console.WriteLine(responseString);

            //return responseString;

            /*
            myRequest.BeginGetResponse
            (
                new AsyncCallback(ResponseCB),
                myRequest
            );*/
        }

        private void ResponseCB(IAsyncResult ar)
        {
            WebRequest req = ar.AsyncState as WebRequest;
            WebResponse myResponse = req.EndGetResponse(ar);

            StreamReader sr = new StreamReader(myResponse.GetResponseStream(), System.Text.Encoding.UTF8);
            string responseString = sr.ReadToEnd();

            sr.Close();
            myResponse.Close();

            Console.WriteLine(responseString);
        }

        public Dictionary<string, string> parseResponseSingle(string response)
        {
            JavaScriptSerializer jsonParser = new JavaScriptSerializer();
            Dictionary<string, string> myDic = jsonParser.Deserialize<Dictionary<string, string>>(response);

            return myDic;
        }

        public List<Dictionary<string, string>> parseResponseArray(string response)
        {
            JavaScriptSerializer jsonParser = new JavaScriptSerializer();
            List<Dictionary<string, string>> myDicList = jsonParser.Deserialize<List<Dictionary<string, string>>>(response);

            return myDicList;
        }

        public void StartService()
        {
            //Prepare the POST data
            JavaScriptSerializer jsonParser = new JavaScriptSerializer();

            Dictionary<string, string> serviceEntry = new Dictionary<string, string>();
            serviceEntry.Add(@"name", _name);
            serviceEntry.Add(@"description",_description);

            IPAddress[] localAddrs = Dns.GetHostAddresses(Dns.GetHostName());
            IPAddress ip4Addr = localAddrs[0];

            foreach (IPAddress curIP in localAddrs)
            {
                if (curIP.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    ip4Addr = curIP;
                }
            }

            serviceEntry.Add(@"ip", ip4Addr.ToString());
            serviceEntry.Add(@"port", localEP.Port.ToString());

            string serviceEntryStr = jsonParser.Serialize(serviceEntry);
            Console.WriteLine(serviceEntryStr);

            string postDataStr = @"type="+ (int) RequestType.Publish + "&data=" + serviceEntryStr;

            try
            {
                sendRequest(postDataStr);
            }
            catch(WebException e)
            {
                Console.WriteLine(e.Message);
            }
            
        }

        public void StopService()
        {
            //Prepare the POST data
            JavaScriptSerializer jsonParser = new JavaScriptSerializer();

            Dictionary<string, string> postData = new Dictionary<string, string>();

            Dictionary<string, string> serviceEntry = new Dictionary<string, string>();
            serviceEntry.Add(@"name", _name);
            string serviceEntryStr = jsonParser.Serialize(serviceEntry);


            string postDataStr = @"type="+ (int)RequestType.Remove + "&data=" + serviceEntryStr;

            try
            {
                sendRequest(postDataStr);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
