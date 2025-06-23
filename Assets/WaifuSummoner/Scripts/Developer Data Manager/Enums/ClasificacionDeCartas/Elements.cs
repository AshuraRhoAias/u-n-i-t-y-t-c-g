using UnityEngine;

namespace WaifuSummoner.Cards
{
    public enum ElementType
    {
        Fire,
        Water,
        Earth,
        Air,
        Dark,
        Light,
        Neutral
    }

    public static class ElementTypeDescriptions
    {
        public static string GetDescription(ElementType element)
        {
            return element switch
            {
                ElementType.Fire => "Fire element - Aggressive and powerful",
                ElementType.Water => "Water element - Flowing and adaptable",
                ElementType.Earth => "Earth element - Stable and defensive",
                ElementType.Air => "Air element - Swift and elusive",
                ElementType.Dark => "Dark element - Mysterious and forbidden",
                ElementType.Light => "Light element - Pure and healing",
                ElementType.Neutral => "Neutral element - Balanced",
                _ => "Unknown element"
            };
        }
    }
}