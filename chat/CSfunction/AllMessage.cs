using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CSfunction
{
    [Serializable]
    public class AllMessage:Message
    {
        public DateTime DateTime;
        public int SenderID;
        public string data;
        public AllMessage(IPAddress address, DateTime DateTime, int senderID, string data) : base(address)
        {
            this.DateTime = DateTime;
            this.SenderID = senderID;
            this.data = data;
        }
    }
}
