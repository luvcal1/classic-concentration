using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Rebus.Puzzle
{
    [System.Serializable]
    public struct RebusPuzzleElement
    {
        public string text;
        public float fontSize;
        public float xOffset;
        public float yOffset;
        public Color color;
        public bool isBold;
        public bool isItalic;
        public float rotation;

        public RebusPuzzleElement(string text, float fontSize, float x, float y,
            Color color, bool bold = true, bool italic = false, float rotation = 0f)
        {
            this.text = text;
            this.fontSize = fontSize;
            this.xOffset = x;
            this.yOffset = y;
            this.color = color;
            this.isBold = bold;
            this.isItalic = italic;
            this.rotation = rotation;
        }
    }

    [System.Serializable]
    public class RebusPuzzle
    {
        public string answer;
        public List<RebusPuzzleElement> elements;

        public RebusPuzzle(string answer, List<RebusPuzzleElement> elements)
        {
            this.answer = answer;
            this.elements = elements;
        }

        public bool CheckAnswer(string guess)
        {
            string cleanGuess = Normalize(guess);
            string cleanAnswer = Normalize(answer);
            return cleanGuess == cleanAnswer;
        }

        private string Normalize(string s)
        {
            s = s.ToLowerInvariant().Trim();
            s = Regex.Replace(s, @"[^a-z0-9\s]", "");
            s = Regex.Replace(s, @"\s+", " ");
            return s;
        }

        /// <summary>
        /// Returns indices of elements that should be visible based on match progress.
        /// Elements are revealed in a shuffled order as pairs are matched.
        /// </summary>
        public List<int> GetVisibleIndices(int pairsMatched, int totalPairs)
        {
            List<int> result = new List<int>();
            if (elements == null || elements.Count == 0) return result;

            // Calculate how many elements to show
            float progress = (float)pairsMatched / totalPairs;
            int elementsToShow = Mathf.CeilToInt(progress * elements.Count);
            elementsToShow = Mathf.Clamp(elementsToShow, 0, elements.Count);

            // Use a seeded shuffle so the reveal order is consistent per puzzle
            System.Random rng = new System.Random(answer.GetHashCode());
            List<int> order = new List<int>();
            for (int i = 0; i < elements.Count; i++) order.Add(i);

            for (int i = order.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                int temp = order[i];
                order[i] = order[j];
                order[j] = temp;
            }

            for (int i = 0; i < elementsToShow; i++)
            {
                result.Add(order[i]);
            }

            return result;
        }
    }
}
