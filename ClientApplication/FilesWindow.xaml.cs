using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Windows;
using DetailsLibrary;
using Newtonsoft.Json;
using System.Threading;
using System.Reflection;
using System.IO;
using System.Windows.Controls;
using System.Xml;
using System.Threading.Tasks;

namespace ClientApplication
{
    /// <summary>
    /// Interaction logic for FilesWindow.xaml
    /// </summary>
    public partial class FilesWindow : Window
    {

        private Socket _s;
        private DetailsFromUser _details = new DetailsFromUser();
        private string _downloadPath;
        private List<MySearches> _searchResults = new List<MySearches>();
        private List<Upload> _uploads = new List<Upload>();
        private List<Download> _downloads = new List<Download>();


        public FilesWindow()
        {
          
            InitializeComponent();
            GetDetailsFromXmlFile();
            _s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _s.Connect("10.100.102.9", 8080);//To change if not the computer of the server
            Thread.Sleep(2000);
         
            try
            {
                Title = "Welcome " + _details.UserName + " ! ! !";
                _s.Send(Encoding.Default.GetBytes(JsonConvert.SerializeObject(_details)));
                byte[] buf = new byte[1024];
                int rec = _s.Receive(buf, buf.Length, 0);
                if (rec < buf.Length) { Array.Resize(ref buf, rec); }
                if (Encoding.Default.GetString(buf) == "ERROR")
                { MessageBox.Show("Error loading file"); return; }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message); }
            //loading the content of listviews
            listViewSearchResult.ItemsSource = _searchResults;
            listViewUpload.ItemsSource = _uploads;
            listViewDownload.ItemsSource = _downloads;



            // Start listening to upload requests from other clients
            Task.Factory.StartNew(() =>
            {
                UploadFolder uFolder = new UploadFolder();
                uFolder.UploadStartedEvent += NewUploadStarted;
                uFolder.UploadFinishedEvent += UploadFinished;
                uFolder.TCPListen();
            });


        }//end of main c'tor


        private void NewUploadStarted(Upload upload)
        {
            Dispatcher.BeginInvoke(new Action(delegate
            {
                upload.Status = "Started";
                _uploads.Add(upload);
                listViewUpload.Items.Refresh();
            }));
        }
        private void UploadFinished(Upload upload)
        {
            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                upload.Status = "Finished";
                listViewUpload.Items.Refresh();
            }));

        }
        private void NewDownloadStarted(Download download)
        {
            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                download.StartTime = DateTime.Now;
                _downloads.Add(download);
                listViewDownload.Items.Refresh();
            }));
        }
        private void DownloadFinished(Download download)
        {
            Dispatcher.BeginInvoke(new Action(delegate ()
            {
                download.TimeTaken = DateTime.Now - download.StartTime;
                download.Kbps = (int)((download.Size / 1000) / download.TimeTaken.TotalSeconds);
                download.Status = "Finished";
                listViewDownload.Items.Refresh();
            }));

        }

        private void DownloadFile(List<string> list, string filename, int size)
        {
            Task.Factory.StartNew(() =>
            {
                DownloadFolder d = new DownloadFolder(list, filename, size);
                d.DownloadFinishedEvent += DownloadFinished;
                d.DownloadStartedEvent += NewDownloadStarted;
                d.Start();
            });
        }


       
        private void GetDetailsFromXmlFile()
        {
            XmlTextReader xmlReader = new XmlTextReader(MainWindow.FileName);

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
                        case "DownloadFolder":
                            xmlReader.Read();
                            _downloadPath = xmlReader.Value;
                            xmlReader.Close();
                            return;
                    }
                }
            }
            xmlReader.Close();

        }

        private void log_out_button_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(MainWindow.FileName))
            {
                File.Delete(MainWindow.FileName);
            }
            new MainWindow();
            Close();
        }

        private void searchButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                String nameFileSearchText = textBox_search.Text;
                if (nameFileSearchText == "")
                {
                    MessageBox.Show("You must enter name!");
                    return;
                }
                GetDetailsFromXmlFile();


                _s.Send(Encoding.Default.GetBytes(JsonConvert.SerializeObject(_details)));
                Thread.Sleep(1000);
                _s.Send(Encoding.Default.GetBytes(nameFileSearchText));

                byte[] buf = new byte[1024];

                int rec = _s.Receive(buf, buf.Length, 0);
                if (rec < buf.Length)
                {
                    Array.Resize<byte>(ref buf, rec);
                }
                try
                {
                    List<MySearches> res = JsonConvert.DeserializeObject<List<MySearches>>(Encoding.Default.GetString(buf));
                    _searchResults.Clear();
                    foreach (MySearches ms in res)
                    {
                        _searchResults.Add(ms);
                        //MessageBox.Show(msr.ToString());
                    }
                    listViewSearchResult.Items.Refresh();
                }
                catch
                {
                    MessageBox.Show("Not found!");
                    return;
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            try
            {
                String fileName = textBox_search.Text;
                if (fileName == "")
                {
                    MessageBox.Show("Enter a file name!");
                    return;
                }
                GetDetailsFromXmlFile();


                _s.Send(Encoding.Default.GetBytes(JsonConvert.SerializeObject(_details)));
                Thread.Sleep(1000);
                _s.Send(Encoding.Default.GetBytes(fileName));

                byte[] buf = new byte[1024];

                int rec = _s.Receive(buf, buf.Length, 0);
                if (rec < buf.Length)
                {
                    Array.Resize<byte>(ref buf, rec);
                }
                try
                {
                    List<MySearches> res = JsonConvert.DeserializeObject<List<MySearches>>(Encoding.Default.GetString(buf));
                    _searchResults.Clear();
                    foreach (MySearches ms in res)
                    {
                        _searchResults.Add(ms);
                        //MessageBox.Show(msr.ToString());
                    }
                    listViewSearchResult.Items.Refresh();
                }
                catch
                {
                    MessageBox.Show(fileName + "does not exiest");
                }


            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void listViewSearchResult_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            MySearches msr = (MySearches)listViewSearchResult.SelectedItem;
            DownloadFile(msr.Ips, msr.FileName, (int)msr.Size);
        }

        private void listViewDownload_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Download md = (Download)listViewDownload.SelectedItem;
            // Check if finished
            if (!md.Status.Equals("Finished")) return;
            // Check if dll
            if (!md.FileName.EndsWith(".dll")) return;
            

            //This is for Reflection 
            // Load dll from download folder 
            Assembly a = Assembly.LoadFrom(_downloadPath + "\\" + md.FileName);
            Type[] allTypes = a.GetTypes();
            foreach (Type t in allTypes)
            {
                AuthorAttribute author = (AuthorAttribute)Attribute.GetCustomAttribute(t, typeof(AuthorAttribute));

                if (author == null)
                {
                    MessageBox.Show("The attribute in this .dll file is not supported. Must be of type AuthorAttribute", "Unknown attribute");
                }
                else
                {
                    if (author.Name.Equals("Yael"))
                    {
                        MethodInfo m = t.GetMethod("Panic");
                        object obj = Activator.CreateInstance(t);

                        object[] objects = { 42 };
                        m.Invoke(obj, objects);
                    }
                    else if (author.Name.Equals("Daniel"))
                    {
                        MethodInfo m = t.GetMethod("GoOnVacation");
                        object obj = Activator.CreateInstance(t);

                        object[] objects = { "Colorado" };
                        m.Invoke(obj, objects);
                    }
                    else if (author.Name.Equals("Cookie"))
                    {
                        MethodInfo m = t.GetMethod("DigInTheYard");
                        object obj = Activator.CreateInstance(t);
                        m.Invoke(obj, null);
                    }
                    else
                    {
                        MessageBox.Show("Class written by unknown author and will not be executed", "Unknown Author");
                    }

                }
            }
        }

        //private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        //{
        //    s.Send(Encoding.Default.GetBytes(JsonConvert.SerializeObject(new DetailsFromUser())));

        //    s.Close();
        //    s.Dispose();
        //}


    }// end of class FilesWindow

}//end of namespace
