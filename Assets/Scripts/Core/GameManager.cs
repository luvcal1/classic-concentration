using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections;
using Rebus.Board;
using Rebus.Puzzle;
using Rebus.UI;
using Rebus.Audio;

namespace Rebus.Core
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public GameState CurrentState { get; private set; }
        public GameMode Mode { get; private set; }
        public int MatchesFound { get; private set; }
        public int Attempts { get; private set; }

        // Two-player state
        public int CurrentPlayer { get; private set; } // 0 or 1
        public int[] PlayerMatches { get; private set; } = new int[2];
        public int[] PlayerAttempts { get; private set; } = new int[2];

        public event Action<GameState> OnStateChanged;
        public event Action<int> OnMatchFound;
        public event Action OnMismatch;
        public event Action<int> OnTurnChanged; // passes new player index

        private Panel firstSelected;
        private Panel secondSelected;
        private bool isProcessing;

        private RebusPuzzle currentPuzzle;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void StartGame()
        {
            Mode = GameConfig.SelectedMode;
            MatchesFound = 0;
            Attempts = 0;
            CurrentPlayer = 0;
            PlayerMatches[0] = 0;
            PlayerMatches[1] = 0;
            PlayerAttempts[0] = 0;
            PlayerAttempts[1] = 0;
            firstSelected = null;
            secondSelected = null;
            isProcessing = false;

            currentPuzzle = PuzzleDatabase.GetRandomPuzzle();

            if (RebusPuzzleRenderer.Instance != null)
                RebusPuzzleRenderer.Instance.LoadPuzzle(currentPuzzle);

            if (BoardManager.Instance != null)
                BoardManager.Instance.OnPanelClicked += OnPanelSelected;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.SetGameMode(Mode);
                UIManager.Instance.ResetUI();
                UIManager.Instance.StartTimer();
                UIManager.Instance.OnSolvePuzzleClicked += OnSolvePuzzleClicked;
                UIManager.Instance.OnSolveSubmitted += OnSolveSubmitted;
                UIManager.Instance.OnSolveCancelled += OnSolveCancelled;
                UIManager.Instance.OnPlayAgainClicked += OnPlayAgain;
                UIManager.Instance.OnMainMenuClicked += OnMainMenu;

                if (Mode == GameMode.TwoPlayer)
                    UIManager.Instance.UpdateTurn(CurrentPlayer);
            }

            SetState(GameState.Playing);
        }

        private void SetState(GameState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
        }

        private void SwitchTurn()
        {
            if (Mode != GameMode.TwoPlayer) return;

            CurrentPlayer = 1 - CurrentPlayer;
            OnTurnChanged?.Invoke(CurrentPlayer);

            if (UIManager.Instance != null)
                UIManager.Instance.UpdateTurn(CurrentPlayer);
        }

        public void OnPanelSelected(Panel panel)
        {
            if (CurrentState != GameState.Playing) return;
            if (isProcessing) return;
            if (panel.IsFlipped || panel.IsMatched || panel.IsAnimating) return;

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlaySound(AudioManager.Sound.PanelFlip);

            if (firstSelected == null)
            {
                firstSelected = panel;
                panel.FlipToBack();
            }
            else if (secondSelected == null && panel != firstSelected)
            {
                secondSelected = panel;
                panel.FlipToBack(() => CheckMatch());
            }
        }

        private void CheckMatch()
        {
            if (firstSelected == null || secondSelected == null) return;

            isProcessing = true;
            Attempts++;
            PlayerAttempts[CurrentPlayer]++;

            if (firstSelected.PrizeName == secondSelected.PrizeName)
            {
                MatchesFound++;
                PlayerMatches[CurrentPlayer]++;

                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySound(AudioManager.Sound.Match);

                OnMatchFound?.Invoke(MatchesFound);

                if (UIManager.Instance != null)
                    UIManager.Instance.UpdateScore(MatchesFound, Attempts, PlayerMatches, PlayerAttempts, CurrentPlayer);

                Panel p1 = firstSelected;
                Panel p2 = secondSelected;
                firstSelected = null;
                secondSelected = null;

                // Match = same player goes again (no turn switch)
                StartCoroutine(ProcessMatch(p1, p2));
            }
            else
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySound(AudioManager.Sound.Mismatch);

                OnMismatch?.Invoke();

                if (UIManager.Instance != null)
                    UIManager.Instance.UpdateScore(MatchesFound, Attempts, PlayerMatches, PlayerAttempts, CurrentPlayer);

                StartCoroutine(ProcessMismatch());
            }
        }

        private IEnumerator ProcessMatch(Panel p1, Panel p2)
        {
            yield return new WaitForSeconds(0.3f);

            p1.MatchAndRemove();
            p2.MatchAndRemove();

            if (RebusPuzzleRenderer.Instance != null)
            {
                RebusPuzzleRenderer.Instance.UpdateVisibility(MatchesFound, GameConfig.TOTAL_PAIRS);

                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySound(AudioManager.Sound.PuzzleReveal);
            }

            yield return new WaitForSeconds(GameConfig.MATCH_ANIMATION_TIME);

            isProcessing = false;

            if (MatchesFound >= GameConfig.TOTAL_PAIRS)
            {
                if (RebusPuzzleRenderer.Instance != null)
                    RebusPuzzleRenderer.Instance.RevealAll();

                yield return new WaitForSeconds(1f);
                OnSolvePuzzleClicked();
            }
        }

        private IEnumerator ProcessMismatch()
        {
            yield return new WaitForSeconds(GameConfig.MISMATCH_DISPLAY_TIME);

            Panel p1 = firstSelected;
            Panel p2 = secondSelected;
            firstSelected = null;
            secondSelected = null;

            int flipsRemaining = 2;
            Action onFlipDone = () =>
            {
                flipsRemaining--;
                if (flipsRemaining <= 0)
                {
                    isProcessing = false;
                    // Mismatch = switch turns in 2-player mode
                    SwitchTurn();
                }
            };

            if (p1 != null && !p1.IsMatched) p1.FlipToFront(onFlipDone);
            else { flipsRemaining--; }

            if (p2 != null && !p2.IsMatched) p2.FlipToFront(onFlipDone);
            else { flipsRemaining--; }

            if (flipsRemaining <= 0)
            {
                isProcessing = false;
                SwitchTurn();
            }
        }

        private void OnSolvePuzzleClicked()
        {
            if (CurrentState != GameState.Playing) return;

            SetState(GameState.PuzzleSolve);

            if (UIManager.Instance != null)
                UIManager.Instance.ShowSolvePanel();
        }

        private void OnSolveSubmitted(string guess)
        {
            if (currentPuzzle == null) return;

            if (currentPuzzle.CheckAnswer(guess))
            {
                SetState(GameState.Victory);

                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySound(AudioManager.Sound.Victory);

                if (RebusPuzzleRenderer.Instance != null)
                    RebusPuzzleRenderer.Instance.RevealAll();

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.HideSolvePanel();

                    if (Mode == GameMode.TwoPlayer)
                    {
                        UIManager.Instance.ShowVictory2P(
                            currentPuzzle.answer,
                            CurrentPlayer,
                            PlayerMatches,
                            PlayerAttempts,
                            UIManager.Instance.GetElapsedTime()
                        );
                    }
                    else
                    {
                        UIManager.Instance.ShowVictory(
                            currentPuzzle.answer,
                            MatchesFound,
                            Attempts,
                            UIManager.Instance.GetElapsedTime()
                        );
                    }
                }
            }
            else
            {
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySound(AudioManager.Sound.WrongAnswer);

                // Wrong guess in 2-player: lose your turn
                if (Mode == GameMode.TwoPlayer)
                {
                    UIManager.Instance.HideSolvePanel();
                    SetState(GameState.Playing);
                    SwitchTurn();
                }
            }
        }

        private void OnSolveCancelled()
        {
            if (CurrentState == GameState.PuzzleSolve)
                SetState(GameState.Playing);
        }

        private void OnPlayAgain()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnMainMenu()
        {
            SceneManager.LoadScene("MainMenu");
        }

        private void OnDestroy()
        {
            if (BoardManager.Instance != null)
                BoardManager.Instance.OnPanelClicked -= OnPanelSelected;

            if (UIManager.Instance != null)
            {
                UIManager.Instance.OnSolvePuzzleClicked -= OnSolvePuzzleClicked;
                UIManager.Instance.OnSolveSubmitted -= OnSolveSubmitted;
                UIManager.Instance.OnSolveCancelled -= OnSolveCancelled;
                UIManager.Instance.OnPlayAgainClicked -= OnPlayAgain;
                UIManager.Instance.OnMainMenuClicked -= OnMainMenu;
            }
        }
    }
}
