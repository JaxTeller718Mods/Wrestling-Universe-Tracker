namespace WrestlingUniverse.UI
{
    /// <summary>Identifies the scheduled show instance currently open in the booking workspace.</summary>
    public static class ActiveBookingSession
    {
        public static string UniverseId { get; private set; }
        public static string SourceId { get; private set; }
        public static string SourceType { get; private set; }
        public static string ShowName { get; private set; }
        public static int Year { get; private set; }
        public static string Month { get; private set; }
        public static int Week { get; private set; }
        public static string DayOfWeek { get; private set; }

        public static void Begin(string universeId, string sourceId, string sourceType, string showName,
            int year, string month, int week, string dayOfWeek)
        {
            UniverseId = universeId;
            SourceId = sourceId;
            SourceType = sourceType;
            ShowName = showName;
            Year = year;
            Month = month;
            Week = week;
            DayOfWeek = dayOfWeek;
        }

        public static void Clear()
        {
            UniverseId = SourceId = SourceType = ShowName = Month = DayOfWeek = string.Empty;
            Year = Week = 0;
        }
    }
}
