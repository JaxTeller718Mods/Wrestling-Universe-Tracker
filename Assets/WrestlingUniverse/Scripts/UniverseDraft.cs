using System;

namespace WrestlingUniverse.UI
{
    [Serializable]
    public sealed class UniverseDraft
    {
        public string id;
        public string ownerName;
        public string promotionName;
        public string promotionInitials;
        public string startDate;
        public string ownerImagePath;
        public string promotionImagePath;
        public string createdUtc;
    }
}
