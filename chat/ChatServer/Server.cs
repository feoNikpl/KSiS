using System.Text;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using CSfunction;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;

namespace ChatServer
{
    class Server
    {
        private const int ServerPort = 8001;
        private Serializer serializer;
        private IPAddress ServerIP;
        private Socket UdpSocket;
        private Socket TcpSocket;
        private Thread listenUdpThread;
        private Thread listenTcpThread;
        private List<Client> clients;
        private SqlConnection connection;
        private const string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=E:\УНИВЕР\КСиС\chat\ChatServer\Database1.mdf;Integrated Security=True";
        private List<СhatMembers> members;
        public Server()
        {
            UdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            TcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serializer = new Serializer();
            ServerIP = GetIP();
            IPEndPoint localipUdp = new IPEndPoint(IPAddress.Any, ServerPort);
            IPEndPoint localipTcp = new IPEndPoint(ServerIP, ServerPort);
            clients = new List<Client>();
            UdpSocket.Bind(localipUdp);
            TcpSocket.Bind(localipTcp);
            listenUdpThread = new Thread(UDPlistener);
            listenTcpThread = new Thread(TCPListener);
            listenUdpThread.Start();
            listenTcpThread.Start();
            connection = new SqlConnection(connectionString);
            connection.Open();
            GetListMembers();
        }
        //Прослушивание
        public void UDPlistener()
        {
            int bytes = 0;
            byte[] data = new byte[1024];
            EndPoint remoteip = new IPEndPoint(IPAddress.Any, 0);
            while (true)
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    do
                    {
                        bytes = UdpSocket.ReceiveFrom(data, ref remoteip);
                        memoryStream.Write(data, 0, bytes);
                    }
                    while (UdpSocket.Available > 0);
                    if (memoryStream.Length > 0)
                    {
                        MessageManager(serializer.Deserialize(memoryStream.ToArray()));
                    }
                }
            }
        }

        public void TCPListener()
        {
            int bytes = 0;
            TcpSocket.Listen(10);
            while (true)
            {
                Socket connectedSocket = TcpSocket.Accept();
                byte[] data = new byte[1024];
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    do
                    {
                        bytes = connectedSocket.Receive(data, data.Length, SocketFlags.None);
                        memoryStream.Write(data, 0, bytes);
                    }
                    while (TcpSocket.Available > 0);
                    if (memoryStream.Length > 0)
                    {
                        NewClientAppear(serializer.Deserialize(memoryStream.ToArray()), connectedSocket);
                    }
                }
            }
        }

        //добавление новых клиентов
        public void NewClientAppear(Message message, Socket ConectedSocket)
        {
            if (message is TCPConnectMessage)
            {
                TCPConnectMessage RegMessage = (TCPConnectMessage)message;
                Client client;
                if (RegMessage.id == 0)
                {
                    client = new Client(RegMessage.name, GetID(), ConectedSocket, serializer);
                    members.Add(new СhatMembers(RegMessage.name, client.id));
                    FillinDBClients(RegMessage.name, client.id);
                }
                else
                {
                    if (CheckNameID(RegMessage.name, RegMessage.id))
                    {
                        client = new Client(RegMessage.name, RegMessage.id, ConectedSocket, serializer);
                    }
                    else
                    {
                        GeneralFunction.CloseSocket(ref ConectedSocket);
                        return;
                    }
                }
                client.messageManager += MessageManager;
                client.ClientDisconnectedEvent += RemoveConnection;
                clients.Add(client);
                Console.WriteLine(RegMessage.name + " join chat");
                SendMessageClient( new ClientIDMessage(RegMessage.SenderAddress,client.id), client);
                SendMessageToAll(new MembersListMessage(ServerIP,members));
            }
        }

        //сортировка сообщений
        public void MessageManager(Message message)
        {
            if (message is ServerRequest)
                SerwerAnswerRequest((ServerRequest)message);

            if (message is PrivateMessage)
                PrivateMessageManager((PrivateMessage)message);

            if (message is HistoryMessageRequest)
                HistoryMessageManager((HistoryMessageRequest)message);
        }

        public void SerwerAnswerRequest(ServerRequest message)
        {
            UdpSocket.SendTo(serializer.Serialize(new ServerAnswerRequest(ServerIP, ServerPort, FindNameDB(message.ClientName))), new IPEndPoint(message.SenderAddress, message.ClientPort));
        }

        public void PrivateMessageManager(PrivateMessage message)
        {
            InsertintoDB(message);
            if (message.reciverID == 0)
            {
                SendMessageToAll(message);
            }
            else
            {
                foreach (Client client in clients)
                {
                    if (client.id == message.reciverID)
                    {
                        SendMessageClient(message, client);
                        break;
                    }
                }
            }
        }

        public void HistoryMessageManager(HistoryMessageRequest message)
        {
            foreach (Client client in clients)
            {
                if (client.id == message.id)
                {
                    SendMessageClient(new HistoryMessageAnswer(ServerIP,MakeHistoryList(message)), client);
                    break;
                }
            }
        }

        //Пересылка сообщений
        public void SendMessageToAll(Message message)
        {
            foreach( Client client in clients)
            {
                SendMessageClient(message, client);
            }
        }

        public void SendMessageClient(Message message, Client client)
        {
            client.tcpSocket.Send(serializer.Serialize(message));
        }

        public IPAddress GetIP()
        {
            string HostName = Dns.GetHostName();
            return Dns.GetHostByName(HostName).AddressList[2];
        }
        //
        public void RemoveConnection(Client disconnectedClient)
        {
            if (clients.Remove(disconnectedClient))
                Console.WriteLine(disconnectedClient.name + " left from the chat!");
            SendMessageToAll(new MembersListMessage(ServerIP, members));
        }
        public void Close()
        {
            foreach (Client client in clients)
            {
                GeneralFunction.CloseSocket(ref client.tcpSocket);
                GeneralFunction.CloseThread(ref client.listenTcpThread);
            }
            GeneralFunction.CloseSocket(ref TcpSocket);
            GeneralFunction.CloseSocket(ref UdpSocket);
            GeneralFunction.CloseThread(ref listenTcpThread);
            GeneralFunction.CloseThread(ref listenUdpThread);
        }
        public int GetID()
        {
            return (members.Count);
        }
        public void GetListMembers()
        {
            members = new List<СhatMembers>();
            members.Add(new СhatMembers("All", 0));
            string sql = "SELECT * FROM Clients";
            SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            foreach(DataRow dr in ds.Tables[0].Rows)
            {
                members.Add(new СhatMembers(dr.ItemArray[1].ToString(),Convert.ToInt32(dr.ItemArray[0])));
            }
        }

        public void FillinDBClients(string ClientName, int id)
        {
            string sql = "INSERT INTO Clients (Id,Name) Values(" + id +",'"+ ClientName + "')";
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.InsertCommand = new SqlCommand(sql, connection);
            adapter.InsertCommand.ExecuteNonQuery();

        }
        public void InsertintoDB(PrivateMessage message)
        {
            string sql = "INSERT INTO Messages (ReciverId,SenderId,Data,Message) Values(" + message.reciverID+ "," + message.SenderID + ", '" + message.DateTime.ToString() + "','" + message.data + "')";
            SqlDataAdapter adapter = new SqlDataAdapter();
            adapter.InsertCommand = new SqlCommand(sql, connection);
            adapter.InsertCommand.ExecuteNonQuery();
        }

        public List<PrivateMessage> MakeHistoryList(HistoryMessageRequest message)
        {
            List<PrivateMessage> HistoryList = new List<PrivateMessage>();
            string sql = "SELECT * FROM Messages";
            SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            if (ds.Tables[0].Rows.Count != 0)
            {
                if (message.DialogIndex == 0)
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        if (row.ItemArray[0].ToString() == "0")
                        {
                            HistoryList.Add(new PrivateMessage(ServerIP, Convert.ToDateTime(row.ItemArray[2]), Convert.ToInt32(row.ItemArray[1]), row.ItemArray[3].ToString(), Convert.ToInt32(row.ItemArray[0])));
                        }
                    }
                }
                else
                {
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        if ((row.ItemArray[0].ToString() == message.id.ToString() && row.ItemArray[1].ToString() == message.DialogIndex.ToString()) || (row.ItemArray[1].ToString() == message.id.ToString() && row.ItemArray[0].ToString() == message.DialogIndex.ToString()))
                        {
                            HistoryList.Add(new PrivateMessage(ServerIP, Convert.ToDateTime(row.ItemArray[2]), Convert.ToInt32(row.ItemArray[1]), row.ItemArray[3].ToString(), Convert.ToInt32(row.ItemArray[0])));
                        }
                    }
                }
            }
            return HistoryList;
        }
        public bool FindNameDB(string ClientName)
        {
            string sql = "SELECT * FROM Clients WHERE Name='"+ClientName+"'";
            SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            if (ds.Tables[0].Rows.Count == 0)
                return false;
            else
                return true;
        }

        public bool CheckNameID(string ClientName,int id)
        {
            string sql = "SELECT * FROM Clients WHERE Name='" + ClientName + "'";
            SqlDataAdapter adapter = new SqlDataAdapter(sql, connection);
            DataSet ds = new DataSet();
            adapter.Fill(ds);
            if (ds.Tables[0].Rows[0].ItemArray[0].ToString() != id.ToString())
                return false;
            else
                return true;
        }
        ~Server()
        {
            Close();
            connection.Close();
        }
    }
}
