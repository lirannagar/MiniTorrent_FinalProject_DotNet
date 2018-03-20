using System;
using System.Text;
using System.Windows;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using System.Xml.Serialization;
using DetailsLibrary;
using Newtonsoft.Json;

namespace ClientApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public static String FileName = "MyConfig.xml";
        private String _uploadPath;
        private String _downloadPath;
        private DetailsFromUser _details;
        private XmlSerializer _serializerObj;
        private Socket _s;




        public MainWindow()
        {
            try
            {
                InitializeComponent();
                Hide();

                //for creating xml file

                //{
                //    _uploadPath = "C:\\Users\\liran\\Desktop";
                //    _downloadPath = "C:\\Users\\liran\\Desktop";
                //    _details = new DetailsFromUser("Vika", "777777", "10.100.102.9", 8005, 8006);
                //    CreateXmlFile();
                //}

                _details = new DetailsFromUser();
                _s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _s.Connect("10.100.102.9", 8085); //insert changed ip here


                if (File.Exists(FileName))
                {
                    GetDetailsFromXmlFile();
                    _details.IpLocalHost = GetIpAddress().ToString();

                    if (!Directory.Exists(_uploadPath))
                    {
                        Directory.CreateDirectory(_uploadPath);
                    }
                    if (!Directory.Exists(_downloadPath))
                    {
                        Directory.CreateDirectory(_downloadPath);
                    }
                    GetFilesFromPath();
                    var e = _details;
                    _s.Send(Encoding.Default.GetBytes(JsonConvert.SerializeObject(_details)));

                    byte[] buf = new byte[1024];

                    int rec = _s.Receive(buf, buf.Length, 0);
                    if (rec < buf.Length)
                    {
                        Array.Resize<byte>(ref buf, rec);
                    }
                    String msg = Encoding.Default.GetString(buf);
                    if (msg == "OK")
                    {
                        CreateXmlFile();
                       
                        new FilesWindow().Show();
                        Close();
                    }
                    else
                    {
                        Show();
                        //errorLabel.Content = msg;
                    }

                }
                else
                    Show();
            }
            catch (Exception ex)
            {
                Show();
                MessageBox.Show(ex.Message);
            }

        }//end of MainWindow()

        private void GetDetailsFromXmlFile()
        {
            XmlTextReader xmlReader = new XmlTextReader(FileName);

            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element)
                {
                    switch (xmlReader.Name)
                    {
                        case "UserName":
                            xmlReader.Read();
                            _details.UserName = xmlReader.Value;
                            break;
                        case "Password":
                            xmlReader.Read();
                            _details.Password = xmlReader.Value;
                            break;
                        case "Ip":
                            xmlReader.Read();
                            _details.IpLocalHost = xmlReader.Value;
                            break;
                        case "UploadFolder":
                            xmlReader.Read();
                            _uploadPath = xmlReader.Value;

                            break;
                        case "DownloadFolder":
                            xmlReader.Read();
                            _downloadPath = xmlReader.Value;

                            break;
                        case "PortIn":
                            xmlReader.Read();
                            _details.PortIn = int.Parse(xmlReader.Value);
                            break;
                        case "PortOut":
                            xmlReader.Read();
                            _details.PortOut = int.Parse(xmlReader.Value);
                            break;
                    }
                }
            }
            xmlReader.Close();
        }

        private void CreateXmlFile()
        {
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;
            xmlSettings.IndentChars = "\t";
            using (XmlWriter xmlWriter = XmlWriter.Create(FileName, xmlSettings))
            {

                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Client");

                xmlWriter.WriteStartElement("UserName");
                xmlWriter.WriteString(_details.UserName);
                xmlWriter.WriteEndElement();//UserName

                xmlWriter.WriteStartElement("Password");
                xmlWriter.WriteString(_details.Password);
                xmlWriter.WriteEndElement();//Password

                xmlWriter.WriteStartElement("Ip");
                xmlWriter.WriteString(_details.IpLocalHost);
                xmlWriter.WriteEndElement();//Ip

                xmlWriter.WriteStartElement("UploadFolder");
                xmlWriter.WriteString(_uploadPath);
                xmlWriter.WriteEndElement();//UploadFolder

                xmlWriter.WriteStartElement("DownloadFolder");
                xmlWriter.WriteString(_downloadPath);
                xmlWriter.WriteEndElement();//DownloadFolder

                xmlWriter.WriteStartElement("PortIn");
                xmlWriter.WriteString("8005");
                xmlWriter.WriteEndElement();//PortIn

                xmlWriter.WriteStartElement("PortOut");
                xmlWriter.WriteString("8006");
                xmlWriter.WriteEndElement();//PortOut

                xmlWriter.WriteEndDocument();//Doc
                xmlWriter.Close();
            }
        }

        private void uploadBrowse_button_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                System.Windows.Forms.DialogResult unused = dialog.ShowDialog();
                uploadPath_textBox.Text = dialog.SelectedPath;
                success_upload.Visibility = Visibility.Visible;
            }
        }

        private void downloadBrowse_button_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.ShowDialog();
                downloadPath_textBox.Text = dialog.SelectedPath;
                success_download.Visibility = Visibility.Visible;
            }
        }

        private void login_button_Click(object sender, RoutedEventArgs e)
        {
            _details.UserName = userName_textBox.Text;
            _details.Password = password_textBox.Password;
            _uploadPath = uploadPath_textBox.Text;
            _downloadPath = downloadPath_textBox.Text;
            _details.PortIn = 8005;
            _details.PortOut = 8006;

            try
            {
                InputCheck(_details.UserName, _details.Password, _downloadPath, _uploadPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Confirmation", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _details.IpLocalHost = GetIpAddress().ToString();
            GetFilesFromPath();
            _s.Send(Encoding.Default.GetBytes(JsonConvert.SerializeObject(_details)));
            byte[] buf = new byte[1024];
            int rec = _s.Receive(buf, buf.Length, 0);
            if (rec < buf.Length)
            {
                Array.Resize<byte>(ref buf, rec);
            }
            String msg = Encoding.Default.GetString(buf);
            Console.WriteLine(msg);
            if (msg == "OK")
            {
                CreateXmlFile();
                new FilesWindow().Show();
                Close();
            }
            else
            {
                MessageBox.Show(msg);
            }

            //SaveToFile();
        }

        private void SaveToFile()
        {
            using (var srFileStream = new FileStream(FileName, FileMode.Create))
            {
                _serializerObj = new XmlSerializer(typeof(DetailsFromUser));
                _serializerObj.Serialize(srFileStream, _details);
            }
        }

        private void InputCheck(string inputUserName, string inputPassword, string pathDownload, string pathUpload)
        {
            if (inputUserName == "" && inputPassword == "")
                throw new Exception("You must enter username and password before uploading...");
            else if (inputUserName == "")
                throw new Exception("You must enter username before uploading...");
            else if (inputPassword == "")
                throw new Exception("You must enter password before uploading...");
            else if (pathDownload == "" && pathUpload == "")
                throw new Exception("You must choose paths...");
            else if (pathDownload == "")
                throw new Exception("You must choose download path...");
            else if (pathUpload == "")
                throw new Exception("You must choose upload path...");
        }

        public static IPAddress GetIpAddress()
        {
            IPAddress[] hostAddresses = Dns.GetHostAddresses("");
            foreach (IPAddress hostAddress in hostAddresses)
            {
                if (hostAddress.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(hostAddress) && !hostAddress.ToString().StartsWith("169.254."))
                    return hostAddress;
            }
            return null;
        }

        public void GetFilesFromPath()
        {
            _details.ClearFiles();
            var files = new DirectoryInfo(_uploadPath).GetFiles();
            for (int i = 0; i < files.Length; i++)
            {
                _details.AddFile(files[i].Name, files[i].Length);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _s.Close();
            _s.Dispose();
        }

    }//end of class

}//end of namespace
