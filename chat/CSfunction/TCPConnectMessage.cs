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
        public TCPConnectMessage(IPAddress address, string name) : base(address)
        {
            this.name = name;
        }
    }
}
