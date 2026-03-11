using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using Rebus.Core;

namespace Rebus.UI
{
    public class MainMenuUI : MonoBehaviour
    {
        // Modern palette
        private readonly Color accentCyan = new Color(0f, 0.9f, 1f);
        private readonly Color accentGold = new Color(1f, 0.75f, 0.2f);
        private readonly Color surfaceDark = new Color(0.06f, 0.06f, 0.12f);
        private readonly Color surfaceMid = new Color(0.12f, 0.13f, 0.22f);
        private readonly Color surfaceLight = new Color(0.18f, 0.2f, 0.32f);
        private readonly Color textPrimary = new Color(0.94f, 0.96f, 0.98f);
        private readonly Color textSecondary = new Color(0.55f, 0.58f, 0.65f);

        [SerializeField] private string gameSceneName = "GameScene";

        private Canvas canvas;
        private GameObject howToPlayPanel;

        // Animated background
        private RectTransform[] bgPanels;
        private float[] panelFlipTimers;

        private void Start()
        {
            BuildUI();
            StartCoroutine(AnimateBackground());
        }

        // ------------------------------------------------------------------
        // UI Construction
        // ------------------------------------------------------------------

        private void BuildUI()
        {
            // Canvas
            GameObject canvasObj = new GameObject("MainMenuCanvas");
            canvasObj.transform.SetParent(transform, false);
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Dark background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(canvasObj.transform, false);
            RectTransform bgRect = bg.AddComponent<RectTransform>();
            StretchFull(bgRect);
            Image bgImg = bg.AddComponent<Image>();
            bgImg.color = surfaceDark;

            // Animated background grid
            CreateAnimatedBackground(canvasObj.transform);

            // Purple glow upper
            GameObject glowTop = new GameObject("GlowTop");
            glowTop.transform.SetParent(canvasObj.transform, false);
            RectTransform glowTopRect = glowTop.AddComponent<RectTransform>();
            glowTopRect.anchorMin = new Vector2(0, 0.5f);
            glowTopRect.anchorMax = new Vector2(1, 1);
            glowTopRect.offsetMin = Vector2.zero;
            glowTopRect.offsetMax = Vector2.zero;
            Image glowTopImg = glowTop.AddComponent<Image>();
            glowTopImg.color = new Color(0.18f, 0.05f, 0.30f, 0.25f);

            // Warm glow lower
            GameObject glowBot = new GameObject("GlowBottom");
            glowBot.transform.SetParent(canvasObj.transform, false);
            RectTransform glowBotRect = glowBot.AddComponent<RectTransform>();
            glowBotRect.anchorMin = new Vector2(0, 0);
            glowBotRect.anchorMax = new Vector2(1, 0.4f);
            glowBotRect.offsetMin = Vector2.zero;
            glowBotRect.offsetMax = Vector2.zero;
            Image glowBotImg = glowBot.AddComponent<Image>();
            glowBotImg.color = new Color(0.20f, 0.08f, 0.02f, 0.2f);

            // Teal accent center
            GameObject glowMid = new GameObject("GlowCenter");
            glowMid.transform.SetParent(canvasObj.transform, false);
            RectTransform glowMidRect = glowMid.AddComponent<RectTransform>();
            glowMidRect.anchorMin = new Vector2(0.1f, 0.3f);
            glowMidRect.anchorMax = new Vector2(0.9f, 0.7f);
            glowMidRect.offsetMin = Vector2.zero;
            glowMidRect.offsetMax = Vector2.zero;
            Image glowMidImg = glowMid.AddComponent<Image>();
            glowMidImg.color = new Color(0f, 0.15f, 0.22f, 0.15f);

            // Content container
            GameObject content = new GameObject("Content");
            content.transform.SetParent(canvasObj.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            StretchFull(contentRect);

            VerticalLayoutGroup vLayout = content.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(60, 60, 200, 200);
            vLayout.spacing = 20;
            vLayout.childAlignment = TextAnchor.MiddleCenter;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = false;
            vLayout.childForceExpandWidth = true;
            vLayout.childForceExpandHeight = false;

            // Title with cyan accent
            TextMeshProUGUI title = CreateText("Title", content.transform,
                "CLASSIC\nCONCENTRATION", 80, accentCyan);
            title.fontStyle = FontStyles.Bold;
            LayoutElement titleLE = title.gameObject.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 250;

            // Subtitle
            TextMeshProUGUI subtitle = CreateText("Subtitle", content.transform,
                "The Rebus Puzzle Game", 38, textSecondary);
            subtitle.fontStyle = FontStyles.Italic;
            LayoutElement subLE = subtitle.gameObject.AddComponent<LayoutElement>();
            subLE.preferredHeight = 70;

            // Spacer
            AddSpacer(content.transform, 80);

            // Play button - primary action, accent gold
            Button playBtn = CreateModernMenuButton("PlayBtn", content.transform,
                "PLAY", 56, accentGold, surfaceDark, 140);
            playBtn.onClick.AddListener(OnPlayClicked);

            // 2 Players button - secondary action, accent cyan
            Button twoPlayerBtn = CreateModernMenuButton("TwoPlayerBtn", content.transform,
                "2 PLAYERS", 48, accentCyan, surfaceDark, 120);
            twoPlayerBtn.onClick.AddListener(OnTwoPlayerClicked);

            AddSpacer(content.transform, 20);

            // How to Play button - tertiary, subtle
            Button howToPlayBtn = CreateModernMenuButton("HowToPlayBtn", content.transform,
                "HOW TO PLAY", 38, surfaceLight, textPrimary, 100);
            howToPlayBtn.onClick.AddListener(OnHowToPlayClicked);

            // How to Play panel
            CreateHowToPlayPanel(canvasObj.transform);
        }

        private void CreateAnimatedBackground(Transform parent)
        {
            GameObject bgGrid = new GameObject("BgGrid");
            bgGrid.transform.SetParent(parent, false);
            RectTransform gridRect = bgGrid.AddComponent<RectTransform>();
            StretchFull(gridRect);

            int cols = 5;
            int rows = 8;
            bgPanels = new RectTransform[cols * rows];
            panelFlipTimers = new float[cols * rows];

            for (int r = 0; r < rows; r++)
            {
                for (int c = 0; c < cols; c++)
                {
                    int idx = r * cols + c;

                    GameObject panel = new GameObject($"BgPanel_{r}_{c}");
                    panel.transform.SetParent(bgGrid.transform, false);
                    RectTransform pRect = panel.AddComponent<RectTransform>();

                    pRect.anchorMin = new Vector2((float)c / cols, 1f - (float)(r + 1) / rows);
                    pRect.anchorMax = new Vector2((float)(c + 1) / cols, 1f - (float)r / rows);
                    pRect.offsetMin = new Vector2(2, 2);
                    pRect.offsetMax = new Vector2(-2, -2);

                    Image img = panel.AddComponent<Image>();
                    // Each tile gets a slightly different color hue
                    float hueShift = ((r * cols + c) % 6) / 6f;
                    float rr = surfaceMid.r + Mathf.Sin(hueShift * Mathf.PI * 2f) * 0.06f;
                    float gg = surfaceMid.g + Mathf.Sin((hueShift + 0.33f) * Mathf.PI * 2f) * 0.04f;
                    float bb = surfaceMid.b + Mathf.Sin((hueShift + 0.66f) * Mathf.PI * 2f) * 0.06f;
                    img.color = new Color(
                        Mathf.Clamp01(rr + Random.Range(-0.01f, 0.01f)),
                        Mathf.Clamp01(gg + Random.Range(-0.01f, 0.01f)),
                        Mathf.Clamp01(bb + Random.Range(-0.01f, 0.01f)),
                        0.35f
                    );

                    bgPanels[idx] = pRect;
                    panelFlipTimers[idx] = Random.Range(2f, 10f);
                }
            }
        }

        private void CreateHowToPlayPanel(Transform parent)
        {
            howToPlayPanel = new GameObject("HowToPlayPanel");
            howToPlayPanel.transform.SetParent(parent, false);
            RectTransform overlayRect = howToPlayPanel.AddComponent<RectTransform>();
            StretchFull(overlayRect);
            Image overlayImg = howToPlayPanel.AddComponent<Image>();
            overlayImg.color = new Color(0.02f, 0.02f, 0.06f, 0.9f);

            // Card
            GameObject card = new GameObject("HTPCard");
            card.transform.SetParent(howToPlayPanel.transform, false);
            RectTransform cardRect = card.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.05f, 0.1f);
            cardRect.anchorMax = new Vector2(0.95f, 0.9f);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = surfaceMid;

            Outline cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = new Color(accentCyan.r, accentCyan.g, accentCyan.b, 0.2f);
            cardOutline.effectDistance = new Vector2(1.5f, -1.5f);

            VerticalLayoutGroup vLayout = card.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(50, 50, 50, 50);
            vLayout.spacing = 30;
            vLayout.childAlignment = TextAnchor.UpperCenter;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = false;
            vLayout.childForceExpandWidth = true;
            vLayout.childForceExpandHeight = false;

            TextMeshProUGUI htpTitle = CreateText("HTPTitle", card.transform,
                "HOW TO PLAY", 52, accentCyan);
            htpTitle.fontStyle = FontStyles.Bold;
            LayoutElement titleLE = htpTitle.gameObject.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 90;

            string[] rules = new string[]
            {
                "1. Match pairs of prizes by\n   flipping panels on the board.",
                "2. Each match reveals part of\n   a hidden rebus puzzle.",
                "3. Solve the rebus to win!\n   The fewer attempts, the better.",
            };

            foreach (string rule in rules)
            {
                TextMeshProUGUI ruleText = CreateText("Rule", card.transform,
                    rule, 32, textPrimary);
                ruleText.alignment = TextAlignmentOptions.MidlineLeft;
                LayoutElement ruleLE = ruleText.gameObject.AddComponent<LayoutElement>();
                ruleLE.preferredHeight = 100;
            }

            TextMeshProUGUI exLabel = CreateText("ExampleLabel", card.transform,
                "Example Rebus:", 34, accentGold);
            LayoutElement exLabelLE = exLabel.gameObject.AddComponent<LayoutElement>();
            exLabelLE.preferredHeight = 60;

            // Example box with modern styling
            GameObject exampleBox = new GameObject("ExampleBox");
            exampleBox.transform.SetParent(card.transform, false);
            exampleBox.AddComponent<RectTransform>();
            Image exBg = exampleBox.AddComponent<Image>();
            exBg.color = surfaceDark;

            Outline exOutline = exampleBox.AddComponent<Outline>();
            exOutline.effectColor = new Color(accentGold.r, accentGold.g, accentGold.b, 0.2f);
            exOutline.effectDistance = new Vector2(1, -1);

            LayoutElement exBoxLE = exampleBox.AddComponent<LayoutElement>();
            exBoxLE.preferredHeight = 120;

            TextMeshProUGUI exText = CreateText("ExampleRebus", exampleBox.transform,
                "EZ  +  [image of pie]\n= EASY AS PIE", 30, accentGold);
            RectTransform exTextRect = exText.GetComponent<RectTransform>();
            StretchFull(exTextRect);

            // Flexible spacer
            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(card.transform, false);
            spacer.AddComponent<RectTransform>();
            LayoutElement spacerLE = spacer.AddComponent<LayoutElement>();
            spacerLE.preferredHeight = 20;
            spacerLE.flexibleHeight = 1;

            Button closeBtn = CreateModernMenuButton("CloseHTPBtn", card.transform,
                "GOT IT!", 42, accentCyan, surfaceDark, 110);
            closeBtn.onClick.AddListener(() => howToPlayPanel.SetActive(false));

            howToPlayPanel.SetActive(false);
        }

        // ------------------------------------------------------------------
        // Background Animation
        // ------------------------------------------------------------------

        private IEnumerator AnimateBackground()
        {
            if (bgPanels == null) yield break;

            while (true)
            {
                for (int i = 0; i < bgPanels.Length; i++)
                {
                    panelFlipTimers[i] -= Time.deltaTime;

                    if (panelFlipTimers[i] <= 0f)
                    {
                        panelFlipTimers[i] = Random.Range(3f, 12f);
                        StartCoroutine(FlipPanel(bgPanels[i]));
                    }
                }

                yield return null;
            }
        }

        private IEnumerator FlipPanel(RectTransform panel)
        {
            if (panel == null) yield break;

            Image img = panel.GetComponent<Image>();
            Color originalColor = img.color;
            // Flash with a random vibrant color
            Color[] flashColors = {
                new Color(0f, 0.9f, 1f, 0.15f),    // Cyan
                new Color(1f, 0.4f, 0.7f, 0.15f),   // Pink
                new Color(0.5f, 0.3f, 1f, 0.15f),   // Purple
                new Color(1f, 0.7f, 0.2f, 0.15f),   // Gold
                new Color(0.3f, 1f, 0.5f, 0.15f),   // Green
            };
            Color flashColor = flashColors[Random.Range(0, flashColors.Length)];

            float duration = 0.5f;
            float half = duration * 0.5f;
            float elapsed = 0f;

            while (elapsed < half)
            {
                float t = elapsed / half;
                float scaleX = Mathf.Lerp(1f, 0f, t);
                panel.localScale = new Vector3(scaleX, 1f, 1f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            img.color = flashColor;

            elapsed = 0f;
            while (elapsed < half)
            {
                float t = elapsed / half;
                float scaleX = Mathf.Lerp(0f, 1f, t);
                panel.localScale = new Vector3(scaleX, 1f, 1f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            panel.localScale = Vector3.one;

            elapsed = 0f;
            float fadeDuration = 1.2f;
            while (elapsed < fadeDuration)
            {
                img.color = Color.Lerp(flashColor, originalColor, elapsed / fadeDuration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            img.color = originalColor;
        }

        // ------------------------------------------------------------------
        // Button Handlers
        // ------------------------------------------------------------------

        private void OnPlayClicked()
        {
            GameConfig.SelectedMode = GameMode.SinglePlayer;
            SceneManager.LoadScene(gameSceneName);
        }

        private void OnTwoPlayerClicked()
        {
            GameConfig.SelectedMode = GameMode.TwoPlayer;
            SceneManager.LoadScene(gameSceneName);
        }

        private void OnHowToPlayClicked()
        {
            howToPlayPanel.SetActive(true);
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private TextMeshProUGUI CreateText(string name, Transform parent,
            string text, float fontSize, Color color)
        {
            GameObject obj = new GameObject(name);
            obj.transform.SetParent(parent, false);
            obj.AddComponent<RectTransform>();

            TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

            return tmp;
        }

        private Button CreateModernMenuButton(string name, Transform parent,
            string label, float fontSize, Color bgColor, Color textColor, float height)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            btnObj.AddComponent<RectTransform>();

            Image img = btnObj.AddComponent<Image>();
            img.color = bgColor;

            // Glow outline
            Outline outline = btnObj.AddComponent<Outline>();
            outline.effectColor = new Color(bgColor.r, bgColor.g, bgColor.b, 0.3f);
            outline.effectDistance = new Vector2(2, -2);

            // Drop shadow
            Shadow shadow = btnObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.4f);
            shadow.effectDistance = new Vector2(2, -3);

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;

            ColorBlock cb = btn.colors;
            cb.normalColor = bgColor;
            cb.highlightedColor = Color.Lerp(bgColor, Color.white, 0.15f);
            cb.pressedColor = Color.Lerp(bgColor, Color.black, 0.2f);
            cb.selectedColor = bgColor;
            btn.colors = cb;

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = 100;

            GameObject txtObj = new GameObject("Label");
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform txtRect = txtObj.AddComponent<RectTransform>();
            StretchFull(txtRect);

            TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
            txt.text = label;
            txt.fontSize = fontSize;
            txt.color = textColor;
            txt.alignment = TextAlignmentOptions.Center;
            txt.fontStyle = FontStyles.Bold;

            return btn;
        }

        private void AddSpacer(Transform parent, float height)
        {
            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(parent, false);
            spacer.AddComponent<RectTransform>();
            LayoutElement le = spacer.AddComponent<LayoutElement>();
            le.preferredHeight = height;
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
