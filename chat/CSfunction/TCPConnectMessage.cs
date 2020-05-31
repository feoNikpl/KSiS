using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CSfunction
{
    [Serializable]
    public class TCPConnectMessage:Message
    {
        public string name;
        public int id;
        public TCPConnectMessage(IPAddress address, string name, int id) : base(address)
        {
            this.id = id;
            this.name = name;
        }
    }
}
