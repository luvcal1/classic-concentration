using UnityEngine;

namespace Rebus.Core
{
    public enum GameState
    {
        MainMenu,
        Playing,
        PuzzleSolve,
        GameOver,
        Victory
    }

    public enum GameMode
    {
        SinglePlayer,
        TwoPlayer
    }

    public static class GameConfig
    {
        public const int BOARD_ROWS = 5;
        public const int BOARD_COLS = 6;
        public const int TOTAL_PAIRS = 15;
        public const float MISMATCH_DISPLAY_TIME = 1.5f;
        public const float MATCH_ANIMATION_TIME = 0.8f;
        public const float FLIP_ANIMATION_TIME = 0.3f;
        public const float PANEL_SIZE = 150f;
        public const float PANEL_SPACING = 10f;
        public const int REFERENCE_WIDTH = 1080;
        public const int REFERENCE_HEIGHT = 1920;

        // Persists between scenes to pass menu selection to game
        public static GameMode SelectedMode = GameMode.SinglePlayer;

        public static readonly Color PLAYER1_COLOR = new Color(0.2f, 0.6f, 1f);   // Blue
        public static readonly Color PLAYER2_COLOR = new Color(1f, 0.4f, 0.4f);   // Red
    }
}
