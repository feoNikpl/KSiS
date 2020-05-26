using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace CSfunction
{
    [Serializable]
    public class ServerRequest : Message
    {
        public int ClientPort;
        public ServerRequest(IPAddress address, int port) : base(address)
        {
            this.ClientPort = port;
        }
    }
}
