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
        public int DialogIndex;
        public HistoryMessageRequest(IPAddress address, int id, int index) : base(address)
        {
            this.id = id;
            this.DialogIndex = index;
        }
    }
}
