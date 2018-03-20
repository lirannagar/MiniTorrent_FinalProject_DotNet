using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ClientApplication
{
    class DownloadFolder
    {

        public DownloadHandler DownloadStartedEvent;
        public DownloadHandler DownloadFinishedEvent;
        byte[] bytes;
        private string filename;
        private List<string> ipList;
        private int size;
        //private string downloadPath;

        static string downloadFolder;

        private void getPathFromXmlFile()
        {
            XmlTextReader xmlReader = new XmlTextReader(MainWindow.FileName);

            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    switch (xmlReader.Name)
                    {

                        case "DownloadFolder":
                            xmlReader.Read();
                            downloadFolder = xmlReader.Value;
                            xmlReader.Close();
                            return;
                    }
                }
            }
            xmlReader.Close();


        }

        public DownloadFolder(List<string> ipList, string filename, int size)
        {
            getPathFromXmlFile();
            this.ipList = ipList;
            this.filename = filename;
            this.size = size;
        }

        public void Start()
        {
            //delete file if exists
            if (File.Exists(downloadFolder + "\\" + filename))
                File.Delete(downloadFolder + "\\" + filename);

            FileStream fileStream = File.Open(downloadFolder + "\\" + filename, FileMode.Create);
            bytes = new byte[size];
            var downloads = new List<SinglePartDownload>();
            int chunksize = size / ipList.Count;
            int bytesRemaining = size;

            Download myDownload = new Download { FileName = filename, Ips = ipList, Size = size, Status = "Starting" };
            DownloadStartedEvent(myDownload);

            for (int i = 0; i < ipList.Count; i++)
            {
                var oldi = i;
                SinglePartDownload spd;

                if (i == ipList.Count - 1) // Last iteration
                {
                    spd = new SinglePartDownload(ipList[oldi], filename, size, chunksize * oldi, bytesRemaining);

                    downloads.Add(spd);
                    spd.task.Start();
                    break;
                }
                spd = new SinglePartDownload(ipList[oldi], filename, size, chunksize * oldi, chunksize);

                bytesRemaining -= chunksize;
                downloads.Add(spd);
                spd.task.Start();
            }

            // Wait for everyone to finish then save everything to file
            for (int i = 0; i < downloads.Count; i++)
            {
                Task.WaitAll(downloads[i].task);
                fileStream.Write(downloads[i].bytes, 0, downloads[i].bytes.Length);
            }
            fileStream.Close();
            DownloadFinishedEvent(myDownload);
        }

        class SinglePartDownload
        {
            private string ip;
            private string filename;
            private int size;
            private int offset;
            private int amount;
            public Task task { get; }
            public byte[] bytes { get; }

            public SinglePartDownload(string ip, string filename, int size, int offset, int amount)
            {
                this.ip = ip;
                this.filename = filename;
                this.size = size;
                this.offset = offset;
                this.amount = amount;

                bytes = new byte[amount];
                task = new Task(() =>
                {
                    DownloadSinglePart(this.ip, this.filename, this.size, this.offset, this.amount);
                });
            }

            private void DownloadSinglePart(string ip, string filename, int size, int offset, int amount)
            {

                try
                {
                    TcpClient tcpClient = new TcpClient(ip, 8005);
                    NetworkStream networkStream = tcpClient.GetStream();

                    StreamWriter streamWriter = new StreamWriter(networkStream);

                    /**
                     * Send and receive protocol:
                     * 
                     * Send each in a line:
                     *  file name
                     *  file size
                     *  byte offset
                     *  amount of bytes to send
                     * 
                     * Receive the data requested.
                     **/
                    streamWriter.WriteLine(filename);
                    streamWriter.Flush();
                    streamWriter.WriteLine(size);
                    streamWriter.Flush();
                    streamWriter.WriteLine(offset);
                    streamWriter.Flush();
                    streamWriter.WriteLine(amount);
                    streamWriter.Flush();

                    Debug.WriteLine("hi " + filename + " " + size + " " + offset + " " + amount);

                    int amntReceived = 0;
                    amntReceived += networkStream.Read(bytes, 0, bytes.Length);     // Reading requested data into bytes array
                    while (amntReceived < bytes.Length)
                        networkStream.Read(bytes, 0, bytes.Length);

                }
                catch (SocketException ex)
                {
                    Console.WriteLine(ex);
                }
            }
        }
    }
}
