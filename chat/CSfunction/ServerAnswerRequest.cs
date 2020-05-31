﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CSfunction
{
    [Serializable]
    public class ServerAnswerRequest : Message
    {
        public int ClientPort;
        public bool existance;
        public ServerAnswerRequest(IPAddress address, int port, bool exist) : base(address)
        {
            this.ClientPort = port;
            this.existance = exist;
        }
    }
}
