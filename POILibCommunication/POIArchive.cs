using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;

namespace POILibCommunication
{
    public class POIArchive
    {
        static public String ArchiveHome 
        {
            get { return System.Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments); }
        }
    }
}
