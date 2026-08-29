using System;
using System.Collections.Generic;

namespace WrestlingUniverse.UI
{
    [Serializable]
    public sealed class BookedMatchRecord
    {
        public string id;
        public string universeId;
        public string sourceId;
        public string sourceType;
        public int year;
        public string month;
        public int week;
        public string dayOfWeek;
        public int cardPosition;
        public string stipulation;
        public string format;
        public string titleId;
        public string titleName;
        public string titleImagePath;
        public string stageOneStipulation;
        public string stageTwoStipulation;
        public string stageThreeStipulation;
        public string createdUtc;
        public List<string> participantIds = new List<string>();
        public List<WrestlerRecord> participants = new List<WrestlerRecord>();
    }

    [Serializable]
    public sealed class BookedSegmentRecord
    {
        public string id;
        public string universeId;
        public string sourceId;
        public string sourceType;
        public int year;
        public string month;
        public int week;
        public string dayOfWeek;
        public int cardPosition;
        public string title;
        public string summary;
        public string createdUtc;
        public List<string> participantIds = new List<string>();
        public List<WrestlerRecord> participants = new List<WrestlerRecord>();
    }

    [Serializable]
    public sealed class MatchResultRecord
    {
        public string matchId;
        public string winnerWrestlerId;
        public string finishType;
        public int rating;
        public string duration;
        public string notes;
        public bool titleChanged;
        public bool isDraw;
        public string createdUtc;
    }

    [Serializable]
    public sealed class CompetitionRecord
    {
        public int wins;
        public int losses;
        public int draws;

        public override string ToString()
        {
            return wins + "-" + losses + "-" + draws;
        }
    }
}
