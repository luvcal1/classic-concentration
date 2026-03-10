using UnityEngine;
using System.Collections.Generic;

namespace Rebus.Data
{
    public static class PrizeData
    {
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
            new Prize("NEW CAR",           new Color(0.95f, 0.25f, 0.30f)),  // Vibrant Red
            new Prize("TRIP TO HAWAII",    new Color(0.00f, 0.80f, 0.90f)),  // Tropical Cyan
            new Prize("DIAMOND RING",      new Color(0.60f, 0.75f, 0.95f)),  // Soft Blue
            new Prize("BIG SCREEN TV",     new Color(0.25f, 0.27f, 0.35f)),  // Slate Gray
            new Prize("$5,000 CASH",       new Color(0.20f, 0.75f, 0.40f)),  // Money Green
            new Prize("LUXURY CRUISE",     new Color(0.15f, 0.45f, 0.90f)),  // Ocean Blue
            new Prize("DESIGNER WATCH",    new Color(0.90f, 0.72f, 0.25f)),  // Gold
            new Prize("HOME THEATER",      new Color(0.50f, 0.30f, 0.70f)),  // Rich Purple
            new Prize("GIFT CERTIFICATE",  new Color(0.95f, 0.45f, 0.70f)),  // Hot Pink
            new Prize("MOUNTAIN BIKE",     new Color(0.70f, 0.42f, 0.20f)),  // Warm Brown
            new Prize("SPA GETAWAY",       new Color(0.55f, 0.85f, 0.85f)),  // Soft Teal
            new Prize("GOLD NECKLACE",     new Color(1.00f, 0.85f, 0.20f)),  // Bright Gold
            new Prize("GAMING CONSOLE",    new Color(0.15f, 0.15f, 0.20f)),  // Charcoal
            new Prize("SMART PHONE",       new Color(0.55f, 0.60f, 0.68f)),  // Cool Silver
            new Prize("VACATION PACKAGE",  new Color(1.00f, 0.55f, 0.15f)),  // Vibrant Orange
            new Prize("FINE DINING",       new Color(0.65f, 0.15f, 0.20f)),  // Deep Wine
            new Prize("CONCERT TICKETS",   new Color(0.70f, 0.20f, 0.90f)),  // Electric Violet
            new Prize("FITNESS SET",       new Color(0.10f, 0.80f, 0.45f)),  // Neon Green
            new Prize("KITCHEN SET",       new Color(0.92f, 0.88f, 0.78f)),  // Warm Cream
            new Prize("LUGGAGE SET",       new Color(0.45f, 0.32f, 0.25f)),  // Leather
            new Prize("JET SKI",           new Color(0.10f, 0.65f, 0.95f)),  // Sky Blue
            new Prize("POOL TABLE",        new Color(0.10f, 0.55f, 0.30f)),  // Felt Green
            new Prize("GRAND PIANO",       new Color(0.12f, 0.12f, 0.15f)),  // Ebony
            new Prize("SAPPHIRE BROOCH",   new Color(0.15f, 0.40f, 0.85f)),  // Sapphire
            new Prize("CAMPING GEAR",      new Color(0.50f, 0.65f, 0.20f)),  // Olive
        };

        public static List<Prize> GetAllPrizes()
        {
            return new List<Prize>(allPrizes);
        }

        public static List<Prize> GetRandomPrizes(int count)
        {
            List<Prize> pool = new List<Prize>(allPrizes);
            count = Mathf.Clamp(count, 0, pool.Count);

            for (int i = pool.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                Prize temp = pool[i];
                pool[i] = pool[j];
                pool[j] = temp;
            }

            return pool.GetRange(0, count);
        }

        public static int PrizeCount => allPrizes.Count;
    }
}
