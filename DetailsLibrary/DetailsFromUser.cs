using System;
using System.Collections.Generic;

namespace DetailsLibrary
{
    public class DetailsFromUser //: IDataErrorInfo
    {
        private List<FileDetails> files;

        public DetailsFromUser()
        {
            files = new List<FileDetails>();
        }

        public DetailsFromUser(string userName, string password, string ip, int portIn, int portOut)
        {
            UserName = userName;
            Password = password;
            IpLocalHost = ip;
            //PathUpload = upPath;
            //PathDownload = downPath;
            PortIn = portIn;
            PortOut = portOut;
            files = new List<FileDetails>();
        }


        public string UserName { set; get; }
        public string Password { set; get; }
        //public string PathDownload { set; get; }
        //public string PathUpload { set; get; }
        public string IpLocalHost { set; get; }
        public int PortIn { set; get; }
        public int PortOut { set; get; }
        public List<FileDetails> Files { get { return files; } }

        public void AddFile(string name, long numOfBytes)
        {
            files.Add(new FileDetails(name, numOfBytes));
        }

        public override string ToString()
        {
            string s = "userName: " + UserName + " password: " + Password + " \nip: " + IpLocalHost + " port: " + PortIn + "\n";
            s += "files:\n";
            foreach (FileDetails file in files)
            {
                s += file.NameFile + " " + file.NumberOfBytesOfFiles + "\n";
            }
            return s;
        }

        public void ClearFiles()
        {
            files.Clear();
        }
        //public string Error
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

        //public string this[string columnName]
        //{
        //    get
        //    {
        //        throw new NotImplementedException();
        //    }
        //}

    }//end of class DetailsFromUser

    public class FileDetails
    {
        public FileDetails(string name, long numberBytes)
        {
            NameFile = name;
            NumberOfBytesOfFiles = numberBytes;
        }
        public string NameFile { set; get; }
        public long NumberOfBytesOfFiles { set; get; }

    }//end of class FileDetails

    public class MySearches
    {
        private List<String> ipAddresses;

        public MySearches()
        {
            ipAddresses = new List<string>();
        }
        public MySearches(String fileName, long size, List<String> ips)
        {
            FileName = fileName;
            Size = size;
            this.ipAddresses = ips;
        }

        public string FileName { get; set; }
        public long Size { get; set; }
        public List<String> Ips { get { return ipAddresses; } }
        public void AddIp(String ip)
        {
            ipAddresses.Add(ip);
        }
        public void ClearIps()
        {
            ipAddresses.Clear();
        }
        public override string ToString()
        {
            string s = "file name " + FileName + " size " + Size + "\nips:\n";
            foreach (string ip in ipAddresses)
                s += ip + "\n";
            return s;


        }

    }//end of class MySearches

}//end of namespace
