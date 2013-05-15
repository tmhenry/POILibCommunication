using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace POILibCommunication
{
    public class POITimestamp
    {
        //Convert unix style timestamp to C# date
        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return origin.AddSeconds(timestamp);
        }

        //Convert C# date timestamp to unix 
        public static double ConvertToUnixTimestamp(DateTime date)
        {
            DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            TimeSpan diff = date.ToUniversalTime() - origin;
            return diff.TotalSeconds;
        }
    }
}
