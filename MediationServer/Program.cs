using System;
using System.Collections.Generic;
using System.Net.Sockets;
using DAL;
using DetailsLibrary;

namespace MediationServer
{
    class Server
    {
        static SocketHandler _fileRequestListener;//listener for file requests
        static SocketHandler _connectionListener;//listener for connection 
        //static List<Client> _clients;
        public static readonly int FileRequest = 1;
        public static readonly int Connection = 0;

        static void Main()
        {

            _fileRequestListener = new SocketHandler(8080);
            _connectionListener = new SocketHandler(8085);
            _connectionListener.SocketAcceptedConnectionEvent += l_socketAcceptedConnection;
            _fileRequestListener.SocketAcceptedFileRequestEvent += l_socketAcceptedFileRequest;
            //_clients = new List<Client>();
            Console.WriteLine("\n\n************************************* VIKA and LIRAN Afeka Mini Torrent Server***************************************\n");
            Console.WriteLine("Regisered accounts:");
            FileDetails file = new FileDetails("bla", 640);
            List<FileDetails> fl = new List<FileDetails>();
            fl.Add(file);
            ClientsTable.Instance.ActivateUser("Dika", "10.0.0.3", fl);
            Console.WriteLine(ClientsTable.Instance.PrintAccounts());
            _fileRequestListener.Start();
            _connectionListener.Start();
            Console.WriteLine("SERVER STARTED LISTENING...");

            //Console.WriteLine(ClientsTable.Instance.PrintAccounts());
            Console.Read();

        }//end of main

        private static void l_socketAcceptedConnection(Socket s)
        {
            Client client = new Client(s, Connection);
            client.ClientRecievedEvent += client_Received;
            client.ClientDisconnectedEvent += client_Disconnected;
            Console.WriteLine("Port 8085 Socket accepted:\nEndPoint: {0}, client id: {1} ...\n", client.EndPoint, client.Id);
        }

        private static void l_socketAcceptedFileRequest(Socket s)
        {
            Client client = new Client(s, FileRequest);
            client.ClientRecievedEvent += client_Received;
            client.ClientDisconnectedEvent += client_Disconnected;
            Console.WriteLine("Port 8080 Socket accepted:\nEndPoint: {0}, client id: {1} ...\n", client.EndPoint, client.Id);
        }

        private static void client_Disconnected(String id)
        {
            Console.WriteLine("Client {0} has disconnected from server\n", id);
        }

        private static void client_Received(Client sender, string msg)
        {
            Console.WriteLine("Client received {2}...\nmsg: {0}, client id: {1}\n", msg, sender.Id, DateTime.Now);
        }

    }//end of class server

}//end of namespace
