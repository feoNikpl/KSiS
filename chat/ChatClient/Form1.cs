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
            client.ReceiveMessageEvent += ShowReceivedMessage;
        }

        public void ShowReceivedMessage(Message message)
        {
            if (message is ServerAnswerRequest)
            {
                AnswerRequestManager(message);
            }
            if (message is ClientIDMessage)
                IDManager(message); 
            if (message is PrivateMessage)
            {
                PrivateMessage mes = (PrivateMessage)message;
                  if (mes.reciverID != 0)
                    RefreshListBox(1);
                  else
                    RefreshListBox(0);
            }
            if (message is HistoryMessageAnswer)
            {
                RefreshListBox(selectindex);
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
                if (i == 0  && selectindex == 0)
                {
                    foreach(PrivateMessage message in client.PrivateMessages)
                    {
                        if (message.reciverID == 0)
                            ChatHistory.Text += "[" + client.GetClientName(message.SenderID) + " " + Convert.ToString(message.DateTime) + "] " + message.data + "\n";
                    }
                }
                if (i > 0 && client.ClientID != selectindex)
                {
                    foreach (PrivateMessage message in client.PrivateMessages)
                    {
                        if ((selectindex == message.SenderID || message.reciverID == selectindex) && message.reciverID != 0)
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
            if (ClientID.Text != "")
            {
                if (client.TCPConnectServer(ClientName.Text,Convert.ToInt32(ClientID.Text)))
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
            send.Enabled = true;
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
            if (client.ClientID != selectindex)
            {
                client.SendMessage(Message.Text, selectindex);
                RefreshListBox(selectindex);
                Message.Text = "";
            }
        }

        private void GetHistory_Click(object sender, EventArgs e)
        {
            client.ClearHistory(selectindex);
            client.SendMessage(new HistoryMessageRequest(client.ClientIP, client.ClientID, selectindex));
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            client.UDPConnectServer(ClientName.Text);
            ClientName.Enabled = false;
            UdpConnect.Enabled = false;
        }

        private void AnswerRequestManager(Message message)
        {
            Action action = delegate
            {
                ServerAnswerRequest mes = (ServerAnswerRequest)message;
                if (mes.existance == true)
                {
                    ClientID.Visible = true;
                    label3.Visible = true;
                    SerwerConnect.Visible = true;
                }
                else
                {
                    client.TCPConnectServer(ClientName.Text, 0);
                    GetHistory.Enabled = true;
                    send.Enabled = true;
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

        private void IDManager(Message message)
        {
            Action action = delegate
            {
                ClientIDMessage mes = (ClientIDMessage)message;
                ClientID.Visible = true;
                label3.Visible = true;
                ClientID.Enabled = false;
                ClientID.Text = mes.id.ToString();
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
    }
}

