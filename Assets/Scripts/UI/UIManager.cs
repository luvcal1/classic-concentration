using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using Rebus.Core;

namespace Rebus.UI
{
    /// <summary>
    /// Singleton manager for all in-game UI elements.
    /// Creates and controls the top bar (score, attempts, timer),
    /// solve puzzle button, solve input panel, game over panel, and victory panel.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("Color Scheme")]
        private readonly Color goldColor = new Color(1f, 0.843f, 0f);            // #FFD700
        private readonly Color darkBlueColor = new Color(0.102f, 0.137f, 0.494f); // #1A237E
        private readonly Color whiteColor = Color.white;

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

            // Start hidden
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
        // Top Bar
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
            topBg.color = darkBlueColor;

            HorizontalLayoutGroup layout = topBar.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(30, 30, 10, 10);
            layout.spacing = 20;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            scoreText = CreateTextElement("ScoreText", topBar.transform, "Matches: 0/15", 36, goldColor);
            attemptsText = CreateTextElement("AttemptsText", topBar.transform, "Attempts: 0", 36, whiteColor);
            timerText = CreateTextElement("TimerText", topBar.transform, "0:00", 36, whiteColor);
        }

        // ------------------------------------------------------------------
        // Solve Puzzle Button (always visible at bottom during gameplay)
        // ------------------------------------------------------------------

        private void CreateSolvePuzzleButton()
        {
            GameObject btnObj = CreateButtonObject("SolvePuzzleButton", mainCanvas.transform,
                "SOLVE PUZZLE", 48, goldColor, darkBlueColor);

            RectTransform btnRect = btnObj.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0.1f, 0);
            btnRect.anchorMax = new Vector2(0.9f, 0);
            btnRect.pivot = new Vector2(0.5f, 0);
            btnRect.sizeDelta = new Vector2(0, 120);
            btnRect.anchoredPosition = new Vector2(0, 40);

            solvePuzzleButton = btnObj.GetComponent<Button>();
            solvePuzzleButton.onClick.AddListener(() => OnSolvePuzzleClicked?.Invoke());
        }

        // ------------------------------------------------------------------
        // Solve Input Panel (overlay)
        // ------------------------------------------------------------------

        private void CreateSolvePanel()
        {
            // Full-screen dimmed overlay
            solvePanel = CreatePanel("SolvePanel", mainCanvas.transform);
            RectTransform overlayRect = solvePanel.GetComponent<RectTransform>();
            SetFullStretch(overlayRect);
            solvePanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.7f);

            // Center card
            GameObject card = CreatePanel("SolveCard", solvePanel.transform);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.05f, 0.25f);
            cardRect.anchorMax = new Vector2(0.95f, 0.75f);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;
            card.GetComponent<Image>().color = darkBlueColor;

            VerticalLayoutGroup vLayout = card.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(40, 40, 40, 40);
            vLayout.spacing = 30;
            vLayout.childAlignment = TextAnchor.MiddleCenter;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = false;
            vLayout.childForceExpandWidth = true;
            vLayout.childForceExpandHeight = false;

            // Title
            TextMeshProUGUI solveTitle = CreateTextElement("SolveTitle", card.transform,
                "SOLVE THE PUZZLE", 48, goldColor);
            LayoutElement titleLE = solveTitle.gameObject.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 80;

            // Input field
            GameObject inputObj = new GameObject("SolveInput");
            inputObj.transform.SetParent(card.transform, false);
            Image inputBg = inputObj.AddComponent<Image>();
            inputBg.color = whiteColor;

            solveInputField = inputObj.AddComponent<TMP_InputField>();

            // Input text area
            RectTransform inputRect = inputObj.GetComponent<RectTransform>();
            inputRect.sizeDelta = new Vector2(0, 100);

            GameObject textArea = new GameObject("TextArea");
            textArea.transform.SetParent(inputObj.transform, false);
            RectTransform textAreaRect = textArea.AddComponent<RectTransform>();
            SetFullStretch(textAreaRect);
            textAreaRect.offsetMin = new Vector2(20, 5);
            textAreaRect.offsetMax = new Vector2(-20, -5);
            textArea.AddComponent<RectMask2D>();

            // Placeholder
            GameObject placeholderObj = new GameObject("Placeholder");
            placeholderObj.transform.SetParent(textArea.transform, false);
            TextMeshProUGUI placeholder = placeholderObj.AddComponent<TextMeshProUGUI>();
            placeholder.text = "Type your answer...";
            placeholder.fontSize = 36;
            placeholder.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            placeholder.alignment = TextAlignmentOptions.MidlineLeft;
            RectTransform phRect = placeholderObj.GetComponent<RectTransform>();
            SetFullStretch(phRect);

            // Input text
            GameObject inputTextObj = new GameObject("Text");
            inputTextObj.transform.SetParent(textArea.transform, false);
            TextMeshProUGUI inputText = inputTextObj.AddComponent<TextMeshProUGUI>();
            inputText.fontSize = 36;
            inputText.color = Color.black;
            inputText.alignment = TextAlignmentOptions.MidlineLeft;
            RectTransform itRect = inputTextObj.GetComponent<RectTransform>();
            SetFullStretch(itRect);

            solveInputField.textViewport = textAreaRect;
            solveInputField.textComponent = inputText;
            solveInputField.placeholder = placeholder;
            solveInputField.characterLimit = 100;
            solveInputField.contentType = TMP_InputField.ContentType.Standard;

            LayoutElement inputLE = inputObj.AddComponent<LayoutElement>();
            inputLE.preferredHeight = 100;

            // Button row
            GameObject buttonRow = new GameObject("ButtonRow");
            buttonRow.transform.SetParent(card.transform, false);
            RectTransform rowRect = buttonRow.AddComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(0, 110);
            HorizontalLayoutGroup hLayout = buttonRow.AddComponent<HorizontalLayoutGroup>();
            hLayout.spacing = 30;
            hLayout.childAlignment = TextAnchor.MiddleCenter;
            hLayout.childControlWidth = true;
            hLayout.childControlHeight = true;
            hLayout.childForceExpandWidth = true;
            hLayout.childForceExpandHeight = true;

            LayoutElement rowLE = buttonRow.AddComponent<LayoutElement>();
            rowLE.preferredHeight = 110;

            // Cancel button
            GameObject cancelObj = CreateButtonObject("CancelBtn", buttonRow.transform,
                "CANCEL", 36, new Color(0.8f, 0.2f, 0.2f), whiteColor);
            cancelButton = cancelObj.GetComponent<Button>();
            cancelButton.onClick.AddListener(() =>
            {
                OnSolveCancelled?.Invoke();
                HideSolvePanel();
            });

            // Submit button
            GameObject submitObj = CreateButtonObject("SubmitBtn", buttonRow.transform,
                "SUBMIT", 36, new Color(0.2f, 0.7f, 0.2f), whiteColor);
            submitButton = submitObj.GetComponent<Button>();
            submitButton.onClick.AddListener(() =>
            {
                string guess = solveInputField.text.Trim();
                if (!string.IsNullOrEmpty(guess))
                {
                    OnSolveSubmitted?.Invoke(guess);
                }
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
            gameOverPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);

            GameObject card = CreatePanel("GameOverCard", gameOverPanel.transform);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.1f, 0.25f);
            cardRect.anchorMax = new Vector2(0.9f, 0.75f);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;
            card.GetComponent<Image>().color = darkBlueColor;

            VerticalLayoutGroup vLayout = card.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(40, 40, 50, 50);
            vLayout.spacing = 30;
            vLayout.childAlignment = TextAnchor.MiddleCenter;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = false;
            vLayout.childForceExpandWidth = true;
            vLayout.childForceExpandHeight = false;

            TextMeshProUGUI title = CreateTextElement("GameOverTitle", card.transform,
                "GAME OVER", 64, goldColor);
            LayoutElement titleLE = title.gameObject.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 100;

            gameOverScoreText = CreateTextElement("GameOverScore", card.transform,
                "Score: 0", 42, whiteColor);
            LayoutElement scoreLE = gameOverScoreText.gameObject.AddComponent<LayoutElement>();
            scoreLE.preferredHeight = 60;

            gameOverTimeText = CreateTextElement("GameOverTime", card.transform,
                "Time: 0:00", 42, whiteColor);
            LayoutElement timeLE = gameOverTimeText.gameObject.AddComponent<LayoutElement>();
            timeLE.preferredHeight = 60;

            // Play Again
            GameObject playAgainObj = CreateButtonObject("PlayAgainBtn", card.transform,
                "PLAY AGAIN", 42, goldColor, darkBlueColor);
            playAgainButton = playAgainObj.GetComponent<Button>();
            playAgainButton.onClick.AddListener(() => OnPlayAgainClicked?.Invoke());
            LayoutElement paLE = playAgainObj.AddComponent<LayoutElement>();
            paLE.preferredHeight = 110;

            // Main Menu
            GameObject menuObj = CreateButtonObject("MainMenuBtn", card.transform,
                "MAIN MENU", 42, whiteColor, darkBlueColor);
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
            victoryPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.8f);

            GameObject card = CreatePanel("VictoryCard", victoryPanel.transform);
            RectTransform cardRect = card.GetComponent<RectTransform>();
            cardRect.anchorMin = new Vector2(0.05f, 0.15f);
            cardRect.anchorMax = new Vector2(0.95f, 0.85f);
            cardRect.offsetMin = Vector2.zero;
            cardRect.offsetMax = Vector2.zero;
            card.GetComponent<Image>().color = darkBlueColor;

            VerticalLayoutGroup vLayout = card.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(40, 40, 50, 50);
            vLayout.spacing = 25;
            vLayout.childAlignment = TextAnchor.MiddleCenter;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = false;
            vLayout.childForceExpandWidth = true;
            vLayout.childForceExpandHeight = false;

            // Confetti particle system
            CreateConfettiEffect(victoryPanel.transform);

            victoryTitleText = CreateTextElement("VictoryTitle", card.transform,
                "CONGRATULATIONS!", 64, goldColor);
            LayoutElement titleLE = victoryTitleText.gameObject.AddComponent<LayoutElement>();
            titleLE.preferredHeight = 100;

            TextMeshProUGUI answerLabel = CreateTextElement("AnswerLabel", card.transform,
                "The answer was:", 36, whiteColor);
            LayoutElement alLE = answerLabel.gameObject.AddComponent<LayoutElement>();
            alLE.preferredHeight = 50;

            victoryAnswerText = CreateTextElement("VictoryAnswer", card.transform,
                "", 52, goldColor);
            LayoutElement vaLE = victoryAnswerText.gameObject.AddComponent<LayoutElement>();
            vaLE.preferredHeight = 80;

            victoryStatsText = CreateTextElement("VictoryStats", card.transform,
                "", 36, whiteColor);
            LayoutElement vsLE = victoryStatsText.gameObject.AddComponent<LayoutElement>();
            vsLE.preferredHeight = 120;

            // Play Again
            GameObject playAgainObj = CreateButtonObject("VictoryPlayAgainBtn", card.transform,
                "PLAY AGAIN", 42, goldColor, darkBlueColor);
            victoryPlayAgainButton = playAgainObj.GetComponent<Button>();
            victoryPlayAgainButton.onClick.AddListener(() => OnPlayAgainClicked?.Invoke());
            LayoutElement paLE = playAgainObj.AddComponent<LayoutElement>();
            paLE.preferredHeight = 110;

            // Main Menu
            GameObject menuObj = CreateButtonObject("VictoryMainMenuBtn", card.transform,
                "MAIN MENU", 42, whiteColor, darkBlueColor);
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
            main.startColor = new ParticleSystem.MinMaxGradient(goldColor, whiteColor);
            main.gravityModifier = 0.5f;
            main.maxParticles = 200;
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
                    new GradientColorKey(goldColor, 0f),
                    new GradientColorKey(whiteColor, 0.5f),
                    new GradientColorKey(goldColor, 1f)
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
            bannerRect.sizeDelta = new Vector2(0, 70);
            bannerRect.anchoredPosition = new Vector2(0, -120); // Below top bar

            turnBannerBg = turnBanner.GetComponent<Image>();
            turnBannerBg.color = GameConfig.PLAYER1_COLOR;

            turnText = CreateTextElement("TurnText", turnBanner.transform,
                "PLAYER 1'S TURN", 32, Color.white);
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
                // In 2P mode, show per-player stats in the top bar
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
            if (currentMode == GameMode.TwoPlayer) return; // Use overload below
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

        /// <summary>
        /// Shows the solve puzzle input overlay.
        /// </summary>
        public void ShowSolvePanel()
        {
            solvePanel.SetActive(true);
            solveInputField.text = "";
            solveInputField.ActivateInputField();
        }

        /// <summary>
        /// Hides the solve puzzle input overlay.
        /// </summary>
        public void HideSolvePanel()
        {
            solvePanel.SetActive(false);
        }

        /// <summary>
        /// Shows the victory panel with stats and confetti.
        /// </summary>
        public void ShowVictory(string answer, int matches, int attempts, float time)
        {
            StopTimer();
            solvePuzzleButton.gameObject.SetActive(false);
            victoryPanel.SetActive(true);
            victoryAnswerText.text = answer.ToUpper();
            victoryStatsText.text = $"Matches: {matches}/15\nAttempts: {attempts}\nTime: {FormatTime(time)}";

            if (confettiParticles != null)
            {
                confettiParticles.Play();
            }
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

        /// <summary>
        /// Shows the game over panel with final stats.
        /// </summary>
        public void ShowGameOver()
        {
            StopTimer();
            solvePuzzleButton.gameObject.SetActive(false);
            gameOverPanel.SetActive(true);
            gameOverScoreText.text = $"Score: {scoreText.text}";
            gameOverTimeText.text = $"Time: {timerText.text}";
        }

        /// <summary>
        /// Starts the gameplay timer from zero.
        /// </summary>
        public void StartTimer()
        {
            elapsedTime = 0f;
            timerRunning = true;
        }

        /// <summary>
        /// Stops the gameplay timer.
        /// </summary>
        public void StopTimer()
        {
            timerRunning = false;
        }

        /// <summary>
        /// Returns the elapsed time in seconds.
        /// </summary>
        public float GetElapsedTime()
        {
            return elapsedTime;
        }

        /// <summary>
        /// Resets all UI to initial gameplay state.
        /// </summary>
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
            victoryTitleText.color = goldColor;

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
            RectTransform rect = panel.AddComponent<RectTransform>();
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

        private GameObject CreateButtonObject(string name, Transform parent,
            string label, float fontSize, Color bgColor, Color textColor)
        {
            GameObject btnObj = new GameObject(name);
            btnObj.transform.SetParent(parent, false);
            RectTransform btnRect = btnObj.AddComponent<RectTransform>();
            btnRect.sizeDelta = new Vector2(0, 110); // Minimum 100px touch target

            Image btnImage = btnObj.AddComponent<Image>();
            btnImage.color = bgColor;

            Button btn = btnObj.AddComponent<Button>();
            ColorBlock colors = btn.colors;
            colors.normalColor = bgColor;
            colors.highlightedColor = bgColor * 1.1f;
            colors.pressedColor = bgColor * 0.8f;
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
