using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Rebus.Core;

namespace Rebus.UI
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        // Modern color palette
        private readonly Color accentCyan = new Color(0f, 0.9f, 1f);              // Vibrant cyan
        private readonly Color accentGold = new Color(1f, 0.75f, 0.2f);           // Warm amber
        private readonly Color surfaceDark = new Color(0.08f, 0.08f, 0.16f);      // Card background
        private readonly Color surfaceMid = new Color(0.12f, 0.13f, 0.22f);       // Elevated surface
        private readonly Color surfaceLight = new Color(0.18f, 0.2f, 0.32f);      // Lighter surface
        private readonly Color textPrimary = new Color(0.94f, 0.96f, 0.98f);      // Near white
        private readonly Color textSecondary = new Color(0.55f, 0.58f, 0.65f);    // Muted
        private readonly Color dangerRed = new Color(1f, 0.35f, 0.35f);
        private readonly Color successGreen = new Color(0.3f, 0.85f, 0.5f);
        private readonly Color overlayColor = new Color(0.02f, 0.02f, 0.06f, 0.85f);

        // --- Top Bar ---
        private TextMeshProUGUI scoreText;
        private TextMeshProUGUI attemptsText;
        private TextMeshProUGUI timerText;

        // --- Solve Puzzle Button ---
        private Button solvePuzzleButton;

        // --- Solve Input Panel ---
        private GameObject solvePanel;
        private TMP_InputField solveInputField;
        private Button submitButton;
        private Button cancelButton;

        // --- Game Over Panel ---
        private GameObject gameOverPanel;
        private TextMeshProUGUI gameOverScoreText;
        private TextMeshProUGUI gameOverTimeText;
        private Button playAgainButton;
        private Button mainMenuButton;

        // --- Victory Panel ---
        private GameObject victoryPanel;
        private TextMeshProUGUI victoryTitleText;
        private TextMeshProUGUI victoryAnswerText;
        private TextMeshProUGUI victoryStatsText;
        private Button victoryPlayAgainButton;
        private Button victoryMainMenuButton;
        private ParticleSystem confettiParticles;

        // --- Turn Indicator (2-player) ---
        private GameObject turnBanner;
        private TextMeshProUGUI turnText;
        private Image turnBannerBg;

        // --- Game Mode ---
        private GameMode currentMode;

        // --- Canvas ---
        private Canvas mainCanvas;

        // --- Events ---
        public event Action OnSolvePuzzleClicked;
        public event Action<string> OnSolveSubmitted;
        public event Action OnSolveCancelled;
        public event Action OnPlayAgainClicked;
        public event Action OnMainMenuClicked;

        // --- Timer ---
        private float elapsedTime;
        private bool timerRunning;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            CreateCanvas();
            CreateTopBar();
            CreateSolvePuzzleButton();
            CreateSolvePanel();
            CreateGameOverPanel();
            CreateVictoryPanel();
            CreateTurnBanner();

            solvePanel.SetActive(false);
            gameOverPanel.SetActive(false);
            victoryPanel.SetActive(false);
            turnBanner.SetActive(false);
        }

        private void Update()
        {
            if (timerRunning)
            {
                elapsedTime += Time.deltaTime;
                UpdateTimerDisplay();
            }
        }

        // ------------------------------------------------------------------
        // Canvas
        // ------------------------------------------------------------------

        private void CreateCanvas()
        {
            GameObject canvasObj = new GameObject("UICanvas");
            canvasObj.transform.SetParent(transform);
            mainCanvas = canvasObj.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            mainCanvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();
        }

        // ------------------------------------------------------------------
        // Top Bar - frosted glass style
        // ------------------------------------------------------------------

        private void CreateTopBar()
        {
            GameObject topBar = CreatePanel("TopBar", mainCanvas.transform);
            RectTransform topRect = topBar.GetComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0, 1);
            topRect.anchorMax = new Vector2(1, 1);
            topRect.pivot = new Vector2(0.5f, 1);
            topRect.sizeDelta = new Vector2(0, 120);
            topRect.anchoredPosition = Vector2.zero;

            Image topBg = topBar.GetComponent<Image>();
            topBg.color = new Color(surfaceDark.r, surfaceDark.g, surfaceDark.b, 0.92f);

            // Bottom accent line
            GameObject accentLine = new GameObject("AccentLine");
            accentLine.transform.SetParent(topBar.transform, false);
            RectTransform lineRect = accentLine.AddComponent<RectTransform>();
            lineRect.anchorMin = new Vector2(0, 0);
            lineRect.anchorMax = new Vector2(1, 0);
            lineRect.pivot = new Vector2(0.5f, 0);
            lineRect.sizeDelta = new Vector2(0, 2);
            lineRect.anchoredPosition = Vector2.zero;
            Image lineImg = accentLine.AddComponent<Image>();
            lineImg.color = new Color(accentCyan.r, accentCyan.g, accentCyan.b, 0.4f);

            HorizontalLayoutGroup layout = topBar.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(30, 30, 10, 14);
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            scoreText = CreateTextElement("ScoreText", topBar.transform, "Matches: 0/15", 34, accentCyan);
            attemptsText = CreateTextElement("AttemptsText", topBar.transform, "Attempts: 0", 34, textPrimary);
            timerText = CreateTextElement("TimerText", topBar.transform, "0:00", 34, accentGold);
        }

        // ------------------------------------------------------------------
        // Solve Puzzle Button - gradient accent style
        // ------------------------------------------------------------------

        private void CreateSolvePuzzleButton()
        {
            GameObject btnObj = CreateModernButton("SolvePuzzleButton", mainCanvas.transform,
                "SOLVE PUZZLE", 44, accentCyan, surfaceDark);

            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.12f, 0);
            btnRect.anchorMax = new Vector2(0.88f, 0);
            btnRect.pivot = new Vector2(0.5f, 0);
            btnRect.sizeDelta = new Vector2(0, 110);
            btnRect.anchoredPosition = new Vector2(0, 45);

            solvePuzzleButton = btnObj.GetComponent<Button>();
            solvePuzzleButton.onClick.AddListener(() => OnSolvePuzzleClicked?.Invoke());
        }

        // ------------------------------------------------------------------
        // Solve Input Panel
        // ------------------------------------------------------------------

        private void CreateSolvePanel()
        {
            solvePanel = CreatePanel("SolvePanel", mainCanvas.transform);
            RectTransform overlayRect = solvePanel.GetComponent<RectTransform>();
            SetFullStretch(overlayRect);
            solvePanel.GetComponent<Image>().color = overlayColor;

            // Card
            GameObject card = CreatePanel("SolveCard", solvePanel.transform);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.05f, 0.25f);
            cardRect.anchorMax = new Vector2(0.95f, 0.75f);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;
            card.GetComponent<Image>().color = surfaceMid;

            // Card border
            Outline cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = new Color(accentCyan.r, accentCyan.g, accentCyan.b, 0.2f);
            cardOutline.effectDistance = new Vector2(1.5f, -1.5f);

            VerticalLayoutGroup vLayout = card.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(40, 40, 40, 40);
            vLayout.spacing = 30;
            vLayout.childAlignment = TextAnchor.MiddleCenter;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = false;
            vLayout.childForceExpandWidth = true;
            vLayout.childForceExpandHeight = false;

            TextMeshProUGUI solveTitle = CreateTextElement("SolveTitle", card.transform,
                "SOLVE THE PUZZLE", 48, accentCyan);
            LayoutElement titleLE = solveTitle.gameObject.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 80;

            // Input field with dark bg
            GameObject inputObj = new GameObject("SolveInput");
            inputObj.transform.SetParent(card.transform, false);
            Image inputBg = inputObj.AddComponent<Image>();
            inputBg.color = surfaceDark;

            Outline inputOutline = inputObj.AddComponent<Outline>();
            inputOutline.effectColor = new Color(accentCyan.r, accentCyan.g, accentCyan.b, 0.3f);
            inputOutline.effectDistance = new Vector2(1, -1);

            solveInputField = inputObj.AddComponent<TMP_InputField>();

            RectTransform inputRect = inputObj.GetComponent<RectTransform>();
            inputRect.sizeDelta = new Vector2(0, 100);

            GameObject textArea = new GameObject("TextArea");
            textArea.transform.SetParent(inputObj.transform, false);
            RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
            SetFullStretch(textAreaRect);
            textAreaRect.offsetMin = new Vector2(20, 5);
            textAreaRect.offsetMax = new Vector2(-20, -5);
            textArea.AddComponent<RectMask2D>();

            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(textArea.transform, false);
            TextMeshProUGUI placeholder = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholder.text = "Type your answer...";
            placeholder.fontSize = 36;
            placeholder.color = textSecondary;
            placeholder.alignment = TextAlignmentOptions.MidlineLeft;
            RectTransform phRect = placeholderObj.GetComponent<RectTransform>();
            SetFullStretch(phRect);

            GameObject inputTextObj = new GameObject("Text");
            inputTextObj.transform.SetParent(textArea.transform, false);
            TextMeshProUGUI inputText = inputTextObj.AddComponent<TextMeshProUGUI>();
            inputText.fontSize = 36;
            inputText.color = textPrimary;
            inputText.alignment = TextAlignmentOptions.MidlineLeft;
            RectTransform itRect = inputTextObj.GetComponent<RectTransform>();
            SetFullStretch(itRect);

            solveInputField.textViewport = textAreaRect;
            solveInputField.textComponent = inputText;
            solveInputField.placeholder = placeholder;
            solveInputField.characterLimit = 100;
            solveInputField.contentType = TMP_InputField.ContentType.Standard;

            // Custom caret color
            solveInputField.caretColor = accentCyan;
            solveInputField.selectionColor = new Color(accentCyan.r, accentCyan.g, accentCyan.b, 0.3f);

            LayoutElement inputLE = inputObj.AddComponent<LayoutElement>();
            inputLE.preferredHeight = 100;

            // Button row
            GameObject buttonRow = new GameObject("ButtonRow");
            buttonRow.transform.SetParent(card.transform, false);
            RectTransform rowRect = buttonRow.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0, 110);
            HorizontalLayoutGroup hLayout = buttonRow.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 20;
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.childControlWidth = true;
            hLayout.childControlHeight = true;
            hLayout.childForceExpandWidth = true;
            hLayout.childForceExpandHeight = true;

            LayoutElement rowLE = buttonRow.AddComponent<LayoutElement>();
            rowLE.preferredHeight = 110;

            GameObject cancelObj = CreateModernButton("CancelBtn", buttonRow.transform,
                "CANCEL", 36, dangerRed, textPrimary);
            cancelButton = cancelObj.GetComponent<Button>();
            cancelButton.onClick.AddListener(() =>
            {
                OnSolveCancelled?.Invoke();
                HideSolvePanel();
            });

            GameObject submitObj = CreateModernButton("SubmitBtn", buttonRow.transform,
                "SUBMIT", 36, successGreen, surfaceDark);
            submitButton = submitObj.GetComponent<Button>();
            submitButton.onClick.AddListener(() =>
            {
                string guess = solveInputField.text.Trim();
                if (!string.IsNullOrEmpty(guess))
                    OnSolveSubmitted?.Invoke(guess);
            });
        }

        // ------------------------------------------------------------------
        // Game Over Panel
        // ------------------------------------------------------------------

        private void CreateGameOverPanel()
        {
            gameOverPanel = CreatePanel("GameOverPanel", mainCanvas.transform);
            RectTransform overlayRect = gameOverPanel.GetComponent<RectTransform>();
            SetFullStretch(overlayRect);
            gameOverPanel.GetComponent<Image>().color = overlayColor;

            GameObject card = CreatePanel("GameOverCard", gameOverPanel.transform);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.1f, 0.25f);
            cardRect.anchorMax = new Vector2(0.9f, 0.75f);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;
            card.GetComponent<Image>().color = surfaceMid;

            Outline cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = new Color(dangerRed.r, dangerRed.g, dangerRed.b, 0.3f);
            cardOutline.effectDistance = new Vector2(1.5f, -1.5f);

            VerticalLayoutGroup vLayout = card.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(40, 40, 50, 50);
            vLayout.spacing = 30;
            vLayout.childAlignment = TextAnchor.MiddleCenter;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = false;
            vLayout.childForceExpandWidth = true;
            vLayout.childForceExpandHeight = false;

            TextMeshProUGUI title = CreateTextElement("GameOverTitle", card.transform,
                "GAME OVER", 64, dangerRed);
            LayoutElement titleLE = title.gameObject.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 100;

            gameOverScoreText = CreateTextElement("GameOverScore", card.transform,
                "Score: 0", 42, textPrimary);
            LayoutElement scoreLE = gameOverScoreText.gameObject.AddComponent<LayoutElement>();
            scoreLE.preferredHeight = 60;

            gameOverTimeText = CreateTextElement("GameOverTime", card.transform,
                "Time: 0:00", 42, textSecondary);
            LayoutElement timeLE = gameOverTimeText.gameObject.AddComponent<LayoutElement>();
            timeLE.preferredHeight = 60;

            GameObject playAgainObj = CreateModernButton("PlayAgainBtn", card.transform,
                "PLAY AGAIN", 42, accentCyan, surfaceDark);
            playAgainButton = playAgainObj.GetComponent<Button>();
            playAgainButton.onClick.AddListener(() => OnPlayAgainClicked?.Invoke());
            LayoutElement paLE = playAgainObj.AddComponent<LayoutElement>();
            paLE.preferredHeight = 110;

            GameObject menuObj = CreateModernButton("MainMenuBtn", card.transform,
                "MAIN MENU", 42, surfaceLight, textPrimary);
            mainMenuButton = menuObj.GetComponent<Button>();
            mainMenuButton.onClick.AddListener(() => OnMainMenuClicked?.Invoke());
            LayoutElement mmLE = menuObj.AddComponent<LayoutElement>();
            mmLE.preferredHeight = 110;
        }

        // ------------------------------------------------------------------
        // Victory Panel
        // ------------------------------------------------------------------

        private void CreateVictoryPanel()
        {
            victoryPanel = CreatePanel("VictoryPanel", mainCanvas.transform);
            RectTransform overlayRect = victoryPanel.GetComponent<RectTransform>();
            SetFullStretch(overlayRect);
            victoryPanel.GetComponent<Image>().color = overlayColor;

            GameObject card = CreatePanel("VictoryCard", victoryPanel.transform);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.05f, 0.15f);
            cardRect.anchorMax = new Vector2(0.95f, 0.85f);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;
            card.GetComponent<Image>().color = surfaceMid;

            Outline cardOutline = card.AddComponent<Outline>();
            cardOutline.effectColor = new Color(1f, 0.5f, 0.8f, 0.35f); // Pink-gold glow
            cardOutline.effectDistance = new Vector2(2f, -2f);

            VerticalLayoutGroup vLayout = card.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(40, 40, 50, 50);
            vLayout.spacing = 25;
            vLayout.childAlignment = TextAnchor.MiddleCenter;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = false;
            vLayout.childForceExpandWidth = true;
            vLayout.childForceExpandHeight = false;

            CreateConfettiEffect(victoryPanel.transform);

            victoryTitleText = CreateTextElement("VictoryTitle", card.transform,
                "CONGRATULATIONS!", 60, accentGold);
            LayoutElement titleLE = victoryTitleText.gameObject.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 100;

            TextMeshProUGUI answerLabel = CreateTextElement("AnswerLabel", card.transform,
                "The answer was:", 34, textSecondary);
            LayoutElement alLE = answerLabel.gameObject.AddComponent<LayoutElement>();
            alLE.preferredHeight = 50;

            victoryAnswerText = CreateTextElement("VictoryAnswer", card.transform,
                "", 52, accentCyan);
            LayoutElement vaLE = victoryAnswerText.gameObject.AddComponent<LayoutElement>();
            vaLE.preferredHeight = 80;

            victoryStatsText = CreateTextElement("VictoryStats", card.transform,
                "", 34, textPrimary);
            LayoutElement vsLE = victoryStatsText.gameObject.AddComponent<LayoutElement>();
            vsLE.preferredHeight = 120;

            GameObject playAgainObj = CreateModernButton("VictoryPlayAgainBtn", card.transform,
                "PLAY AGAIN", 42, accentCyan, surfaceDark);
            victoryPlayAgainButton = playAgainObj.GetComponent<Button>();
            victoryPlayAgainButton.onClick.AddListener(() => OnPlayAgainClicked?.Invoke());
            LayoutElement paLE = playAgainObj.AddComponent<LayoutElement>();
            paLE.preferredHeight = 110;

            GameObject menuObj = CreateModernButton("VictoryMainMenuBtn", card.transform,
                "MAIN MENU", 42, surfaceLight, textPrimary);
            victoryMainMenuButton = menuObj.GetComponent<Button>();
            victoryMainMenuButton.onClick.AddListener(() => OnMainMenuClicked?.Invoke());
            LayoutElement mmLE = menuObj.AddComponent<LayoutElement>();
            mmLE.preferredHeight = 110;
        }

        private void CreateConfettiEffect(Transform parent)
        {
            GameObject confettiObj = new GameObject("ConfettiEffect");
            confettiObj.transform.SetParent(parent, false);
            RectTransform confettiRect = confettiObj.AddComponent<RectTransform>();
            confettiRect.anchorMin = new Vector2(0.5f, 1f);
            confettiRect.anchorMax = new Vector2(0.5f, 1f);
            confettiRect.anchoredPosition = Vector2.zero;

            confettiParticles = confettiObj.AddComponent<ParticleSystem>();

            var main = confettiParticles.main;
            main.duration = 3f;
            main.loop = true;
            main.startLifetime = 3f;
            main.startSpeed = new ParticleSystem.MinMaxCurve(200f, 400f);
            main.startSize = new ParticleSystem.MinMaxCurve(10f, 25f);
            Gradient startGradient = new Gradient();
            startGradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(accentCyan, 0f),
                    new GradientColorKey(new Color(1f, 0.4f, 0.7f), 0.25f),  // Pink
                    new GradientColorKey(accentGold, 0.5f),
                    new GradientColorKey(new Color(0.5f, 0.3f, 1f), 0.75f),  // Purple
                    new GradientColorKey(successGreen, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                }
            );
            main.startColor = new ParticleSystem.MinMaxGradient(startGradient);
            main.gravityModifier = 0.5f;
            main.maxParticles = 250;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            var emission = confettiParticles.emission;
            emission.rateOverTime = 50f;

            var shape = confettiParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(800f, 1f, 1f);

            var colorOverLifetime = confettiParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] {
                    new GradientColorKey(accentCyan, 0f),
                    new GradientColorKey(new Color(1f, 0.4f, 0.7f), 0.25f),
                    new GradientColorKey(accentGold, 0.5f),
                    new GradientColorKey(new Color(0.5f, 0.3f, 1f), 0.75f),
                    new GradientColorKey(successGreen, 1f)
                },
                new GradientAlphaKey[] {
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(gradient);

            var renderer = confettiObj.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));

            confettiParticles.Stop();
        }

        // ------------------------------------------------------------------
        // Turn Banner (2-player)
        // ------------------------------------------------------------------

        private void CreateTurnBanner()
        {
            turnBanner = CreatePanel("TurnBanner", mainCanvas.transform);
            RectTransform bannerRect = turnBanner.GetComponent<RectTransform>();
            bannerRect.anchorMin = new Vector2(0, 1);
            bannerRect.anchorMax = new Vector2(1, 1);
            bannerRect.pivot = new Vector2(0.5f, 1);
            bannerRect.sizeDelta = new Vector2(0, 60);
            bannerRect.anchoredPosition = new Vector2(0, -120);

            turnBannerBg = turnBanner.GetComponent<Image>();
            turnBannerBg.color = GameConfig.PLAYER1_COLOR;

            // Subtle shadow under banner
            Shadow bannerShadow = turnBanner.AddComponent<Shadow>();
            bannerShadow.effectColor = new Color(0, 0, 0, 0.4f);
            bannerShadow.effectDistance = new Vector2(0, -3);

            turnText = CreateTextElement("TurnText", turnBanner.transform,
                "PLAYER 1'S TURN", 30, Color.white);
            RectTransform turnTextRect = turnText.GetComponent<RectTransform>();
            SetFullStretch(turnTextRect);
        }

        // ------------------------------------------------------------------
        // Public API
        // ------------------------------------------------------------------

        public void SetGameMode(GameMode mode)
        {
            currentMode = mode;
            turnBanner.SetActive(mode == GameMode.TwoPlayer);

            if (mode == GameMode.TwoPlayer)
            {
                if (scoreText != null)
                    scoreText.text = "P1: 0  P2: 0";
                if (attemptsText != null)
                    attemptsText.text = "Attempts: 0";
            }
        }

        public void UpdateTurn(int player)
        {
            if (turnText == null || turnBannerBg == null) return;

            Color playerColor = player == 0 ? GameConfig.PLAYER1_COLOR : GameConfig.PLAYER2_COLOR;
            turnBannerBg.color = playerColor;
            turnText.text = $"PLAYER {player + 1}'S TURN";
        }

        public void UpdateScore(int matches, int attempts)
        {
            if (currentMode == GameMode.TwoPlayer) return;
            if (scoreText != null)
                scoreText.text = $"Matches: {matches}/15";
            if (attemptsText != null)
                attemptsText.text = $"Attempts: {attempts}";
        }

        public void UpdateScore(int matches, int attempts, int[] playerMatches, int[] playerAttempts, int currentPlayer)
        {
            if (currentMode == GameMode.TwoPlayer)
            {
                if (scoreText != null)
                    scoreText.text = $"P1: {playerMatches[0]}  P2: {playerMatches[1]}";
                if (attemptsText != null)
                    attemptsText.text = $"Attempts: {attempts}";
            }
            else
            {
                UpdateScore(matches, attempts);
            }
        }

        public void ShowSolvePanel()
        {
            solvePanel.SetActive(true);
            solveInputField.text = "";
            solveInputField.ActivateInputField();
        }

        public void HideSolvePanel()
        {
            solvePanel.SetActive(false);
        }

        public void ShowVictory(string answer, int matches, int attempts, float time)
        {
            StopTimer();
            solvePuzzleButton.gameObject.SetActive(false);
            victoryPanel.SetActive(true);
            victoryAnswerText.text = answer.ToUpper();
            victoryStatsText.text = $"Matches: {matches}/15\nAttempts: {attempts}\nTime: {FormatTime(time)}";

            if (confettiParticles != null)
                confettiParticles.Play();
        }

        public void ShowVictory2P(string answer, int solvingPlayer, int[] playerMatches, int[] playerAttempts, float time)
        {
            StopTimer();
            solvePuzzleButton.gameObject.SetActive(false);
            turnBanner.SetActive(false);
            victoryPanel.SetActive(true);

            Color winnerColor = solvingPlayer == 0 ? GameConfig.PLAYER1_COLOR : GameConfig.PLAYER2_COLOR;
            victoryTitleText.text = $"PLAYER {solvingPlayer + 1} WINS!";
            victoryTitleText.color = winnerColor;

            victoryAnswerText.text = answer.ToUpper();
            victoryStatsText.text =
                $"Player 1: {playerMatches[0]} matches ({playerAttempts[0]} attempts)\n" +
                $"Player 2: {playerMatches[1]} matches ({playerAttempts[1]} attempts)\n" +
                $"Time: {FormatTime(time)}";

            if (confettiParticles != null)
                confettiParticles.Play();
        }

        public void ShowGameOver()
        {
            StopTimer();
            solvePuzzleButton.gameObject.SetActive(false);
            gameOverPanel.SetActive(true);
            gameOverScoreText.text = $"Score: {scoreText.text}";
            gameOverTimeText.text = $"Time: {timerText.text}";
        }

        public void StartTimer()
        {
            elapsedTime = 0f;
            timerRunning = true;
        }

        public void StopTimer()
        {
            timerRunning = false;
        }

        public float GetElapsedTime()
        {
            return elapsedTime;
        }

        public void ResetUI()
        {
            scoreText.text = "Matches: 0/15";
            attemptsText.text = "Attempts: 0";
            solvePanel.SetActive(false);
            gameOverPanel.SetActive(false);
            victoryPanel.SetActive(false);
            turnBanner.SetActive(false);
            solvePuzzleButton.gameObject.SetActive(true);
            elapsedTime = 0f;
            timerRunning = false;
            UpdateTimerDisplay();
            victoryTitleText.text = "CONGRATULATIONS!";
            victoryTitleText.color = accentGold;

            if (confettiParticles != null)
            {
                confettiParticles.Stop();
                confettiParticles.Clear();
            }
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private void UpdateTimerDisplay()
        {
            if (timerText != null)
                timerText.text = FormatTime(elapsedTime);
        }

        private string FormatTime(float seconds)
        {
            int minutes = Mathf.FloorToInt(seconds / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);
            return $"{minutes}:{secs:D2}";
        }

        private GameObject CreatePanel(string name, Transform parent)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            panel.AddComponent<RectTransform>();
            panel.AddComponent<Image>();
            return panel;
        }

        private TextMeshProUGUI CreateTextElement(string name, Transform parent,
            string text, float fontSize, Color color)
        {
            GameObject textObj = new GameObject(name);
            textObj.transform.SetParent(parent, false);
            textObj.AddComponent<RectTransform>();

            TextMeshProUGUI tmp = textObj.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.fontStyle = FontStyles.Bold;

            return tmp;
        }

        /// <summary>
        /// Creates a modern-styled button with outline glow and shadow.
        /// </summary>
        private GameObject CreateModernButton(string name, Transform parent,
            string label, float fontSize, Color bgColor, Color textColor)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(0, 110);

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = bgColor;

            // Glow outline
            Outline outline = btnObj.AddComponent<Outline>();
            outline.effectColor = new Color(bgColor.r, bgColor.g, bgColor.b, 0.35f);
            outline.effectDistance = new Vector2(2, -2);

            // Drop shadow
            Shadow shadow = btnObj.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.4f);
            shadow.effectDistance = new Vector2(2, -3);

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = Color.Lerp(bgColor, Color.white, 0.15f);
            colors.pressedColor = Color.Lerp(bgColor, Color.black, 0.2f);
            colors.selectedColor = bgColor;
            btn.colors = colors;
            btn.targetGraphic = btnImage;

            TextMeshProUGUI btnText = CreateTextElement(name + "Text", btnObj.transform,
                label, fontSize, textColor);
            RectTransform textRect = btnText.GetComponent<RectTransform>();
            SetFullStretch(textRect);

            return btnObj;
        }

        private void SetFullStretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
