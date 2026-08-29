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

    [Serializable]
    public sealed class TitleReignRecord
    {
        public string id;
        public string titleId;
        public string holderWrestlerId;
        public string holderName;
        public int reignNumber;
        public string wonShowName;
        public int wonYear;
        public string wonMonth;
        public int wonWeek;
        public string wonDayOfWeek;
        public string lostShowName;
        public int? lostYear;
        public string lostMonth;
        public int? lostWeek;
        public string lostDayOfWeek;
    }
}
