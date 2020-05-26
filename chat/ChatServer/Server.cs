using System.Text;
using System;
using System.Threading.Tasks;
using System.IO;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using CSfunction;
using System.Collections.Generic;

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
        private List<AllMessage> AllMessages;
        public Server()
        {
            UdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            TcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serializer = new Serializer();
            ServerIP = GetIP();
            IPEndPoint localipUdp = new IPEndPoint(IPAddress.Any, ServerPort);
            IPEndPoint localipTcp = new IPEndPoint(ServerIP, ServerPort);
            clients = new List<Client>();
            AllMessages = new List<AllMessage>();
            UdpSocket.Bind(localipUdp);
            TcpSocket.Bind(localipTcp);
            listenUdpThread = new Thread(UDPlistener);
            listenTcpThread = new Thread(TCPListener);
            listenUdpThread.Start();
            listenTcpThread.Start();
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
                Client client = new Client(RegMessage.name, GetID(), ConectedSocket, serializer);
                client.messageManager += MessageManager;
                client.ClientDisconnectedEvent += RemoveConnection;
                clients.Add(client);
                Console.WriteLine(RegMessage.name + " join chat");
                SendMessageClient( new ClientIDMessage(RegMessage.SenderAddress,client.id), client);
                SendMessageToAll(new MembersListMessage(ServerIP,GetMembersList()));
            }
        }

        //сортировка сообщений
        public void MessageManager(Message message)
        {
            if (message is ServerRequest)
                SerwerAnswerRequest((ServerRequest)message);
            if (message is AllMessage)
            {
                if (message is PrivateMessage)
                    PrivateMessageManager((PrivateMessage)message);
                else
                    AllMessageManager(message);
            }
            if (message is HistoryMessageRequest)
                HistoryMessageManager((HistoryMessageRequest)message);
        }

        public void SerwerAnswerRequest(ServerRequest message)
        {
            UdpSocket.SendTo(serializer.Serialize(new ServerRequest(ServerIP, ServerPort)), new IPEndPoint(message.SenderAddress, message.ClientPort));
        }

        public void AllMessageManager(Message message)
        {
            AllMessages.Add((AllMessage)message);
            SendMessageToAll(message);
        }

        public void PrivateMessageManager(PrivateMessage message)
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

        public void HistoryMessageManager(HistoryMessageRequest message)
        {
            foreach (Client client in clients)
            {
                if (client.id == message.id)
                {
                    SendMessageClient(new HistoryMessageAnswer(ServerIP,AllMessages), client);
                    break;
                }
            }
        }

        //Создание сообщений
        public List<СhatMembers> GetMembersList()
        {
            List<СhatMembers> members = new List<СhatMembers>();
            members.Add(new СhatMembers("All", 0));
            foreach (Client client in clients)
            {
                members.Add(new СhatMembers(client.name, client.id));
            }
            return members;
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
            SendMessageToAll(new MembersListMessage(ServerIP, GetMembersList()));
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
            return (clients.Count + 1);
        }
        ~Server()
        {
            Close();
        }
    }
}
