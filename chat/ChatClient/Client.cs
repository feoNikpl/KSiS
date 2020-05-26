using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using CSfunction;
using System.Collections.Generic;
using System;

namespace ChatClient
{
    public delegate void ReceiveMessage(Message message);
    class Client
    {
        private static int localPort = 0;
        private const int ServerPort = 8001;
        private Serializer serializer;
        public IPAddress ClientIP;
        private IPEndPoint ServerEndPoint; 
        private Socket UdpSocket;
        public Socket TcpSocket;
        private Thread listenUdpThread;
        private Thread listenTcpThread;
        public int ClientID = -1;
        public List<СhatMembers> Members;
        public List<AllMessage> AllMessages;
        public List<PrivateMessage> PrivateMessages;
        public event ReceiveMessage ReceiveMessageEvent;

        public Client()
        {
            UdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            TcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serializer = new Serializer();
            ClientIP = GetIP();
            IPEndPoint localip = new IPEndPoint(ClientIP, localPort);
            AllMessages = new List<AllMessage>();
            Members = new List<СhatMembers>();
            PrivateMessages = new List<PrivateMessage>();
            UdpSocket.Bind(localip);
            TcpSocket.Bind(localip);
            listenUdpThread = new Thread(UDPlistener);
            listenTcpThread = new Thread(TCPListener);
        }
        //Прослушивание сообщений
        public void UDPlistener()
        {
            int byts = 0;
            byte[] dat = new byte[1024];
            EndPoint remoteip = new IPEndPoint(IPAddress.Any, localPort);
            using (MemoryStream memoryStream = new MemoryStream())
            {
                do
                {
                    byts = UdpSocket.ReceiveFrom(dat, ref remoteip);
                    memoryStream.Write(dat, 0, byts);
                }
                while (UdpSocket.Available > 0);
                if (memoryStream.Length > 0)
                {
                    MessageManager(serializer.Deserialize(memoryStream.ToArray()));
                }
            }
        }
        
        public void TCPListener()
        {
            int bytes = 0;
            while (true)
            {
                try
                {
                    byte[] data = new byte[1024];
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        do
                        {
                            bytes = TcpSocket.Receive(data, data.Length, SocketFlags.None);
                            memoryStream.Write(data, 0, bytes);
                        }
                        while (TcpSocket.Available > 0);
                        if (bytes> 0)
                        {
                            MessageManager(serializer.Deserialize(memoryStream.ToArray()));
                        }
                    }
                }
                catch (Exception ex)
                {
                    Disconnect();
                }
  
            }

        }

        //соединение с сервером
        public void UDPConnectServer()
        {
            IPEndPoint Broadcast = new IPEndPoint(IPAddress.Broadcast, ServerPort);
            UdpSocket.EnableBroadcast = true;
            UdpSocket.SendTo(serializer.Serialize(MakeServerRequest()), Broadcast);
            listenUdpThread.Start();
        }

        public bool TCPConnectServer(string clientName)
        {
            try
            {
                TcpSocket.Connect(ServerEndPoint);
                listenTcpThread.Start();
                SendMessage(MakeRegistrationMessage(clientName));
                GeneralFunction.CloseSocket(ref UdpSocket);
                GeneralFunction.CloseThread(ref listenUdpThread);
                return true;

            }
            catch (Exception ex)
            {
                return false;
            }
        }

        //сортировка сообщений
        public void MessageManager(Message message)
        {
            if (message is ServerRequest)
                SerwerAnswerRequest((ServerRequest)message);
            if (message is AllMessage)
                if (message is PrivateMessage)
                    PrivateMessageManager((PrivateMessage)message);
                else
                    AllMessageManager((AllMessage)message);
            if (message is HistoryMessageAnswer)
                HistoryMessageManager((HistoryMessageAnswer)message);
            if (message is ClientIDMessage)
                ClientIDMessageManager((ClientIDMessage)message);
            if (message is MembersListMessage)
                MembersListMessageManager((MembersListMessage)message);
            if (message is AllMessage || message is HistoryMessageAnswer || message is PrivateMessage || message is MembersListMessage)
                ReceiveMessageEvent(message);
        }
        public void MembersListMessageManager(MembersListMessage message) 
        {
            Members = message.ChatMembersList;
        }
        public void ClientIDMessageManager(ClientIDMessage message)
        {
            ClientID = message.id;
        }
        public void AllMessageManager(AllMessage message)
        {
            AllMessages.Add(message);
        }

        public void PrivateMessageManager(PrivateMessage message)
        {
            if (ClientID == message.reciverID)
                PrivateMessages.Add(message);
        }

        public void HistoryMessageManager(HistoryMessageAnswer message)
        {
            AllMessages = message.History;
        }

        public void SerwerAnswerRequest(ServerRequest message)
        {
            ServerEndPoint = new IPEndPoint(message.SenderAddress, message.ClientPort);
        }

        //Отправка сообщений 
        public void SendMessage(Message message)
        {
            TcpSocket.Send(serializer.Serialize(message));
        }
        //Создание сообщений
        public ServerRequest MakeServerRequest()
        {
            IPEndPoint localIp = (IPEndPoint)UdpSocket.LocalEndPoint;
            return new ServerRequest(localIp.Address, localIp.Port);
        }
        public TCPConnectMessage MakeRegistrationMessage(string clientName)
        {
            return new TCPConnectMessage(ClientIP, clientName);
        }
        public void SendMessage(string data, int id)
        {
            if (id == 0)
                SendMessage(new AllMessage(ClientIP, DateTime.Now, ClientID, data));
            if (id > 0)
            {
                PrivateMessage message = new PrivateMessage(ClientIP, DateTime.Now, ClientID, data, id);
                SendMessage(message);
                PrivateMessages.Add(message);
            }
        }


        public IPAddress GetIP()
        {
            string HostName = Dns.GetHostName();
            return Dns.GetHostByName(HostName).AddressList[2];
        }

        public string GetClientName(int id)
        {
            foreach(СhatMembers members in Members)
            {
                if (id == members.ClientID)
                {
                    return members.ClientName;
                } 
            }
            return "";
        }

        public void Disconnect()
        {
            GeneralFunction.CloseThread(ref listenTcpThread);
            GeneralFunction.CloseSocket(ref TcpSocket);
            GeneralFunction.CloseSocket(ref UdpSocket);
            GeneralFunction.CloseThread(ref listenUdpThread);
        }

    }
}
