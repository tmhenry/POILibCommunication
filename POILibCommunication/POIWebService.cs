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

        public static Socket ServiceSocket
        {
            get
            {
                return Instance.mySocket;
            }
        }

        private enum RequestType
        {
            Publish = 0,
            Remove,
            ResolveByServerName,
            ResolveByUserRight,
            RequestRight,
            Image,
            ResolveServerAddr,
            UploadPresentation,
            DeletePresentation,
            GetPresentationCover,
            GetPresentationList,
            GetAccessiblePresentationList,
            GetServerAddr,
            KeywordsSearch,
            CreateSession,
            SearchSession,
            EndSession,
            Unknown
        }

        
        //Shared instance for singleton implementation
        private static POIWebService sharedInstance;

        private static POIWebService Instance
        {
            get
            {
                if (sharedInstance == null)
                {
                    sharedInstance = new POIWebService();
                }

                return sharedInstance;
            }
        }

        //Private constructor for web service
        private POIWebService()
        {
            dnsServerHost = POIGlobalVar.DNSServerHome;
        }

        //Start the TCP handling service
        private static void StartTCPService()
        {
            //Bind the port and start the TCP handling
            Instance.localEP = new IPEndPoint(IPAddress.Any, Instance.servicePort);

            Instance.mySocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Instance.mySocket.Bind(Instance.localEP);
            Instance.localEP = (IPEndPoint)Instance.mySocket.LocalEndPoint;
        }

        //Public functions for services
        public static void StartService(string name, string desc, string img)
        {
            Instance._name = name;
            Instance._description = desc;

            StartTCPService();

            //Register the server to the POI DNS server
            JavaScriptSerializer jsonParser = new JavaScriptSerializer();

            Dictionary<string, string> serviceEntry = new Dictionary<string, string>();
            serviceEntry.Add(@"name", name);
            serviceEntry.Add(@"description", desc);

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
            serviceEntry.Add(@"port", Instance.servicePort.ToString());

            string serviceEntryStr = jsonParser.Serialize(serviceEntry);
            Console.WriteLine(serviceEntryStr);

            string postDataStr = @"type=" + (int)RequestType.Publish + "&data=" + serviceEntryStr;

            try
            {
                sendRequest(postDataStr);
            }
            catch (WebException e)
            {
                Console.WriteLine(e.Message);
            }            
        }

        
        public static void StopService()
        {
            //Unregister the server to the DNS server
            JavaScriptSerializer jsonParser = new JavaScriptSerializer();

            Dictionary<string, string> postData = new Dictionary<string, string>();

            Dictionary<string, string> serviceEntry = new Dictionary<string, string>();
            serviceEntry.Add(@"name", Instance._name);
            string serviceEntryStr = jsonParser.Serialize(serviceEntry);


            string postDataStr = @"type=" + (int)RequestType.Remove + "&data=" + serviceEntryStr;

            try
            {
                sendRequest(postDataStr);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        //Register the current presentation and retrieve an presentation ID
        public static int UploadPresentation(Dictionary<string,string> presEntry)
        {

            //Prepare the POST data
            JavaScriptSerializer jsonParser = new JavaScriptSerializer();

            string presEntryStr = jsonParser.Serialize(presEntry);
            string postDataStr = @"type=" + (int)RequestType.UploadPresentation + "&data=" + presEntryStr;

            //Set the default presId when presentation is not inserted
            int presId = -1;

            try
            {
                string response = sendRequest(postDataStr);
                if (response != null)
                {
                    Dictionary<string, string> dict = parseResponseSingle(response);
                    presId = Int32.Parse(dict["PresId"]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return presId;
        }

        public static int CreateSession(Dictionary<string, string> sessionEntry)
        {
            //Prepare the POST data
            JavaScriptSerializer jsonParser = new JavaScriptSerializer();

            string sesEntryStr = jsonParser.Serialize(sessionEntry);
            string postDataStr = @"type=" + (int)RequestType.CreateSession + "&data=" + sesEntryStr;

            int sessionId = -1;

            try
            {
                string response = sendRequest(postDataStr);
                if (response != null)
                {
                    Dictionary<string, string> dict = parseResponseSingle(response);
                    sessionId = Int32.Parse(dict["SessionId"]);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return sessionId;
        }

        public static void EndSession(Dictionary<string, string> sessionEntry)
        {
            //Prepare the POST data
            JavaScriptSerializer jsonParser = new JavaScriptSerializer();

            string sesEntryStr = jsonParser.Serialize(sessionEntry);
            string postDataStr = @"type=" + (int)RequestType.EndSession + "&data=" + sesEntryStr;

            try
            {
                string response = sendRequest(postDataStr);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
        //Utility functions for web service, return null response if error occurs
        public static string sendRequest(string msg)
        {

            //Use the webclient to make post request to the DNS server
            using (WebClient wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
                wc.Proxy = null;

                try
                {
                    string HtmlResult = wc.UploadString(Instance.dnsServerHost, msg);
                    return HtmlResult;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    return null;
                }
            }
        }


        private static Dictionary<string, string> parseResponseSingle(string response)
        {
            JavaScriptSerializer jsonParser = new JavaScriptSerializer();
            Dictionary<string, string> myDic = jsonParser.Deserialize<Dictionary<string, string>>(response);

            return myDic;
        }

        private static List<Dictionary<string, string>> parseResponseArray(string response)
        {
            JavaScriptSerializer jsonParser = new JavaScriptSerializer();
            List<Dictionary<string, string>> myDicList = jsonParser.Deserialize<List<Dictionary<string, string>>>(response);

            return myDicList;
        }
        
    }
}
