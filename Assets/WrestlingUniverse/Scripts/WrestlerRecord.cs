using System;

namespace WrestlingUniverse.UI
{
    [Serializable]
    public sealed class WrestlerRecord
    {
        public string id;
        public string universeId;
        public string name;
        public string brand;
        public string disposition;
        public string gender;
        public string tier;
        public int overall;
        public string photoPath;
        public string createdUtc;
    }
}
