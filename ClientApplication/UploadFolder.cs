using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ClientApplication
{
    class UploadFolder
    {
        public UploadHandler UploadStartedEvent;
        public UploadHandler UploadFinishedEvent;


        static string path = GetPathFromXmlFile();
        static int port = 8005;

        private static string GetPathFromXmlFile()
        {
            XmlTextReader xmlReader = new XmlTextReader(MainWindow.FileName);

            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    switch (xmlReader.Name)
                    {

                        case "UploadFolder":
                            xmlReader.Read();
                            string temp = xmlReader.Value;
                            xmlReader.Close();
                            return temp;

                        default:
                            break;

                    }
                }
            }
            xmlReader.Close();
            return null;

        }

        public void TCPListen()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
            //Console.WriteLine("Server is now listening to incoming connections on port " + port);
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                //Console.WriteLine("Accepted");

                Task.Factory.StartNew(() => TaskUpload(client));
            }
        }

        private void TaskUpload(TcpClient client)
        {
            //Console.WriteLine("New upload request received from " + client.Client.RemoteEndPoint.ToString());

            NetworkStream networkStream = client.GetStream();
            StreamReader streamReader = new StreamReader(networkStream);

            var fileName = streamReader.ReadLine();
            var fileSize = streamReader.ReadLine();
            var startByte = streamReader.ReadLine();
            var byteAmount = streamReader.ReadLine();

            //MessageBox.Show("hi " + fileName + "  " + fileSize + "  " + startByte + "  " + byteAmount);

            Upload upload = new Upload
            {
                FileName = fileName,
                Size = int.Parse(fileSize),
                IP = client.Client.RemoteEndPoint.ToString(),
                Status = "Uploading"
            };
            UploadStartedEvent(upload);

            //Console.WriteLine("File name is " + fileName);
            //Console.WriteLine("File size is " + fileSize);
            //Console.WriteLine("start byte: {0} amount of bytes: {1} ", startByte, byteAmount);

            byte[] bytes = new byte[int.Parse(byteAmount)];
            string filepath = path + "\\" + fileName;

            bool finishedReading = false;

            while (!finishedReading)
            {
                try
                {
                    using (BinaryReader reader = new BinaryReader(new FileStream(filepath, FileMode.Open)))
                    {
                        reader.BaseStream.Seek(long.Parse(startByte), SeekOrigin.Begin);
                        reader.Read(bytes, 0, bytes.Length);
                    }
                    finishedReading = true;
                    networkStream.Write(bytes, 0, bytes.Length);
                }
                catch //(IOException e)
                {
                    finishedReading = false;
                }
            }
            UploadFinishedEvent(upload);
        }

    }
}
