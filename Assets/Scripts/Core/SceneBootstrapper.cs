using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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

        // Modern palette - shared across scene setup
        private static readonly Color BG_DARK = new Color(0.06f, 0.06f, 0.12f);
        private static readonly Color BG_MID = new Color(0.08f, 0.08f, 0.18f);
        private static readonly Color BG_ACCENT = new Color(0.05f, 0.02f, 0.15f);

        private void Awake()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
                eventSystem.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
#else
                eventSystem.AddComponent<StandaloneInputModule>();
#endif
            }

            if (AudioManager.Instance == null)
            {
                GameObject audioObj = new GameObject("AudioManager");
                audioObj.AddComponent<AudioManager>();
            }

            if (Camera.main == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                camObj.tag = "MainCamera";
                Camera cam = camObj.AddComponent<Camera>();
                cam.clearFlags = CameraClearFlags.SolidColor;
                cam.backgroundColor = BG_DARK;
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

            // --- Layered Background ---
            // Base dark layer
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(canvasObj.transform, false);
            RectTransform bgRect = bg.AddComponent<RectTransform>();
            StretchFull(bgRect);
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = BG_DARK;

            // Upper gradient overlay (lighter at top)
            GameObject bgTop = new GameObject("BgTopGrad");
            bgTop.transform.SetParent(canvasObj.transform, false);
            RectTransform bgTopRect = bgTop.AddComponent<RectTransform>();
            bgTopRect.anchorMin = new Vector2(0, 0.5f);
            bgTopRect.anchorMax = new Vector2(1, 1);
            bgTopRect.offsetMin = Vector2.zero;
            bgTopRect.offsetMax = Vector2.zero;
            Image bgTopImg = bgTop.AddComponent<Image>();
            bgTopImg.color = new Color(BG_MID.r, BG_MID.g, BG_MID.b, 0.6f);

            // Subtle colored accent in center
            GameObject bgAccent = new GameObject("BgAccent");
            bgAccent.transform.SetParent(canvasObj.transform, false);
            RectTransform bgAccentRect = bgAccent.AddComponent<RectTransform>();
            bgAccentRect.anchorMin = new Vector2(0, 0.2f);
            bgAccentRect.anchorMax = new Vector2(1, 0.7f);
            bgAccentRect.offsetMin = Vector2.zero;
            bgAccentRect.offsetMax = Vector2.zero;
            Image bgAccentImg = bgAccent.AddComponent<Image>();
            bgAccentImg.color = new Color(0f, 0.15f, 0.3f, 0.25f);

            // Vignette edges (dark borders)
            CreateVignetteEdge(canvasObj.transform, new Vector2(0, 0), new Vector2(1, 0.08f), BG_DARK, 0.8f); // bottom
            CreateVignetteEdge(canvasObj.transform, new Vector2(0, 0.92f), new Vector2(1, 1), BG_DARK, 0.5f); // top

            // --- Puzzle Area ---
            GameObject puzzleArea = new GameObject("PuzzleArea");
            puzzleArea.transform.SetParent(canvasObj.transform, false);
            RectTransform puzzleRect = puzzleArea.AddComponent<RectTransform>();
            puzzleRect.anchorMin = new Vector2(0.03f, 0.22f);
            puzzleRect.anchorMax = new Vector2(0.97f, 0.72f);
            puzzleRect.offsetMin = Vector2.zero;
            puzzleRect.offsetMax = Vector2.zero;

            RebusPuzzleRenderer puzzleRenderer = puzzleArea.AddComponent<RebusPuzzleRenderer>();
            puzzleRenderer.Initialize(puzzleRect);

            // --- Board Area ---
            GameObject boardArea = new GameObject("BoardArea");
            boardArea.transform.SetParent(canvasObj.transform, false);
            RectTransform boardRect = boardArea.AddComponent<RectTransform>();
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

            StartCoroutine(DelayedStart(boardManager, boardRect, gameManager));
        }

        private void CreateVignetteEdge(Transform parent, Vector2 anchorMin, Vector2 anchorMax, Color color, float alpha)
        {
            GameObject edge = new GameObject("VignetteEdge");
            edge.transform.SetParent(parent, false);
            RectTransform rect = edge.AddComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            Image img = edge.AddComponent<Image>();
            img.color = new Color(color.r, color.g, color.b, alpha);
        }

        private System.Collections.IEnumerator DelayedStart(
            BoardManager boardManager, RectTransform boardRect, GameManager gameManager)
        {
            yield return null;
            yield return null;

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
