using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;
using Rebus.Board;
using Rebus.Puzzle;
using Rebus.UI;
using Rebus.Audio;

namespace Rebus.Core
{
    public enum SceneType
    {
        MainMenu,
        Game
    }

    public class SceneBootstrapper : MonoBehaviour
    {
        [SerializeField] private SceneType sceneType = SceneType.Game;

        private void Awake()
        {
            // Ensure EventSystem exists
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<InputSystemUIInputModule>();
            }

            // Ensure AudioManager exists
            if (AudioManager.Instance == null)
            {
                GameObject audioObj = new GameObject("AudioManager");
                audioObj.AddComponent<AudioManager>();
            }

            // Ensure Camera exists
            if (Camera.main == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                camObj.tag = "MainCamera";
                Camera cam = camObj.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = new Color(0.06f, 0.06f, 0.15f);
                cam.orthographic = true;
                camObj.AddComponent<AudioListener>();
            }

            switch (sceneType)
            {
                case SceneType.MainMenu:
                    BuildMainMenu();
                    break;
                case SceneType.Game:
                    BuildGameScene();
                    break;
            }
        }

        private void BuildMainMenu()
        {
            GameObject menuObj = new GameObject("MainMenu");
            menuObj.AddComponent<MainMenuUI>();
        }

        private void BuildGameScene()
        {
            // --- Main Game Canvas ---
            GameObject canvasObj = new GameObject("GameCanvas");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(GameConfig.REFERENCE_WIDTH, GameConfig.REFERENCE_HEIGHT);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObj.AddComponent<GraphicRaycaster>();

            // --- Background ---
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(canvasObj.transform, false);
            RectTransform bgRect = bg.AddComponent<RectTransform>();
            StretchFull(bgRect);
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.06f, 0.08f, 0.18f);

            // --- Puzzle Area (behind the board) ---
            // This area sits in the center and shows the rebus puzzle as panels are removed
            GameObject puzzleArea = new GameObject("PuzzleArea");
            puzzleArea.transform.SetParent(canvasObj.transform, false);
            RectTransform puzzleRect = puzzleArea.AddComponent<RectTransform>();
            // Position: centered, behind the board area
            puzzleRect.anchorMin = new Vector2(0.03f, 0.22f);
            puzzleRect.anchorMax = new Vector2(0.97f, 0.72f);
            puzzleRect.offsetMin = Vector2.zero;
            puzzleRect.offsetMax = Vector2.zero;

            RebusPuzzleRenderer puzzleRenderer = puzzleArea.AddComponent<RebusPuzzleRenderer>();
            puzzleRenderer.Initialize(puzzleRect);

            // --- Board Area ---
            // Overlays the puzzle area - panels hide the puzzle underneath
            GameObject boardArea = new GameObject("BoardArea");
            boardArea.transform.SetParent(canvasObj.transform, false);
            RectTransform boardRect = boardArea.AddComponent<RectTransform>();
            // Same position as puzzle area so panels cover the puzzle
            boardRect.anchorMin = new Vector2(0.03f, 0.22f);
            boardRect.anchorMax = new Vector2(0.97f, 0.72f);
            boardRect.offsetMin = Vector2.zero;
            boardRect.offsetMax = Vector2.zero;

            BoardManager boardManager = boardArea.AddComponent<BoardManager>();

            // --- UI Manager ---
            GameObject uiObj = new GameObject("UIManager");
            UIManager uiManager = uiObj.AddComponent<UIManager>();

            // --- Game Manager ---
            GameObject gmObj = new GameObject("GameManager");
            GameManager gameManager = gmObj.AddComponent<GameManager>();

            // --- Initialize after all components are created ---
            // Use a delayed start to ensure all Awake() methods have run
            StartCoroutine(DelayedStart(boardManager, boardRect, gameManager));
        }

        private System.Collections.IEnumerator DelayedStart(
            BoardManager boardManager, RectTransform boardRect, GameManager gameManager)
        {
            // Wait one frame for all Awake() to complete and layout to calculate
            yield return null;
            yield return null; // Extra frame for RectTransform layout

            // Force layout rebuild so boardRect has correct dimensions
            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(boardRect);

            boardManager.SetupBoard(boardRect);
            gameManager.StartGame();
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
