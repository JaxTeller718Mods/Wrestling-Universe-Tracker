using System;
using System.Collections.Generic;

namespace WrestlingUniverse.UI
{
    [Serializable]
    public sealed class TeamRecord
    {
        public string id;
        public string universeId;
        public string name;
        public string brand;
        public string disposition;
        public string photoPath;
        public string createdUtc;
        public List<string> memberIds = new List<string>();
        public List<string> memberNames = new List<string>();
    }
}
