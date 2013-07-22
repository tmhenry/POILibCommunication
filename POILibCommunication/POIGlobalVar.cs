using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Diagnostics;

using System.IO;
using System.Web.Script.Serialization;

namespace POILibCommunication
{
    public interface LogMessageDelegate
    {
        void logMessage(string msg);
    }

    //Define all the global variables here
    public static class POIGlobalVar
    {
        public static POIKernel SystemKernel { get; set; }
        public static POIComServer SystemDataHandler { get; set; }
        public static Dictionary<string, POIUser> UserProfiles { get; set; }
        public static Dictionary<string, POIUser> WebUserProfiles { get; set; }
        public static Dictionary<string, POIUser> WebConUserMap { get; set; }

        public static String ProxyServerIP { get; set; }
        public static int ProxyServerPort { get; set; }
        public static String ContentServerHome { get; set; }
        public static String DNSServerHome { get; set; }

        public static String KeywordsFileName { get { return "POI_Keywords.txt";} }
        public static String KeywordsFileType { get { return ".txt"; } }

        public static int MaxMobileClientCount { get; set; }

        public static LogMessageDelegate logDelegate { get; set; }

        //public static POIUIScheduler Scheduler { get; set; }

        public static void POIDebugLog(object msg)
        {
            //Debug.WriteLine(msg);

            if (logDelegate != null)
            {
                logDelegate.logMessage(msg.ToString());
            }
        }

        public static void LoadConfigFile()
        {
            string fn = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"poi_config");

            try
            {
                string configJson = File.ReadAllText(fn);
                JavaScriptSerializer jsonHandler = new JavaScriptSerializer();
                Dictionary<string, string> configDict = jsonHandler.Deserialize(configJson, typeof(Dictionary<string, string>)) 
                    as Dictionary<string, string>;

                ContentServerHome = configDict["ContentServer"];
                DNSServerHome = configDict["DNSServer"];
                ProxyServerIP = configDict["ProxyServerIP"];

                POIDebugLog(ContentServerHome);
                POIDebugLog(ProxyServerIP);
            }
            catch (Exception e)
            {
                //ContentServerHome = "http://192.168.0.130/POIWebService/contentServer/";
                //DNSServerHome = "http://192.168.0.130/POIWebService/dnsServer/interface.php";
                POIDebugLog(e);
            }
        }
    }
}
