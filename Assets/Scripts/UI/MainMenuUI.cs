using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Collections;
using Rebus.Core;

namespace Rebus.UI
{
    /// <summary>
    /// Main menu screen with title, play button, how-to-play overlay,
    /// and a subtle animated background.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        private readonly Color goldColor = new Color(1f, 0.843f, 0f);
        private readonly Color darkBlueColor = new Color(0.102f, 0.137f, 0.494f);
        private readonly Color darkBlueLighter = new Color(0.15f, 0.2f, 0.55f);

        [SerializeField] private string gameSceneName = "GameScene";

        private Canvas canvas;
        private GameObject howToPlayPanel;

        // Animated background panels
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
            bgImg.color = darkBlueColor;

            // Animated background panels (subtle grid)
            CreateAnimatedBackground(canvasObj.transform);

            // Content container
            GameObject content = new GameObject("Content");
            content.transform.SetParent(canvasObj.transform, false);
            RectTransform contentRect = content.AddComponent<RectTransform>();
            StretchFull(contentRect);

            VerticalLayoutGroup vLayout = content.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(60, 60, 200, 200);
            vLayout.spacing = 30;
            vLayout.childAlignment = TextAnchor.MiddleCenter;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = false;
            vLayout.childForceExpandWidth = true;
            vLayout.childForceExpandHeight = false;

            // Title
            TextMeshProUGUI title = CreateText("Title", content.transform,
                "CLASSIC\nCONCENTRATION", 80, goldColor);
            title.fontStyle = FontStyles.Bold;
            LayoutElement titleLE = title.gameObject.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 250;

            // Subtitle
            TextMeshProUGUI subtitle = CreateText("Subtitle", content.transform,
                "The Rebus Puzzle Game", 40, Color.white);
            subtitle.fontStyle = FontStyles.Italic;
            LayoutElement subLE = subtitle.gameObject.AddComponent<LayoutElement>();
            subLE.preferredHeight = 80;

            // Spacer
            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(content.transform, false);
            spacer.AddComponent<RectTransform>();
            LayoutElement spacerLE = spacer.AddComponent<LayoutElement>();
            spacerLE.preferredHeight = 100;

            // Play button (large, gold)
            Button playBtn = CreateMenuButton("PlayBtn", content.transform,
                "PLAY", 56, goldColor, darkBlueColor, 140);
            playBtn.onClick.AddListener(OnPlayClicked);

            // 2 Players button
            Button twoPlayerBtn = CreateMenuButton("TwoPlayerBtn", content.transform,
                "2 PLAYERS", 48, GameConfig.PLAYER1_COLOR, Color.white, 120);
            twoPlayerBtn.onClick.AddListener(OnTwoPlayerClicked);

            // Spacer
            GameObject spacer2 = new GameObject("Spacer2");
            spacer2.transform.SetParent(content.transform, false);
            spacer2.AddComponent<RectTransform>();
            LayoutElement spacer2LE = spacer2.AddComponent<LayoutElement>();
            spacer2LE.preferredHeight = 30;

            // How to Play button
            Button howToPlayBtn = CreateMenuButton("HowToPlayBtn", content.transform,
                "HOW TO PLAY", 40, darkBlueLighter, goldColor, 110);
            howToPlayBtn.onClick.AddListener(OnHowToPlayClicked);

            // How to Play panel (hidden by default)
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

            float panelWidth = 1080f / cols;
            float panelHeight = 1920f / rows;

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
                    // Subtle variation of dark blue
                    float variation = Random.Range(-0.02f, 0.02f);
                    img.color = new Color(
                        darkBlueColor.r + variation,
                        darkBlueColor.g + variation,
                        darkBlueColor.b + variation + 0.03f,
                        0.4f
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
            overlayImg.color = new Color(0, 0, 0, 0.85f);

            // Card
            GameObject card = new GameObject("HTPCard");
            card.transform.SetParent(howToPlayPanel.transform, false);
            RectTransform cardRect = card.AddComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.05f, 0.1f);
            cardRect.anchorMax = new Vector2(0.95f, 0.9f);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = darkBlueColor;

            VerticalLayoutGroup vLayout = card.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(50, 50, 50, 50);
            vLayout.spacing = 30;
            vLayout.childAlignment = TextAnchor.UpperCenter;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = false;
            vLayout.childForceExpandWidth = true;
            vLayout.childForceExpandHeight = false;

            // Title
            TextMeshProUGUI htpTitle = CreateText("HTPTitle", card.transform,
                "HOW TO PLAY", 56, goldColor);
            htpTitle.fontStyle = FontStyles.Bold;
            LayoutElement titleLE = htpTitle.gameObject.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 90;

            // Rules
            string[] rules = new string[]
            {
                "1. Match pairs of prizes by\n   flipping panels on the board.",
                "2. Each match reveals part of\n   a hidden rebus puzzle.",
                "3. Solve the rebus to win!\n   The fewer attempts, the better.",
            };

            foreach (string rule in rules)
            {
                TextMeshProUGUI ruleText = CreateText("Rule", card.transform,
                    rule, 34, Color.white);
                ruleText.alignment = TextAlignmentOptions.MidlineLeft;
                LayoutElement ruleLE = ruleText.gameObject.AddComponent<LayoutElement>();
                ruleLE.preferredHeight = 100;
            }

            // Example section
            TextMeshProUGUI exLabel = CreateText("ExampleLabel", card.transform,
                "Example Rebus:", 36, goldColor);
            LayoutElement exLabelLE = exLabel.gameObject.AddComponent<LayoutElement>();
            exLabelLE.preferredHeight = 60;

            // Example rebus illustration
            GameObject exampleBox = new GameObject("ExampleBox");
            exampleBox.transform.SetParent(card.transform, false);
            exampleBox.AddComponent<RectTransform>();
            Image exBg = exampleBox.AddComponent<Image>();
            exBg.color = new Color(0.05f, 0.05f, 0.15f, 0.8f);
            LayoutElement exBoxLE = exampleBox.AddComponent<LayoutElement>();
            exBoxLE.preferredHeight = 120;

            TextMeshProUGUI exText = CreateText("ExampleRebus", exampleBox.transform,
                "EZ  +  [image of pie]\n= EASY AS PIE", 32, goldColor);
            RectTransform exTextRect = exText.GetComponent<RectTransform>();
            StretchFull(exTextRect);

            // Spacer
            GameObject spacer = new GameObject("Spacer");
            spacer.transform.SetParent(card.transform, false);
            spacer.AddComponent<RectTransform>();
            LayoutElement spacerLE = spacer.AddComponent<LayoutElement>();
            spacerLE.preferredHeight = 20;
            spacerLE.flexibleHeight = 1;

            // Close button
            Button closeBtn = CreateMenuButton("CloseHTPBtn", card.transform,
                "GOT IT!", 42, goldColor, darkBlueColor, 110);
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
            Color flashColor = new Color(goldColor.r, goldColor.g, goldColor.b, 0.15f);

            float duration = 0.5f;
            float half = duration * 0.5f;
            float elapsed = 0f;

            // Scale down on X (simulate flip)
            while (elapsed < half)
            {
                float t = elapsed / half;
                float scaleX = Mathf.Lerp(1f, 0f, t);
                panel.localScale = new Vector3(scaleX, 1f, 1f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            img.color = flashColor;

            // Scale back up
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

            // Fade back
            elapsed = 0f;
            float fadeDuration = 1f;
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

        private Button CreateMenuButton(string name, Transform parent,
            string label, float fontSize, Color bgColor, Color textColor, float height)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            btnObj.AddComponent<RectTransform>();

            Image img = btnObj.AddComponent<Image>();
            img.color = bgColor;

            Button btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = img;

            ColorBlock cb = btn.colors;
            cb.normalColor = bgColor;
            cb.highlightedColor = bgColor * 1.1f;
            cb.pressedColor = bgColor * 0.8f;
            cb.selectedColor = bgColor;
            btn.colors = cb;

            LayoutElement le = btnObj.AddComponent<LayoutElement>();
            le.preferredHeight = height;
            le.minHeight = 100; // Minimum 100px touch target

            // Label
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

        private void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
