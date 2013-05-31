using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Diagnostics;

namespace POILibCommunication
{
    //Define all the global variables here
    public static class POIGlobalVar
    {
        public static POIKernel SystemKernel { get; set; }
        public static POIComServer SystemDataHandler { get; set; }
        public static Dictionary<string, POIUser> UserProfiles { get; set; }
        public static Dictionary<string, POIUser> WebUserProfiles { get; set; }

        public static String ContentServerHome { get { return "http://192.168.0.130/"; } }
        public static String DNSServerHome { get { return "http://192.168.0.130/dnsServerTest/interface.php"; } }

        public static String KeywordsFileName { get { return "POI_Keywords.txt";} }
        public static String KeywordsFileType { get { return ".txt"; } }

        //public static POIUIScheduler Scheduler { get; set; }

        public static void POIDebugLog(object msg)
        {
            Debug.WriteLine(msg);
        }
    }
}
