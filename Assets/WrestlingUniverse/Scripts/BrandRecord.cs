using System;

namespace WrestlingUniverse.UI
{
    [Serializable]
    public sealed class BrandRecord
    {
        public string id;
        public string universeId;
        public string name;
        public string imagePath;
        public string colorHex;
        public string createdUtc;
    }
}
