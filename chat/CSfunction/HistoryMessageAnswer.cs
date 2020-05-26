using System;
using System.Collections.Generic;
using System.Net;

namespace CSfunction
{
    [Serializable]
    public class HistoryMessageAnswer : Message
    {
        public List<AllMessage> History;
        public HistoryMessageAnswer(IPAddress address, List<AllMessage> History) : base(address)
        {
            this.History = History;
        }
    }
}
