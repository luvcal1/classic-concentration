using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Rebus.UI
{
    /// <summary>
    /// Handles the puzzle solving modal interaction, including input validation,
    /// incorrect answer feedback with shake animation, and mobile keyboard support.
    /// </summary>
    public class PuzzleSolveUI : MonoBehaviour
    {
        [Header("References (auto-wired if null)")]
        [SerializeField] private GameObject overlayRoot;
        [SerializeField] private TextMeshProUGUI puzzlePreviewText;
        [SerializeField] private TMP_InputField guessInputField;
        [SerializeField] private Button submitButton;
        [SerializeField] private Button cancelButton;
        [SerializeField] private TextMeshProUGUI feedbackText;
        [SerializeField] private RectTransform cardTransform;

        private readonly Color goldColor = new Color(1f, 0.843f, 0f);
        private readonly Color darkBlueColor = new Color(0.102f, 0.137f, 0.494f);
        private readonly Color errorColor = new Color(1f, 0.3f, 0.3f);

        private Canvas canvas;
        private bool isShaking;
        private Coroutine shakeCoroutine;

        // Callback to the GameManager for answer validation
        public System.Action<string> OnGuessSubmitted;
        public System.Action OnCancelled;

        private TouchScreenKeyboard mobileKeyboard;

        private void Awake()
        {
            if (overlayRoot == null)
            {
                BuildUI();
            }

            Hide();
        }

        // ------------------------------------------------------------------
        // Programmatic UI Construction
        // ------------------------------------------------------------------

        private void BuildUI()
        {
            // Canvas
            GameObject canvasObj = new GameObject("PuzzleSolveCanvas");
            canvasObj.transform.SetParent(transform, false);
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 200; // Above UIManager canvas

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // Full-screen overlay dimming
            overlayRoot = new GameObject("Overlay");
            overlayRoot.transform.SetParent(canvasObj.transform, false);
            RectTransform overlayRect = overlayRoot.AddComponent<RectTransform>();
            StretchFull(overlayRect);
            Image overlayImg = overlayRoot.AddComponent<Image>();
            overlayImg.color = new Color(0, 0, 0, 0.75f);

            // Card panel
            GameObject card = new GameObject("SolveCard");
            card.transform.SetParent(overlayRoot.transform, false);
            cardTransform = card.AddComponent<RectTransform>();
            cardTransform.anchorMin = new Vector2(0.05f, 0.2f);
            cardTransform.anchorMax = new Vector2(0.95f, 0.8f);
            cardTransform.offsetMin = Vector2.zero;
            cardTransform.offsetMax = Vector2.zero;

            Image cardBg = card.AddComponent<Image>();
            cardBg.color = darkBlueColor;

            VerticalLayoutGroup vLayout = card.AddComponent<VerticalLayoutGroup>();
            vLayout.padding = new RectOffset(40, 40, 40, 40);
            vLayout.spacing = 25;
            vLayout.childAlignment = TextAnchor.MiddleCenter;
            vLayout.childControlWidth = true;
            vLayout.childControlHeight = false;
            vLayout.childForceExpandWidth = true;
            vLayout.childForceExpandHeight = false;

            // Puzzle preview at top
            puzzlePreviewText = CreateText("PuzzlePreview", card.transform,
                "[ PUZZLE ]", 40, goldColor, 120);

            // Instruction text
            CreateText("InstructionText", card.transform,
                "What does the rebus represent?", 32, Color.white, 50);

            // Input field
            CreateInputField(card.transform);

            // Feedback text (hidden by default)
            feedbackText = CreateText("Feedback", card.transform,
                "", 34, errorColor, 50);
            feedbackText.gameObject.SetActive(false);

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

            // Cancel
            cancelButton = CreateButton("CancelBtn", buttonRow.transform,
                "CANCEL", new Color(0.8f, 0.2f, 0.2f));
            cancelButton.onClick.AddListener(HandleCancel);

            // Submit
            submitButton = CreateButton("SubmitBtn", buttonRow.transform,
                "SUBMIT", new Color(0.2f, 0.7f, 0.2f));
            submitButton.onClick.AddListener(HandleSubmit);
        }

        private void CreateInputField(Transform parent)
        {
            GameObject inputObj = new GameObject("GuessInput");
            inputObj.transform.SetParent(parent, false);

            Image inputBg = inputObj.AddComponent<Image>();
            inputBg.color = Color.white;

            guessInputField = inputObj.AddComponent<TMP_InputField>();

            LayoutElement inputLE = inputObj.AddComponent<LayoutElement>();
            inputLE.preferredHeight = 100;

            // Text area
            GameObject textArea = new GameObject("TextArea");
            textArea.transform.SetParent(inputObj.transform, false);
            RectTransform taRect = textArea.AddComponent<RectTransform>();
            StretchFull(taRect);
            taRect.offsetMin = new Vector2(20, 5);
            taRect.offsetMax = new Vector2(-20, -5);
            textArea.AddComponent<RectMask2D>();

            // Placeholder
            GameObject phObj = new GameObject("Placeholder");
            phObj.transform.SetParent(textArea.transform, false);
            TextMeshProUGUI ph = phObj.AddComponent<TextMeshProUGUI>();
            ph.text = "Type your answer...";
            ph.fontSize = 36;
            ph.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            ph.alignment = TextAlignmentOptions.MidlineLeft;
            StretchFull(phObj.GetComponent<RectTransform>());

            // Text
            GameObject txtObj = new GameObject("Text");
            txtObj.transform.SetParent(textArea.transform, false);
            TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
            txt.fontSize = 36;
            txt.color = Color.black;
            txt.alignment = TextAlignmentOptions.MidlineLeft;
            StretchFull(txtObj.GetComponent<RectTransform>());

            guessInputField.textViewport = taRect;
            guessInputField.textComponent = txt;
            guessInputField.placeholder = ph;
            guessInputField.characterLimit = 100;
            guessInputField.contentType = TMP_InputField.ContentType.Standard;
            guessInputField.onSubmit.AddListener(_ => HandleSubmit());
        }

        // ------------------------------------------------------------------
        // Public API
        // ------------------------------------------------------------------

        /// <summary>
        /// Shows the solve overlay with the given puzzle preview text.
        /// </summary>
        public void Show(string puzzleText = "")
        {
            if (!string.IsNullOrEmpty(puzzleText))
                puzzlePreviewText.text = puzzleText;

            overlayRoot.SetActive(true);
            feedbackText.gameObject.SetActive(false);
            guessInputField.text = "";

            // Focus the input field and open mobile keyboard
            guessInputField.ActivateInputField();
            OpenMobileKeyboard();
        }

        /// <summary>
        /// Hides the solve overlay and dismisses the mobile keyboard.
        /// </summary>
        public void Hide()
        {
            if (overlayRoot != null)
                overlayRoot.SetActive(false);

            CloseMobileKeyboard();

            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                shakeCoroutine = null;
                isShaking = false;
            }
        }

        /// <summary>
        /// Displays incorrect answer feedback with a shake animation.
        /// </summary>
        public void ShowIncorrectFeedback()
        {
            feedbackText.text = "Incorrect! Try again.";
            feedbackText.gameObject.SetActive(true);

            if (shakeCoroutine != null)
                StopCoroutine(shakeCoroutine);

            shakeCoroutine = StartCoroutine(ShakeAnimation());

            guessInputField.text = "";
            guessInputField.ActivateInputField();
        }

        /// <summary>
        /// Updates the puzzle preview text shown at the top of the overlay.
        /// </summary>
        public void UpdatePuzzlePreview(string text)
        {
            if (puzzlePreviewText != null)
                puzzlePreviewText.text = text;
        }

        // ------------------------------------------------------------------
        // Input Handling
        // ------------------------------------------------------------------

        private void HandleSubmit()
        {
            string guess = guessInputField.text.Trim();

            if (string.IsNullOrEmpty(guess))
            {
                feedbackText.text = "Please enter an answer.";
                feedbackText.gameObject.SetActive(true);
                return;
            }

            OnGuessSubmitted?.Invoke(guess);
        }

        private void HandleCancel()
        {
            Hide();
            OnCancelled?.Invoke();
        }

        private void Update()
        {
            // Handle mobile keyboard status
            if (mobileKeyboard != null)
            {
                if (mobileKeyboard.status == TouchScreenKeyboard.Status.Done)
                {
                    guessInputField.text = mobileKeyboard.text;
                    HandleSubmit();
                }
                else if (mobileKeyboard.status == TouchScreenKeyboard.Status.Canceled)
                {
                    // Keyboard was dismissed, do nothing special
                    mobileKeyboard = null;
                }
            }
        }

        // ------------------------------------------------------------------
        // Mobile Keyboard
        // ------------------------------------------------------------------

        private void OpenMobileKeyboard()
        {
            if (TouchScreenKeyboard.isSupported)
            {
                mobileKeyboard = TouchScreenKeyboard.Open(
                    guessInputField.text,
                    TouchScreenKeyboardType.Default,
                    false, false, false, false,
                    "Type your answer..."
                );
            }
        }

        private void CloseMobileKeyboard()
        {
            if (mobileKeyboard != null && mobileKeyboard.active)
            {
                mobileKeyboard.active = false;
                mobileKeyboard = null;
            }
        }

        // ------------------------------------------------------------------
        // Shake Animation
        // ------------------------------------------------------------------

        private IEnumerator ShakeAnimation()
        {
            isShaking = true;
            Vector2 originalPos = cardTransform.anchoredPosition;
            float elapsed = 0f;
            float duration = 0.4f;
            float magnitude = 20f;

            while (elapsed < duration)
            {
                float x = originalPos.x + Random.Range(-magnitude, magnitude)
                    * (1f - elapsed / duration);
                cardTransform.anchoredPosition = new Vector2(x, originalPos.y);

                elapsed += Time.deltaTime;
                yield return null;
            }

            cardTransform.anchoredPosition = originalPos;
            isShaking = false;
            shakeCoroutine = null;
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private TextMeshProUGUI CreateText(string name, Transform parent,
            string text, float fontSize, Color color, float height)
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

            LayoutElement le = obj.AddComponent<LayoutElement>();
            le.preferredHeight = height;

            return tmp;
        }

        private Button CreateButton(string name, Transform parent, string label, Color bgColor)
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

            GameObject txtObj = new GameObject("Label");
            txtObj.transform.SetParent(btnObj.transform, false);
            RectTransform txtRect = txtObj.AddComponent<RectTransform>();
            StretchFull(txtRect);

            TextMeshProUGUI txt = txtObj.AddComponent<TextMeshProUGUI>();
            txt.text = label;
            txt.fontSize = 36;
            txt.color = Color.white;
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
