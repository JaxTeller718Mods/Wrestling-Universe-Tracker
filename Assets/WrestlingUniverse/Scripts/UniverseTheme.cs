using UnityEngine;

namespace WrestlingUniverse.UI
{
    [CreateAssetMenu(fileName = "UniverseTheme", menuName = "Wrestling Universe/UI Theme")]
    public sealed class UniverseTheme : ScriptableObject
    {
        public Color background = new Color32(5, 9, 20, 255);
        public Color panel = new Color32(9, 15, 29, 245);
        public Color panelRaised = new Color32(14, 23, 40, 255);
        public Color cyan = new Color32(45, 190, 230, 255);
        public Color gold = new Color32(240, 190, 42, 255);
        public Color primaryText = new Color32(242, 246, 250, 255);
        public Color secondaryText = new Color32(142, 160, 181, 255);
    }
}
