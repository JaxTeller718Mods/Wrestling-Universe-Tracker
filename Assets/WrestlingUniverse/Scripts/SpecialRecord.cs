using System;
using System.Collections.Generic;

namespace WrestlingUniverse.UI
{
    [Serializable]
    public sealed class SpecialRecord
    {
        public string id;
        public string universeId;
        public string name;
        public string month;
        public string week;
        public string dayOfWeek;
        public string imagePath;
        public string createdUtc;
        public List<string> brandIds = new List<string>();
        public List<string> brandNames = new List<string>();
    }
}
