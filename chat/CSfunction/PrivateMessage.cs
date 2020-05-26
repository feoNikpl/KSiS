using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CSfunction
{
    [Serializable]
    public class PrivateMessage:AllMessage
    {
        public int reciverID;
        public PrivateMessage(IPAddress address, DateTime DateTime, int senderID, string data, int reciverID) :base(address, DateTime, senderID, data)
        {
            this.reciverID = reciverID;  
        }
    }
}
