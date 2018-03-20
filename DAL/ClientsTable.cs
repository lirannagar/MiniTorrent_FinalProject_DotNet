using System;
using System.Collections.Generic;
using System.Linq;
using DetailsLibrary;

namespace DAL
{
    public class ClientsTable
    {

        private static readonly ClientsTable instance = new ClientsTable();
        public static ClientsTable Instance { get { return instance; } }
        private ClientsTable() { }

        //register (add) a new user 
        public void RegisterUser(string newUserName, string password)
        {
            if (!IsRegistered(newUserName))
            {
                using (AfekaMiniTorrent_DataBaseServerDataContext dataBase = new AfekaMiniTorrent_DataBaseServerDataContext())
                {
                    Client newClient = new Client();
                    newClient.UserName = newUserName;
                    newClient.Password = password;
                    newClient.Active = 0;
                    dataBase.Clients.InsertOnSubmit(newClient);
                    //dataBase.Clients.ExecuteDynamicInsert(newClient);
                    dataBase.SubmitChanges();
                }
            }
            else
            {
                throw new Exception("User Name already exists!");
            }
        }

        //check if a certain user is already registered in system 
        public bool IsRegistered(string userName)
        {
            AfekaMiniTorrent_DataBaseServerDataContext dataBase = new AfekaMiniTorrent_DataBaseServerDataContext();
            var registered = from r in dataBase.Clients where r.UserName == userName select r;
            //foreach (var client in dataBase.Clients) { if (client.UserName.Equals(userName)) { count++; } }
            var l = registered.ToList();
            return (registered.Count() != 0);
        }

        public bool IsClientValid(String userName, string password)
        {
            using (AfekaMiniTorrent_DataBaseServerDataContext dataBase = new AfekaMiniTorrent_DataBaseServerDataContext())
            {
                if (IsRegistered(userName))
                {
                    foreach (var client in dataBase.Clients)
                    {
                        if (client.UserName == userName)
                        {

                            if (client.Password == password)
                            {
                                return true;
                            }
                            else
                            {
                                throw new Exception("Wrong password!");
                            }
                        }

                    }
                }
                else { throw new Exception("There is no such user name!"); }
            }
            return false;
        }

        //print all peers
        public String PrintAccounts()
        {
            String all = "";
            int count = 0;
            using (AfekaMiniTorrent_DataBaseServerDataContext dataBase = new AfekaMiniTorrent_DataBaseServerDataContext())
            {
                foreach (var client in dataBase.Clients)
                {
                    count++;
                    String isActive = "Active Now";
                    if (client.Active == 0) { isActive = "Not Active Now"; }
                    all += "#" + count + ":: User Name: " + client.UserName + ", Password: " + client.Password + ", " + isActive + "\n";
                }
            }
            return all;
        }

        //return a list of all clients
        public List<Client> GetAllClients()
        {
            AfekaMiniTorrent_DataBaseServerDataContext dataBase = new AfekaMiniTorrent_DataBaseServerDataContext();
            var clients = from client in dataBase.Clients select client;
            List<Client> clientsList = new List<Client>();
            foreach (var client in clients)
            {
                clientsList.Add(client);
            }
            return clientsList;
        }

        //return a list of all active clients
        public List<Client> GetActiveClients()
        {
            AfekaMiniTorrent_DataBaseServerDataContext dataBase = new AfekaMiniTorrent_DataBaseServerDataContext();
            List<Client> activeClientsList = new List<Client>();
            //var clients = from client in dataBase.Clients select client;
            var clients = from client in dataBase.Clients where client.Active == 1 select client;
            foreach (var client in clients)
            {
                 activeClientsList.Add(client); 
            }
            return activeClientsList;
        }

        public int GetNumOfAllClients() { return GetAllClients().Count(); }
        //public int GetNumOfActiveClients() { return GetActiveClients().Count(); }
        public int GetNumOfActiveClients()
        {
            AfekaMiniTorrent_DataBaseServerDataContext dataBase = new AfekaMiniTorrent_DataBaseServerDataContext();
            var activeClientsList = from client in dataBase.Clients where client.Active == 1 select client;
            return activeClientsList.Count();
        }

        public void ActivateUser(string userName, String ip, List<FileDetails> userFiles)
        {
            using (AfekaMiniTorrent_DataBaseServerDataContext dataBase = new AfekaMiniTorrent_DataBaseServerDataContext())
            {
                //dataBase.Clients.Single(c => c.UserName == userName).Active = 1;
                Client client = dataBase.Clients.Single(c => c.UserName == userName);
                client.Active = 1;
                dataBase.SubmitChanges();
            }

            FilesTable.Instance.AddFiles(ip, userName, userFiles);
        }

        public void DeactivateUser(string userName)
        {
            using (AfekaMiniTorrent_DataBaseServerDataContext dataBase = new AfekaMiniTorrent_DataBaseServerDataContext())
            {
                dataBase.Clients.Single(c => c.UserName == userName).Active = 0;
                dataBase.SubmitChanges();
            }
            FilesTable.Instance.DeleteFiles(userName);
        }





    }
}
