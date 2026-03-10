using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using Rebus.Core;
using Rebus.Data;

namespace Rebus.Board
{
    public class BoardManager : MonoBehaviour
    {
        public static BoardManager Instance { get; private set; }

        private Panel[] panels;
        private RectTransform boardContainer;

        public event Action<Panel> OnPanelClicked;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void SetupBoard(RectTransform parentContainer)
        {
            boardContainer = parentContainer;
            CreatePanels();
        }

        private void CreatePanels()
        {
            // Get 15 random prizes for 15 pairs
            List<PrizeData.Prize> prizes = PrizeData.GetRandomPrizes(GameConfig.TOTAL_PAIRS);

            // Create prize assignments: 2 of each prize
            List<PrizeData.Prize> assignments = new List<PrizeData.Prize>();
            foreach (var prize in prizes)
            {
                assignments.Add(prize);
                assignments.Add(prize);
            }

            // Shuffle assignments
            for (int i = assignments.Count - 1; i > 0; i--)
            {
                int j = UnityEngine.Random.Range(0, i + 1);
                var temp = assignments[i];
                assignments[i] = assignments[j];
                assignments[j] = temp;
            }

            panels = new Panel[GameConfig.BOARD_ROWS * GameConfig.BOARD_COLS];

            float containerWidth = boardContainer.rect.width;
            float containerHeight = boardContainer.rect.height;

            float cellWidth = (containerWidth - (GameConfig.BOARD_COLS - 1) * GameConfig.PANEL_SPACING) / GameConfig.BOARD_COLS;
            float cellHeight = (containerHeight - (GameConfig.BOARD_ROWS - 1) * GameConfig.PANEL_SPACING) / GameConfig.BOARD_ROWS;
            float panelSize = Mathf.Min(cellWidth, cellHeight);

            float totalGridWidth = GameConfig.BOARD_COLS * panelSize + (GameConfig.BOARD_COLS - 1) * GameConfig.PANEL_SPACING;
            float totalGridHeight = GameConfig.BOARD_ROWS * panelSize + (GameConfig.BOARD_ROWS - 1) * GameConfig.PANEL_SPACING;
            float startX = -totalGridWidth / 2f + panelSize / 2f;
            float startY = totalGridHeight / 2f - panelSize / 2f;

            int panelNumber = 1;
            for (int row = 0; row < GameConfig.BOARD_ROWS; row++)
            {
                for (int col = 0; col < GameConfig.BOARD_COLS; col++)
                {
                    int idx = row * GameConfig.BOARD_COLS + col;

                    GameObject panelObj = new GameObject($"Panel_{panelNumber}");
                    panelObj.transform.SetParent(boardContainer, false);

                    RectTransform rect = panelObj.AddComponent<RectTransform>();
                    rect.sizeDelta = new Vector2(panelSize, panelSize);

                    float x = startX + col * (panelSize + GameConfig.PANEL_SPACING);
                    float y = startY - row * (panelSize + GameConfig.PANEL_SPACING);
                    rect.anchoredPosition = new Vector2(x, y);

                    Image img = panelObj.AddComponent<Image>();

                    Panel panel = panelObj.AddComponent<Panel>();
                    panel.Initialize(
                        panelNumber,
                        row, col,
                        assignments[idx].Name,
                        assignments[idx].Color
                    );
                    panel.OnClicked += HandlePanelClicked;

                    panels[idx] = panel;
                    panelNumber++;
                }
            }
        }

        private void HandlePanelClicked(Panel panel)
        {
            OnPanelClicked?.Invoke(panel);
        }

        public Panel GetPanel(int row, int col)
        {
            int idx = row * GameConfig.BOARD_COLS + col;
            if (idx >= 0 && idx < panels.Length)
                return panels[idx];
            return null;
        }

        public void DisablePanel(Panel panel)
        {
            panel.MatchAndRemove();
        }

        public void ResetBoard()
        {
            if (panels != null)
            {
                foreach (var panel in panels)
                {
                    if (panel != null)
                        Destroy(panel.gameObject);
                }
            }
            panels = null;
        }
    }
}
