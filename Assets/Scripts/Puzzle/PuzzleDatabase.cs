using UnityEngine;
using System.Collections.Generic;

namespace Rebus.Puzzle
{
    public static class PuzzleDatabase
    {
        private static readonly Color gold = new Color(1f, 0.84f, 0f);
        private static readonly Color white = Color.white;
        private static readonly Color cyan = Color.cyan;
        private static readonly Color yellow = Color.yellow;
        private static readonly Color lime = new Color(0.5f, 1f, 0.3f);

        private static List<RebusPuzzle> allPuzzles;

        public static RebusPuzzle GetRandomPuzzle()
        {
            if (allPuzzles == null) BuildPuzzles();
            return allPuzzles[Random.Range(0, allPuzzles.Count)];
        }

        public static int PuzzleCount
        {
            get
            {
                if (allPuzzles == null) BuildPuzzles();
                return allPuzzles.Count;
            }
        }

        private static void BuildPuzzles()
        {
            allPuzzles = new List<RebusPuzzle>
            {
                // 1. HEAD over HEELS
                new RebusPuzzle("Head over heels", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("HEAD", 60, 0, 80, gold),
                    new RebusPuzzleElement("over", 36, 0, 20, white, false, true),
                    new RebusPuzzleElement("HEELS", 60, 0, -60, gold),
                }),

                // 2. READING between the LINES
                new RebusPuzzle("Reading between the lines", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("________________", 30, 0, 80, white),
                    new RebusPuzzleElement("READING", 52, 0, 0, gold),
                    new RebusPuzzleElement("________________", 30, 0, -80, white),
                }),

                // 3. ONCE upon a TIME
                new RebusPuzzle("Once upon a time", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("ONCE", 60, 0, 80, gold),
                    new RebusPuzzleElement("upon a", 30, 0, 20, white, false, true),
                    new RebusPuzzleElement("TIME", 60, 0, -60, gold),
                }),

                // 4. RIGHT under your NOSE
                new RebusPuzzle("Right under your nose", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("NOSE", 60, 0, 60, gold),
                    new RebusPuzzleElement("RIGHT", 50, 0, -30, cyan),
                    new RebusPuzzleElement("(under)", 24, 0, 10, white, false, true),
                }),

                // 5. BIG fish in a small pond
                new RebusPuzzle("Big fish in a small pond", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("BIG", 80, -120, 30, gold),
                    new RebusPuzzleElement("fish", 28, -120, -40, white, false, true),
                    new RebusPuzzleElement("small pond", 22, 120, 0, cyan),
                    new RebusPuzzleElement("[         ]", 30, 120, 0, white),
                }),

                // 6. BROKEN promise
                new RebusPuzzle("Broken promise", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("PRO", 64, -100, 0, gold),
                    new RebusPuzzleElement("MISE", 64, 120, 0, gold),
                    new RebusPuzzleElement("//", 48, 10, 0, new Color(1, 0.3f, 0.3f)),
                }),

                // 7. ROUND of applause
                new RebusPuzzle("Round of applause", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("A P P", 40, 0, 100, gold),
                    new RebusPuzzleElement("L", 40, -150, 40, gold),
                    new RebusPuzzleElement("A", 40, 150, 40, gold),
                    new RebusPuzzleElement("U", 40, -150, -40, gold),
                    new RebusPuzzleElement("S", 40, 150, -40, gold),
                    new RebusPuzzleElement("E", 40, 0, -100, gold),
                }),

                // 8. LONG time no see
                new RebusPuzzle("Long time no see", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("T   I   M   E", 56, 0, 40, gold),
                    new RebusPuzzleElement("no C", 48, 0, -50, cyan),
                }),

                // 9. FORGIVE and FORGET
                new RebusPuzzle("Forgive and forget", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("4GIVE", 60, 0, 60, gold),
                    new RebusPuzzleElement("&", 48, 0, 0, white),
                    new RebusPuzzleElement("4GET", 60, 0, -60, gold),
                }),

                // 10. PARADISE (pair of dice)
                new RebusPuzzle("Paradise", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("PAIR", 56, -100, 20, gold),
                    new RebusPuzzleElement("of", 30, 0, 20, white, false, true),
                    new RebusPuzzleElement("DICE", 56, 100, 20, gold),
                    new RebusPuzzleElement("= ?", 40, 0, -50, cyan),
                }),

                // 11. CROSSROADS
                new RebusPuzzle("Crossroads", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("R O A D S", 48, 0, 0, gold),
                    new RebusPuzzleElement("R", 48, 0, 120, cyan),
                    new RebusPuzzleElement("O", 48, 0, 60, cyan),
                    new RebusPuzzleElement("A", 48, 0, -60, cyan),
                    new RebusPuzzleElement("D", 48, 0, -120, cyan),
                    new RebusPuzzleElement("S", 48, 0, -170, cyan),
                }),

                // 12. DOWNTOWN
                new RebusPuzzle("Downtown", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("TOWN", 72, 0, -120, gold),
                }),

                // 13. UNDERCOVER
                new RebusPuzzle("Undercover", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("COVER", 64, 0, 30, gold),
                    new RebusPuzzleElement("____________", 36, 0, -20, white),
                }),

                // 14. BACKWARDS
                new RebusPuzzle("Backwards", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("S D R A W K C A B", 52, 0, 0, gold),
                }),

                // 15. SPLIT SECOND
                new RebusPuzzle("Split second", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("SEC", 64, -100, 0, gold),
                    new RebusPuzzleElement("|", 80, 0, 0, new Color(1, 0.3f, 0.3f)),
                    new RebusPuzzleElement("OND", 64, 100, 0, gold),
                }),

                // 16. TRICYCLE
                new RebusPuzzle("Tricycle", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("3", 80, -80, 0, cyan),
                    new RebusPuzzleElement("CYCLE", 60, 60, 0, gold),
                }),

                // 17. FOR once in my life
                new RebusPuzzle("For once in my life", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("4", 72, -160, 0, cyan),
                    new RebusPuzzleElement("ONCE", 50, -40, 0, gold),
                    new RebusPuzzleElement("IN MY", 36, 80, 20, white),
                    new RebusPuzzleElement("LIFE", 50, 170, 0, gold),
                }),

                // 18. JUST between you and me
                new RebusPuzzle("Just between you and me", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("JUST", 48, 0, 80, gold),
                    new RebusPuzzleElement("YOU", 52, -120, -30, cyan),
                    new RebusPuzzleElement("ME", 52, 120, -30, cyan),
                }),

                // 19. RISING costs
                new RebusPuzzle("Rising costs", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("C", 44, -120, -60, gold),
                    new RebusPuzzleElement("O", 44, -60, -20, gold),
                    new RebusPuzzleElement("S", 44, 0, 20, gold),
                    new RebusPuzzleElement("T", 44, 60, 60, gold),
                    new RebusPuzzleElement("S", 44, 120, 100, gold),
                }),

                // 20. BANANA split
                new RebusPuzzle("Banana split", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("BA", 60, -180, 0, gold),
                    new RebusPuzzleElement("NA", 60, -20, 0, gold),
                    new RebusPuzzleElement("NA", 60, 140, 0, gold),
                }),

                // 21. SANDBOX (sand in a box)
                new RebusPuzzle("Sandbox", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("[          ]", 48, 0, 0, white),
                    new RebusPuzzleElement("SAND", 44, 0, 0, gold),
                }),

                // 22. DOUBLE CROSS
                new RebusPuzzle("Double cross", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("CROSS", 56, 0, 50, gold),
                    new RebusPuzzleElement("CROSS", 56, 0, -50, gold),
                }),

                // 23. MIND over MATTER
                new RebusPuzzle("Mind over matter", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("MIND", 60, 0, 80, gold),
                    new RebusPuzzleElement("over", 32, 0, 20, white, false, true),
                    new RebusPuzzleElement("MATTER", 60, 0, -60, gold),
                }),

                // 24. LITTLE LEAGUE
                new RebusPuzzle("Little league", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("league", 28, 0, 0, gold),
                }),

                // 25. NEON (knee + on)
                new RebusPuzzle("Neon", new List<RebusPuzzleElement>
                {
                    new RebusPuzzleElement("KNEE", 56, -80, 0, cyan),
                    new RebusPuzzleElement("ON", 56, 80, 0, gold),
                }),
            };
        }
    }
}
