using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CSfunction
{
    [Serializable]
    public class HistoryMessageRequest:Message
    {
        public int id;
        public HistoryMessageRequest(IPAddress address, int id) : base(address)
        {
            this.id = id;
        }
    }
}
