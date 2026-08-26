namespace WrestlingUniverse.UI
{
    /// <summary>Identifies the universe currently open in this application session.</summary>
    public static class ActiveUniverseSession
    {
        public static string UniverseId { get; private set; }

        public static void Select(string universeId)
        {
            UniverseId = universeId;
        }

        public static void Clear()
        {
            UniverseId = string.Empty;
        }
    }
}
