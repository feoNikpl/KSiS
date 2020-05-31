using System;
using System.Collections.Generic;
using System.Net;

namespace CSfunction
{
    [Serializable]
    public class HistoryMessageAnswer : Message
    {
        public List<PrivateMessage> History;
        public HistoryMessageAnswer(IPAddress address, List<PrivateMessage> History) : base(address)
        {
            this.History = History;
        }
    }
}
