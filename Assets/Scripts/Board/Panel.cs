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

        // Colorful panel hue palette - each panel gets a unique tint
        private static readonly Color[] PANEL_HUES = {
            new Color(0.20f, 0.10f, 0.35f), // Deep Purple
            new Color(0.10f, 0.15f, 0.38f), // Navy
            new Color(0.08f, 0.22f, 0.35f), // Dark Teal
            new Color(0.12f, 0.28f, 0.20f), // Forest
            new Color(0.30f, 0.12f, 0.18f), // Burgundy
            new Color(0.32f, 0.18f, 0.08f), // Deep Amber
        };

        private Color panelHue;
        private Color panelGlow;

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
            // Each panel gets a unique hue based on its number
            int hueIdx = (PanelNumber - 1) % PANEL_HUES.Length;
            panelHue = PANEL_HUES[hueIdx];

            // Derive a brighter glow color from the hue
            panelGlow = new Color(
                Mathf.Clamp01(panelHue.r * 3f + 0.2f),
                Mathf.Clamp01(panelHue.g * 3f + 0.2f),
                Mathf.Clamp01(panelHue.b * 3f + 0.2f),
                0.5f
            );

            backgroundImage.color = Color.clear;

            // Front face - uniquely tinted card
            frontFace = new GameObject("FrontFace");
            frontFace.transform.SetParent(transform, false);
            RectTransform frontRect = frontFace.AddComponent<RectTransform>();
            StretchFull(frontRect);

            Image frontBg = frontFace.AddComponent<Image>();
            frontBg.color = panelHue;

            // Top highlight strip
            GameObject highlight = new GameObject("Highlight");
            highlight.transform.SetParent(frontFace.transform, false);
            RectTransform hlRect = highlight.AddComponent<RectTransform>();
            hlRect.anchorMin = new Vector2(0, 0.55f);
            hlRect.anchorMax = new Vector2(1, 1);
            hlRect.offsetMin = Vector2.zero;
            hlRect.offsetMax = Vector2.zero;
            Image hlImg = highlight.AddComponent<Image>();
            hlImg.color = new Color(1f, 1f, 1f, 0.1f);

            // Colored border glow matching panel hue
            Outline frontOutline = frontFace.AddComponent<Outline>();
            frontOutline.effectColor = panelGlow;
            frontOutline.effectDistance = new Vector2(1.5f, -1.5f);

            // Drop shadow
            Shadow frontShadow = frontFace.AddComponent<Shadow>();
            frontShadow.effectColor = new Color(0, 0, 0, 0.5f);
            frontShadow.effectDistance = new Vector2(3, -3);

            // Number text - tinted to match panel
            GameObject numObj = new GameObject("Number");
            numObj.transform.SetParent(frontFace.transform, false);
            RectTransform numRect = numObj.AddComponent<RectTransform>();
            StretchFull(numRect);

            Color numColor = Color.Lerp(panelGlow, Color.white, 0.5f);
            numColor.a = 1f;

            TextMeshProUGUI numText = numObj.AddComponent<TextMeshProUGUI>();
            numText.text = PanelNumber.ToString();
            numText.fontSize = 44;
            numText.color = numColor;
            numText.alignment = TextAlignmentOptions.Center;
            numText.fontStyle = FontStyles.Bold;

            // Back face - prize card
            backFace = new GameObject("BackFace");
            backFace.transform.SetParent(transform, false);
            RectTransform backRect = backFace.AddComponent<RectTransform>();
            StretchFull(backRect);

            Image backBg = backFace.AddComponent<Image>();
            // Slightly brighten and saturate prize colors for modern look
            Color brightPrize = Color.Lerp(PrizeColor, Color.white, 0.1f);
            brightPrize = new Color(
                Mathf.Clamp01(brightPrize.r * 1.1f),
                Mathf.Clamp01(brightPrize.g * 1.1f),
                Mathf.Clamp01(brightPrize.b * 1.1f)
            );
            backBg.color = brightPrize;

            // Back card top highlight
            GameObject backHL = new GameObject("BackHighlight");
            backHL.transform.SetParent(backFace.transform, false);
            RectTransform bhlRect = backHL.AddComponent<RectTransform>();
            bhlRect.anchorMin = new Vector2(0, 0.65f);
            bhlRect.anchorMax = new Vector2(1, 1);
            bhlRect.offsetMin = Vector2.zero;
            bhlRect.offsetMax = Vector2.zero;
            Image bhlImg = backHL.AddComponent<Image>();
            bhlImg.color = new Color(1, 1, 1, 0.15f);

            // Back shadow + outline
            Outline backOutline = backFace.AddComponent<Outline>();
            backOutline.effectColor = new Color(1, 1, 1, 0.3f);
            backOutline.effectDistance = new Vector2(1f, -1f);

            Shadow backShadow = backFace.AddComponent<Shadow>();
            backShadow.effectColor = new Color(0, 0, 0, 0.5f);
            backShadow.effectDistance = new Vector2(3, -3);

            // Prize name text
            GameObject prizeObj = new GameObject("PrizeName");
            prizeObj.transform.SetParent(backFace.transform, false);
            RectTransform prizeRect = prizeObj.AddComponent<RectTransform>();
            StretchFull(prizeRect);
            prizeRect.offsetMin = new Vector2(4, 4);
            prizeRect.offsetMax = new Vector2(-4, -4);

            TextMeshProUGUI prizeText = prizeObj.AddComponent<TextMeshProUGUI>();
            prizeText.text = PrizeName;
            prizeText.fontSize = 20;
            prizeText.color = Color.white;
            prizeText.alignment = TextAlignmentOptions.Center;
            prizeText.fontStyle = FontStyles.Bold;
            prizeText.enableWordWrapping = true;
            prizeText.outlineWidth = 0.25f;
            prizeText.outlineColor = new Color(0, 0, 0, 0.6f);

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

            // First half - scale X to 0 with slight Y punch
            while (elapsed < half)
            {
                float t = elapsed / half;
                float scaleX = Mathf.Lerp(1f, 0f, t);
                float scaleY = 1f + Mathf.Sin(t * Mathf.PI) * 0.05f; // subtle stretch
                rectTransform.localScale = new Vector3(scaleX, scaleY, 1f);
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
                float scaleY = 1f + Mathf.Sin((1f - t) * Mathf.PI) * 0.05f;
                rectTransform.localScale = new Vector3(scaleX, scaleY, 1f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            rectTransform.localScale = Vector3.one;
            IsAnimating = false;
            onComplete?.Invoke();
        }

        /// <summary>
        /// Staggered entrance animation - panels pop in from invisible.
        /// </summary>
        public void PlayEntranceAnimation(float delay)
        {
            rectTransform.localScale = Vector3.zero;
            StartCoroutine(EntranceAnimation(delay));
        }

        private IEnumerator EntranceAnimation(float delay)
        {
            yield return new WaitForSeconds(delay);

            float duration = 0.3f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                // Overshoot ease-out for bouncy feel
                float ease = 1f + 0.15f * Mathf.Sin(t * Mathf.PI);
                float scale = Mathf.Lerp(0f, 1f, t) * ease;
                rectTransform.localScale = new Vector3(scale, scale, 1f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            rectTransform.localScale = Vector3.one;
        }

        public void MatchAndRemove(Action onComplete = null)
        {
            IsMatched = true;
            StartCoroutine(MatchAnimation(onComplete));
        }

        private IEnumerator MatchAnimation(Action onComplete)
        {
            IsAnimating = true;
            CanvasGroup cg = gameObject.AddComponent<CanvasGroup>();

            // Pick a random fly-away direction
            float angle = UnityEngine.Random.Range(0f, 360f) * Mathf.Deg2Rad;
            Vector2 flyDirection = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            float flyDistance = 1200f;

            // Flap parameters
            float duration = 0.8f;
            float flapSpeed = UnityEngine.Random.Range(12f, 20f);
            float flapAmplitude = 30f;
            float spinDir = UnityEngine.Random.Range(0, 2) == 0 ? 1f : -1f;

            Vector3 startPos = rectTransform.anchoredPosition3D;
            float startRotZ = 0f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;

                float moveFactor = t * t;
                Vector2 offset = flyDirection * flyDistance * moveFactor;
                rectTransform.anchoredPosition = (Vector2)startPos + offset;

                float flapPhase = Mathf.Sin(elapsed * flapSpeed * Mathf.PI * 2f);
                float scaleX = Mathf.Lerp(1f, 0.15f, Mathf.Abs(flapPhase));
                float scaleY = Mathf.Lerp(1f, 0.85f, (1f - scaleX) * 0.3f);
                rectTransform.localScale = new Vector3(scaleX, scaleY, 1f);

                float rotZ = startRotZ + spinDir * flapAmplitude * Mathf.Sin(elapsed * flapSpeed * Mathf.PI);
                rotZ += spinDir * t * 90f;
                rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotZ);

                cg.alpha = t < 0.5f ? 1f : Mathf.Lerp(1f, 0f, (t - 0.5f) * 2f);

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
