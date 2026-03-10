using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Collections;

namespace Rebus.Board
{
    public class Panel : MonoBehaviour, IPointerClickHandler
    {
        public int PanelNumber { get; private set; }
        public string PrizeName { get; private set; }
        public Color PrizeColor { get; private set; }
        public int Row { get; private set; }
        public int Col { get; private set; }
        public bool IsFlipped { get; private set; }
        public bool IsMatched { get; private set; }
        public bool IsAnimating { get; private set; }

        public event Action<Panel> OnClicked;

        private GameObject frontFace;
        private GameObject backFace;
        private Image backgroundImage;
        private RectTransform rectTransform;

        private static readonly Color FRONT_COLOR = new Color(0.1f, 0.14f, 0.49f); // Dark blue
        private static readonly Color FRONT_HOVER = new Color(0.15f, 0.2f, 0.55f);

        public void Initialize(int number, int row, int col, string prizeName, Color prizeColor)
        {
            PanelNumber = number;
            Row = row;
            Col = col;
            PrizeName = prizeName;
            PrizeColor = prizeColor;

            rectTransform = GetComponent<RectTransform>();
            backgroundImage = GetComponent<Image>();

            BuildVisuals();
        }

        private void BuildVisuals()
        {
            backgroundImage.color = FRONT_COLOR;

            // Front face - shows number
            frontFace = new GameObject("FrontFace");
            frontFace.transform.SetParent(transform, false);
            RectTransform frontRect = frontFace.AddComponent<RectTransform>();
            StretchFull(frontRect);

            Image frontBg = frontFace.AddComponent<Image>();
            frontBg.color = FRONT_COLOR;

            GameObject numObj = new GameObject("Number");
            numObj.transform.SetParent(frontFace.transform, false);
            RectTransform numRect = numObj.AddComponent<RectTransform>();
            StretchFull(numRect);

            TextMeshProUGUI numText = numObj.AddComponent<TextMeshProUGUI>();
            numText.text = PanelNumber.ToString();
            numText.fontSize = 48;
            numText.color = Color.white;
            numText.alignment = TextAlignmentOptions.Center;
            numText.fontStyle = FontStyles.Bold;

            // Back face - shows prize
            backFace = new GameObject("BackFace");
            backFace.transform.SetParent(transform, false);
            RectTransform backRect = backFace.AddComponent<RectTransform>();
            StretchFull(backRect);

            Image backBg = backFace.AddComponent<Image>();
            backBg.color = PrizeColor;

            GameObject prizeObj = new GameObject("PrizeName");
            prizeObj.transform.SetParent(backFace.transform, false);
            RectTransform prizeRect = prizeObj.AddComponent<RectTransform>();
            StretchFull(prizeRect);
            prizeRect.offsetMin = new Vector2(5, 5);
            prizeRect.offsetMax = new Vector2(-5, -5);

            TextMeshProUGUI prizeText = prizeObj.AddComponent<TextMeshProUGUI>();
            prizeText.text = PrizeName;
            prizeText.fontSize = 22;
            prizeText.color = Color.white;
            prizeText.alignment = TextAlignmentOptions.Center;
            prizeText.fontStyle = FontStyles.Bold;
            prizeText.enableWordWrapping = true;

            // Add outline for readability
            prizeText.outlineWidth = 0.2f;
            prizeText.outlineColor = Color.black;

            backFace.SetActive(false);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (IsAnimating || IsFlipped || IsMatched) return;
            OnClicked?.Invoke(this);
        }

        public void FlipToBack(Action onComplete = null)
        {
            if (IsFlipped || IsAnimating) return;
            StartCoroutine(FlipAnimation(true, onComplete));
        }

        public void FlipToFront(Action onComplete = null)
        {
            if (!IsFlipped || IsAnimating) return;
            StartCoroutine(FlipAnimation(false, onComplete));
        }

        private IEnumerator FlipAnimation(bool toBack, Action onComplete)
        {
            IsAnimating = true;
            float duration = Rebus.Core.GameConfig.FLIP_ANIMATION_TIME;
            float half = duration * 0.5f;
            float elapsed = 0f;

            // First half - scale X to 0
            while (elapsed < half)
            {
                float t = elapsed / half;
                float scaleX = Mathf.Lerp(1f, 0f, t);
                rectTransform.localScale = new Vector3(scaleX, 1f, 1f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            // Swap faces
            frontFace.SetActive(!toBack);
            backFace.SetActive(toBack);
            IsFlipped = toBack;

            // Second half - scale X back to 1
            elapsed = 0f;
            while (elapsed < half)
            {
                float t = elapsed / half;
                float scaleX = Mathf.Lerp(0f, 1f, t);
                rectTransform.localScale = new Vector3(scaleX, 1f, 1f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            rectTransform.localScale = Vector3.one;
            IsAnimating = false;
            onComplete?.Invoke();
        }

        public void MatchAndRemove(Action onComplete = null)
        {
            IsMatched = true;
            StartCoroutine(MatchAnimation(onComplete));
        }

        private IEnumerator MatchAnimation(Action onComplete)
        {
            IsAnimating = true;
            float duration = Rebus.Core.GameConfig.MATCH_ANIMATION_TIME;
            float elapsed = 0f;

            Vector3 startScale = Vector3.one;
            CanvasGroup cg = gameObject.AddComponent<CanvasGroup>();

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float scale = Mathf.Lerp(1f, 0.3f, t);
                float alpha = Mathf.Lerp(1f, 0f, t);
                rectTransform.localScale = new Vector3(scale, scale, 1f);
                cg.alpha = alpha;
                elapsed += Time.deltaTime;
                yield return null;
            }

            IsAnimating = false;
            gameObject.SetActive(false);
            onComplete?.Invoke();
        }

        private void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
