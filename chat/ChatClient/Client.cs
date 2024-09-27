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
        public List<PrivateMessage> PrivateMessages;
        public event ReceiveMessage ReceiveMessageEvent;

        public Client()
        {
            UdpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            TcpSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            serializer = new Serializer();
            ClientIP = GetIP();
            IPEndPoint localip = new IPEndPoint(ClientIP, localPort);
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
        public void UDPConnectServer(string clientName)
        {
            IPEndPoint Broadcast = new IPEndPoint(IPAddress.Broadcast, ServerPort);
            UdpSocket.EnableBroadcast = true;
            UdpSocket.SendTo(serializer.Serialize(MakeServerRequest(clientName)), Broadcast);
            listenUdpThread.Start();
        }

        public bool TCPConnectServer(string clientName, int id)
        {
            try
            {
                TcpSocket.Connect(ServerEndPoint);
                listenTcpThread.Start();
                SendMessage(MakeRegistrationMessage(clientName, id));
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
            if (message is ServerAnswerRequest)
                SerwerAnswerRequest((ServerAnswerRequest)message);
            if (message is PrivateMessage)
                PrivateMessageManager((PrivateMessage)message);
            if (message is HistoryMessageAnswer)
                HistoryMessageManager((HistoryMessageAnswer)message);
            if (message is ClientIDMessage)
                ClientIDMessageManager((ClientIDMessage)message);
            if (message is MembersListMessage)
                MembersListMessageManager((MembersListMessage)message);
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

        public void PrivateMessageManager(PrivateMessage message)
        {
            if (ClientID == message.reciverID || (message.reciverID == 0 && ClientID != message.SenderID))
                PrivateMessages.Add(message);
        }

        public void HistoryMessageManager(HistoryMessageAnswer message)
        {
            foreach(PrivateMessage mes in message.History)
            {
                PrivateMessages.Add(mes);
            }
        }

        public void SerwerAnswerRequest(ServerAnswerRequest message)
        {
            ServerEndPoint = new IPEndPoint(message.SenderAddress, message.ClientPort);
        }

        //Отправка сообщений 
        public void SendMessage(Message message)
        {
            TcpSocket.Send(serializer.Serialize(message));
        }
        //Создание сообщений
        public ServerRequest MakeServerRequest(string clientName)
        {
            IPEndPoint localIp = (IPEndPoint)UdpSocket.LocalEndPoint;
            return new ServerRequest(localIp.Address, localIp.Port, clientName);
        }
        public TCPConnectMessage MakeRegistrationMessage(string clientName, int id)
        {
            return new TCPConnectMessage(ClientIP, clientName, id);
        }
        public void SendMessage(string data, int id)
        {
                PrivateMessage message = new PrivateMessage(ClientIP, DateTime.Now, ClientID, data, id);
                SendMessage(message);
                PrivateMessages.Add(message);
        }

        public void ClearHistory(int selectindex)
        {
            List<PrivateMessage> tmp = new List<PrivateMessage>();
            foreach (PrivateMessage message in PrivateMessages)
                if (message.SenderID == selectindex || message.reciverID == selectindex)
                    tmp.Add(message);
            foreach (PrivateMessage message in tmp)
            {
                PrivateMessages.Remove(message);
            }
            tmp.Clear();
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
