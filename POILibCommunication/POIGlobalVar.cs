using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;

namespace POILibCommunication
{
    //Define all the global variables here
    public static class POIGlobalVar
    {
        public static POIKernel SystemKernel { get; set; }
        public static POIComServer SystemDataHandler { get; set; }
        public static Dictionary<string, POIUser> UserProfiles { get; set; }

        public static String ContentServerHome { get { return "http://192.168.0.143/"; } }
        public static String DNSServerHome { get { return "http://192.168.0.143/dnsServer/interface.php"; } }
        public static String KeywordsFileName { get { return "POI_Keywords.txt";} }
        public static String KeywordsFileType { get { return ".txt"; } }

        //public static POIUIScheduler Scheduler { get; set; }
    }
}
