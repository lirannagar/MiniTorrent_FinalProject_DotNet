using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientApplication
{

    public delegate void UploadHandler(Upload upload);
    public delegate void DownloadHandler(Download myDownload);

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class AuthorAttribute : Attribute
    {
        private string name;
        public double version;
        public string Name { get { return name; } }
        public AuthorAttribute(string name)
        {
            this.name = name;
            version = 1.0;
        }
    }
    public class Download
    {
        public List<String> Ips { get; set; }
        public string FileName { get; set; }
        public int Size { get; set; }
        public string Status { get; set; }
        public int Kbps { get; set; }
        public TimeSpan TimeTaken { get; set; }
        public DateTime StartTime { get; set; }
    }

    public class Upload
    {
        public string IP { get; set; }
        public string FileName { get; set; }
        public int Size { get; set; }
        public string Status { get; set; }
    }

}
