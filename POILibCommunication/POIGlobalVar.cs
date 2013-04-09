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

        public static String ContentServerHome { get { return "http://128.174.241.13/"; } }
        public static String DNSServerHome { get { return "http://localhost/dnsServer/interface.php"; } }
        //public static POIUIScheduler Scheduler { get; set; }
    }
}
