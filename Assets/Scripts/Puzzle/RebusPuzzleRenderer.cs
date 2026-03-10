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

        private static readonly Color BG_COLOR = new Color(0.04f, 0.04f, 0.10f, 0.95f);
        private static readonly Color BORDER_COLOR = new Color(0f, 0.9f, 1f, 0.3f);

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

            Image bg = container.gameObject.GetComponent<Image>();
            if (bg == null) bg = container.gameObject.AddComponent<Image>();
            bg.color = BG_COLOR;

            // Subtle cyan border glow
            Outline outline = container.gameObject.AddComponent<Outline>();
            outline.effectColor = BORDER_COLOR;
            outline.effectDistance = new Vector2(2, 2);

            // Inner highlight at top
            GameObject innerHL = new GameObject("InnerHighlight");
            innerHL.transform.SetParent(container, false);
            RectTransform hlRect = innerHL.AddComponent<RectTransform>();
            hlRect.anchorMin = new Vector2(0, 0.85f);
            hlRect.anchorMax = new Vector2(1, 1);
            hlRect.offsetMin = Vector2.zero;
            hlRect.offsetMax = Vector2.zero;
            Image hlImg = innerHL.AddComponent<Image>();
            hlImg.color = new Color(0.1f, 0.15f, 0.3f, 0.3f);
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
                cg.alpha = 0f;

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
                    StartCoroutine(FadeInElement(elementGroups[idx], elementTexts[idx]));
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
                    StartCoroutine(FadeInElement(elementGroups[i], elementTexts[i]));
                }
            }
        }

        private IEnumerator FadeInElement(CanvasGroup cg, TextMeshProUGUI text)
        {
            float duration = 0.6f;
            float elapsed = 0f;

            // Scale up from small
            RectTransform rect = cg.GetComponent<RectTransform>();
            Vector3 targetScale = rect.localScale;
            rect.localScale = targetScale * 0.5f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float ease = 1f - (1f - t) * (1f - t); // ease-out quad
                cg.alpha = ease;
                rect.localScale = Vector3.Lerp(targetScale * 0.5f, targetScale, ease);
                elapsed += Time.deltaTime;
                yield return null;
            }
            cg.alpha = 1f;
            rect.localScale = targetScale;
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
