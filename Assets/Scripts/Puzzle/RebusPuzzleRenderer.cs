using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace Rebus.Puzzle
{
    public class RebusPuzzleRenderer : MonoBehaviour
    {
        public static RebusPuzzleRenderer Instance { get; private set; }

        private RectTransform container;
        private RebusPuzzle currentPuzzle;
        private List<TextMeshProUGUI> elementTexts = new List<TextMeshProUGUI>();
        private List<CanvasGroup> elementGroups = new List<CanvasGroup>();
        private HashSet<int> revealedIndices = new HashSet<int>();

        // Cover overlay that hides unrevealed portions
        private Image coverImage;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void Initialize(RectTransform puzzleContainer)
        {
            container = puzzleContainer;

            // Add a background
            Image bg = container.gameObject.GetComponent<Image>();
            if (bg == null) bg = container.gameObject.AddComponent<Image>();
            bg.color = new Color(0.05f, 0.05f, 0.15f, 0.9f);

            // Add border
            Outline outline = container.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 0.84f, 0f, 0.5f);
            outline.effectDistance = new Vector2(3, 3);
        }

        public void LoadPuzzle(RebusPuzzle puzzle)
        {
            ClearPuzzle();
            currentPuzzle = puzzle;

            if (puzzle == null || puzzle.elements == null) return;

            for (int i = 0; i < puzzle.elements.Count; i++)
            {
                var element = puzzle.elements[i];

                GameObject obj = new GameObject($"Element_{i}");
                obj.transform.SetParent(container, false);

                RectTransform rect = obj.AddComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(element.xOffset, element.yOffset);
                rect.sizeDelta = new Vector2(400, 100);

                if (element.rotation != 0f)
                    rect.localRotation = Quaternion.Euler(0, 0, element.rotation);

                TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
                tmp.text = element.text;
                tmp.fontSize = element.fontSize;
                tmp.color = element.color;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.enableWordWrapping = false;
                tmp.overflowMode = TextOverflowModes.Overflow;

                FontStyles style = FontStyles.Normal;
                if (element.isBold) style |= FontStyles.Bold;
                if (element.isItalic) style |= FontStyles.Italic;
                tmp.fontStyle = style;

                CanvasGroup cg = obj.AddComponent<CanvasGroup>();
                cg.alpha = 0f; // Start hidden

                elementTexts.Add(tmp);
                elementGroups.Add(cg);
            }

            revealedIndices.Clear();
        }

        public void UpdateVisibility(int pairsMatched, int totalPairs)
        {
            if (currentPuzzle == null) return;

            List<int> visible = currentPuzzle.GetVisibleIndices(pairsMatched, totalPairs);

            foreach (int idx in visible)
            {
                if (!revealedIndices.Contains(idx) && idx < elementGroups.Count)
                {
                    revealedIndices.Add(idx);
                    StartCoroutine(FadeInElement(elementGroups[idx]));
                }
            }
        }

        public void RevealAll()
        {
            for (int i = 0; i < elementGroups.Count; i++)
            {
                if (!revealedIndices.Contains(i))
                {
                    revealedIndices.Add(i);
                    StartCoroutine(FadeInElement(elementGroups[i]));
                }
            }
        }

        private IEnumerator FadeInElement(CanvasGroup cg)
        {
            float duration = 0.5f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                cg.alpha = Mathf.SmoothStep(0f, 1f, t);
                elapsed += Time.deltaTime;
                yield return null;
            }
            cg.alpha = 1f;
        }

        public string GetCurrentAnswer()
        {
            return currentPuzzle?.answer ?? "";
        }

        public string GetVisiblePuzzleText()
        {
            if (currentPuzzle == null) return "";
            string result = "";
            for (int i = 0; i < currentPuzzle.elements.Count; i++)
            {
                if (revealedIndices.Contains(i))
                    result += currentPuzzle.elements[i].text + " ";
                else
                    result += "??? ";
            }
            return result.Trim();
        }

        private void ClearPuzzle()
        {
            foreach (var tmp in elementTexts)
            {
                if (tmp != null) Destroy(tmp.gameObject);
            }
            elementTexts.Clear();
            elementGroups.Clear();
            revealedIndices.Clear();
            currentPuzzle = null;
        }
    }
}
