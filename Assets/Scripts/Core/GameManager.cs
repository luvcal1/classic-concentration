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
        public int MatchesFound { get; private set; }
        public int Attempts { get; private set; }

        public event Action<GameState> OnStateChanged;
        public event Action<int> OnMatchFound;
        public event Action OnMismatch;

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
            MatchesFound = 0;
            Attempts = 0;
            firstSelected = null;
            secondSelected = null;
            isProcessing = false;

            // Pick a random puzzle
            currentPuzzle = PuzzleDatabase.GetRandomPuzzle();

            // Load puzzle into renderer
            if (RebusPuzzleRenderer.Instance != null)
                RebusPuzzleRenderer.Instance.LoadPuzzle(currentPuzzle);

            // Setup board
            if (BoardManager.Instance != null)
                BoardManager.Instance.OnPanelClicked += OnPanelSelected;

            // Setup UI
            if (UIManager.Instance != null)
            {
                UIManager.Instance.ResetUI();
                UIManager.Instance.StartTimer();
                UIManager.Instance.OnSolvePuzzleClicked += OnSolvePuzzleClicked;
                UIManager.Instance.OnSolveSubmitted += OnSolveSubmitted;
                UIManager.Instance.OnSolveCancelled += OnSolveCancelled;
                UIManager.Instance.OnPlayAgainClicked += OnPlayAgain;
                UIManager.Instance.OnMainMenuClicked += OnMainMenu;
            }

            SetState(GameState.Playing);
        }

        private void SetState(GameState newState)
        {
            CurrentState = newState;
            OnStateChanged?.Invoke(newState);
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

            if (firstSelected.PrizeName == secondSelected.PrizeName)
            {
                // Match found!
                MatchesFound++;

                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySound(AudioManager.Sound.Match);

                OnMatchFound?.Invoke(MatchesFound);

                if (UIManager.Instance != null)
                    UIManager.Instance.UpdateScore(MatchesFound, Attempts);

                Panel p1 = firstSelected;
                Panel p2 = secondSelected;
                firstSelected = null;
                secondSelected = null;

                StartCoroutine(ProcessMatch(p1, p2));
            }
            else
            {
                // Mismatch
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySound(AudioManager.Sound.Mismatch);

                OnMismatch?.Invoke();

                if (UIManager.Instance != null)
                    UIManager.Instance.UpdateScore(MatchesFound, Attempts);

                StartCoroutine(ProcessMismatch());
            }
        }

        private IEnumerator ProcessMatch(Panel p1, Panel p2)
        {
            yield return new WaitForSeconds(0.3f);

            // Remove matched panels
            p1.MatchAndRemove();
            p2.MatchAndRemove();

            // Reveal puzzle portion
            if (RebusPuzzleRenderer.Instance != null)
            {
                RebusPuzzleRenderer.Instance.UpdateVisibility(MatchesFound, GameConfig.TOTAL_PAIRS);

                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySound(AudioManager.Sound.PuzzleReveal);
            }

            yield return new WaitForSeconds(GameConfig.MATCH_ANIMATION_TIME);

            isProcessing = false;

            // Check if all pairs matched
            if (MatchesFound >= GameConfig.TOTAL_PAIRS)
            {
                // All panels cleared - reveal full puzzle, give player a chance to solve
                if (RebusPuzzleRenderer.Instance != null)
                    RebusPuzzleRenderer.Instance.RevealAll();

                // Auto-prompt solve
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
                    isProcessing = false;
            };

            if (p1 != null && !p1.IsMatched) p1.FlipToFront(onFlipDone);
            else { flipsRemaining--; }

            if (p2 != null && !p2.IsMatched) p2.FlipToFront(onFlipDone);
            else { flipsRemaining--; }

            if (flipsRemaining <= 0)
                isProcessing = false;
        }

        private void OnSolvePuzzleClicked()
        {
            if (CurrentState != GameState.Playing) return;

            SetState(GameState.PuzzleSolve);

            if (UIManager.Instance != null)
            {
                string preview = RebusPuzzleRenderer.Instance != null
                    ? RebusPuzzleRenderer.Instance.GetVisiblePuzzleText()
                    : "???";
                UIManager.Instance.ShowSolvePanel();
            }
        }

        private void OnSolveSubmitted(string guess)
        {
            if (currentPuzzle == null) return;

            if (currentPuzzle.CheckAnswer(guess))
            {
                // Victory!
                SetState(GameState.Victory);

                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySound(AudioManager.Sound.Victory);

                if (RebusPuzzleRenderer.Instance != null)
                    RebusPuzzleRenderer.Instance.RevealAll();

                if (UIManager.Instance != null)
                {
                    UIManager.Instance.HideSolvePanel();
                    UIManager.Instance.ShowVictory(
                        currentPuzzle.answer,
                        MatchesFound,
                        Attempts,
                        UIManager.Instance.GetElapsedTime()
                    );
                }
            }
            else
            {
                // Wrong answer
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlaySound(AudioManager.Sound.WrongAnswer);

                // Stay in solve state, let player try again or cancel
            }
        }

        private void OnSolveCancelled()
        {
            if (CurrentState == GameState.PuzzleSolve)
            {
                SetState(GameState.Playing);
            }
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
