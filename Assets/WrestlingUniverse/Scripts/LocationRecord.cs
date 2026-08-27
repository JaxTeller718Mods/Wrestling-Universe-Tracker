using System;

namespace WrestlingUniverse.UI
{
    [Serializable]
    public sealed class LocationRecord
    {
        public string id;
        public string universeId;
        public string venueName;
        public string venueLocation;
        public int capacity;
        public string createdUtc;
    }
}
