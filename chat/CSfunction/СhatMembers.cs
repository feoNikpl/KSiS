using System;
using System.Collections.Generic;
using System.Text;

namespace CSfunction
{
    [Serializable]
    public class СhatMembers
    {
        public string ClientName;
        public int ClientID;
        public СhatMembers(string name, int id)
        {
            this.ClientName = name;
            this.ClientID = id;
        }
    }
}
