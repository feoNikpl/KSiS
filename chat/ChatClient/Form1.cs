using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CSfunction;
using Message = CSfunction.Message;

namespace ChatClient
{
    
    public partial class Form1 : Form
    {
        private List<СhatMembers> chatMembers;
        private Client client;
        private int selectindex = 0;
        public Form1()
        {
            InitializeComponent();
            client = new Client();
            client.UDPConnectServer();
            client.ReceiveMessageEvent += ShowReceivedMessage;
        }

        public void ShowReceivedMessage(Message message)
        {
            if(message is AllMessage)
            {
                if (message is PrivateMessage)
                {
                    PrivateMessage mes = (PrivateMessage)message;
                    if (client.ClientID == mes.reciverID)
                        RefreshListBox(1);
                }
                else
                {
                    RefreshListBox(0);
                }
            }
            if (message is HistoryMessageAnswer)
            {
                RefreshListBox(0);
            }
            if (message is MembersListMessage)
            {
                chatMembers = client.Members;
                RefreshMemners();
            }
        }

        public void RefreshListBox(int i)
        {
            Action action = delegate
            {
                ChatHistory.Text = "";
                if (i == 0)
                {
                    foreach(AllMessage message in client.AllMessages)
                    {
                        ChatHistory.Text += "[" + client.GetClientName(message.SenderID) + " " + Convert.ToString(message.DateTime) + "] " + message.data + "\n";
                    }
                }
                if (i == 1 && client.ClientID != selectindex)
                {
                    foreach (PrivateMessage message in client.PrivateMessages)
                    {
                        if (selectindex == message.SenderID || selectindex == message.reciverID)
                            ChatHistory.Text += "[" + client.GetClientName(message.SenderID) + " " + Convert.ToString(message.DateTime) + "] " + message.data + "\n";
                    }
                }
            };
            if (InvokeRequired)
            {
                Invoke(action);
            }
            else
            {
                action();
            }
        }

        public void RefreshMemners()
        {
            Action action = delegate
            {
                MembersBox.Items.Clear();
                foreach (СhatMembers member in chatMembers)
                {
                    MembersBox.Items.Add(member.ClientName);
                }
            };
            if (InvokeRequired)
            {
                Invoke(action);
            }
            else
            {
                action();
            }

        }

        private void SerwerConnect_Click(object sender, EventArgs e)
        {
            if (ClientName.Text != "")
            {
                if (client.TCPConnectServer(ClientName.Text))
                {
                    SerwerConnect.Enabled = false;
                    GetHistory.Enabled = true;
                    send.Enabled = true;
                }
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (client.TcpSocket != null)
                client.Disconnect();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectindex = MembersBox.SelectedIndex;
            if (selectindex == 0)
                RefreshListBox(0);
            else
                RefreshListBox(1);
            if (selectindex == client.ClientID)
                send.Enabled = false;
        }

        private void send_Click(object sender, EventArgs e)
        {
            if (selectindex >= 0 && client.ClientID != selectindex)
            {
                client.SendMessage(Message.Text, selectindex);
                RefreshListBox(1);
                Message.Text = "";
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            RefreshListBox(0);
        }

        private void GetHistory_Click(object sender, EventArgs e)
        {
            client.SendMessage(new HistoryMessageRequest(client.ClientIP, client.ClientID));
        }
    }
}
