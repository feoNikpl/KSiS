using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace CSfunction
{
    [Serializable]
    public class MembersListMessage:Message
    {
        public List<СhatMembers> ChatMembersList;
        public MembersListMessage(IPAddress address, List<СhatMembers> сhats) : base(address)
        {
            this.ChatMembersList = сhats;
        }
    }
}
