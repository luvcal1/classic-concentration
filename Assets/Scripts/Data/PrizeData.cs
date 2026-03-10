using UnityEngine;
using System.Collections.Generic;

namespace Rebus.Data
{
    /// <summary>
    /// Static repository of prize definitions themed after the Classic Concentration TV show.
    /// Each prize has a display name and a panel-back color.
    /// </summary>
    public static class PrizeData
    {
        /// <summary>
        /// Represents a single prize with a name and display color.
        /// </summary>
        public struct Prize
        {
            public string Name;
            public Color Color;

            public Prize(string name, Color color)
            {
                Name = name;
                Color = color;
            }
        }

        private static readonly List<Prize> allPrizes = new List<Prize>
        {
            new Prize("NEW CAR",           new Color(0.90f, 0.16f, 0.16f)),  // Red
            new Prize("TRIP TO HAWAII",    new Color(0.00f, 0.75f, 0.85f)),  // Tropical Blue
            new Prize("DIAMOND RING",      new Color(0.72f, 0.84f, 0.92f)),  // Ice Blue
            new Prize("BIG SCREEN TV",     new Color(0.20f, 0.20f, 0.20f)),  // Dark Gray
            new Prize("$5,000 CASH",       new Color(0.13f, 0.55f, 0.13f)),  // Money Green
            new Prize("LUXURY CRUISE",     new Color(0.00f, 0.40f, 0.80f)),  // Ocean Blue
            new Prize("DESIGNER WATCH",    new Color(0.83f, 0.69f, 0.22f)),  // Gold
            new Prize("HOME THEATER",      new Color(0.30f, 0.15f, 0.40f)),  // Purple
            new Prize("GIFT CERTIFICATE",  new Color(0.93f, 0.51f, 0.93f)),  // Pink
            new Prize("MOUNTAIN BIKE",     new Color(0.55f, 0.27f, 0.07f)),  // Brown
            new Prize("SPA GETAWAY",       new Color(0.68f, 0.85f, 0.90f)),  // Light Cyan
            new Prize("GOLD NECKLACE",     new Color(1.00f, 0.84f, 0.00f)),  // Gold
            new Prize("GAMING CONSOLE",    new Color(0.00f, 0.00f, 0.00f)),  // Black
            new Prize("SMART PHONE",       new Color(0.50f, 0.50f, 0.50f)),  // Silver
            new Prize("VACATION PACKAGE",  new Color(1.00f, 0.55f, 0.00f)),  // Orange
            new Prize("FINE DINING",       new Color(0.50f, 0.00f, 0.00f)),  // Burgundy
            new Prize("CONCERT TICKETS",   new Color(0.58f, 0.00f, 0.83f)),  // Violet
            new Prize("FITNESS SET",       new Color(0.00f, 0.65f, 0.31f)),  // Sport Green
            new Prize("KITCHEN SET",       new Color(0.96f, 0.96f, 0.86f)),  // Cream
            new Prize("LUGGAGE SET",       new Color(0.36f, 0.25f, 0.20f)),  // Leather Brown
            new Prize("JET SKI",           new Color(0.00f, 0.60f, 0.90f)),  // Water Blue
            new Prize("POOL TABLE",        new Color(0.00f, 0.39f, 0.15f)),  // Felt Green
            new Prize("GRAND PIANO",       new Color(0.10f, 0.10f, 0.10f)),  // Ebony
            new Prize("SAPPHIRE BROOCH",   new Color(0.06f, 0.32f, 0.73f)),  // Sapphire Blue
            new Prize("CAMPING GEAR",      new Color(0.42f, 0.56f, 0.14f)),  // Olive Green
        };

        /// <summary>
        /// Returns the full list of all available prizes.
        /// </summary>
        public static List<Prize> GetAllPrizes()
        {
            return new List<Prize>(allPrizes);
        }

        /// <summary>
        /// Returns a shuffled random subset of prizes.
        /// </summary>
        /// <param name="count">Number of prizes to return. Clamped to available count.</param>
        public static List<Prize> GetRandomPrizes(int count)
        {
            List<Prize> pool = new List<Prize>(allPrizes);
            count = Mathf.Clamp(count, 0, pool.Count);

            // Fisher-Yates shuffle
            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                Prize temp = pool[i];
                pool[i] = pool[j];
                pool[j] = temp;
            }

            return pool.GetRange(0, count);
        }

        /// <summary>
        /// Returns the total number of available prizes.
        /// </summary>
        public static int PrizeCount => allPrizes.Count;
    }
}
