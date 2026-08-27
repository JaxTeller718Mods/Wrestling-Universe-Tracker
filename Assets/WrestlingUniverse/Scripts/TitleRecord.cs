using System;

namespace WrestlingUniverse.UI
{
    [Serializable]
    public sealed class TitleRecord
    {
        public string id;
        public string universeId;
        public string name;
        public string brand;
        public string division;
        public string holderWrestlerId;
        public string holderName;
        public string imagePath;
        public string createdUtc;
    }
}
