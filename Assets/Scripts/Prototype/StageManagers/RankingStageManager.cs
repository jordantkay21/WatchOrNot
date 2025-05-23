using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace KayosMedia.WatchOrNot.Prototype
{
    public class RankingStageManager : MonoBehaviour
    {
        private enum RankingMode
        {
            InitialRanking,
            FinalAdjustments
        }

        private RankingMode currentMode = RankingMode.InitialRanking;
        private int remainingAdjustments = 3;
        private int? firstSelectedRank = null;

        [Header("Panels")]
        public GameObject rankingStagePanel;
        public GameObject movieInfoPanel;
        public GameObject instructionPanel;

        [Header("Ranking Panel")]
        public List<Button> rankButtons;
        public List<TMP_Text> rankButtonTexts;
        public List<Button> infoButtons;

        [Header("Other")]
        public TMP_Text messageText;
        public Button blindSwapButton;
        public Button finalizeRankButton;

        private List<MovieInfo> moviesToRank;
        private Dictionary<int, MovieInfo> assignedRanks = new();
        private int currentMovieIndex = 0;

        private void Start()
        {
            rankingStagePanel.SetActive(false);
            instructionPanel.SetActive(false);
            finalizeRankButton.gameObject.SetActive(false);
            blindSwapButton.gameObject.SetActive(false);

            blindSwapButton.onClick.AddListener(OnBlindSwapClicked);
            finalizeRankButton.onClick.AddListener(FinishFinalAdjustments);
        }

        private void OnEnable()
        {
            GameManager.OnRankingPhaseStarted += BeginRanking;
            GameManager.OnFinalAdjustmentPhaseStarted += StartFinalAdjustments;
            GameManager.OnMovieRevealed += CrossOutRankForMovie;
        }

        private void OnDisable()
        {
            GameManager.OnRankingPhaseStarted -= BeginRanking;
        }

        public void BeginRanking()
        {
            moviesToRank = new List<MovieInfo>(GameManager.Instance.SelectedMovies);
            assignedRanks.Clear();
            currentMovieIndex = 0;

            SetupRankButtons();
            ShowCurrentMovie();
            rankingStagePanel.SetActive(true);
            movieInfoPanel.SetActive(true);
            GameManager.Instance.SetPhase(GamePhase.Ranking);
        }

        private void SetupRankButtons()
        {
            for (int i = 0; i < rankButtons.Count; i++)
            {
                int rankNumber = i + 1;
                rankButtonTexts[i].text = "Unassigned";
                rankButtons[i].interactable = true;

                //Setup rank selection
                rankButtons[i].onClick.RemoveAllListeners();
                rankButtons[i].onClick.AddListener(() => OnRankButtonClicked(rankNumber));

                //Setup info buttons
                infoButtons[i].onClick.RemoveAllListeners();
                infoButtons[i].onClick.AddListener(() => ShowMovieInfoForRank(rankNumber));

                foreach (Button infoButton in infoButtons)
                    infoButton.gameObject.SetActive(false);
            }
        }

        private void ShowMovieInfoForRank(int rank)
        {
            if (!GameManager.Instance.FinalRankings.ContainsKey(rank))
            {
                Debug.LogWarning($"No movie assigned to rank {rank}");
                return;
            }

            var movie = GameManager.Instance.FinalRankings[rank];



            MovieInfoUIController.Instance.Show(movie);
            instructionPanel.SetActive(false);

            messageText.text = $"Displaying info for rank {rank}: {movie.title}";
            Debug.Log($"[RankingStageManager][ShowMovieInfoForRank] Displaying info for rank {rank}: {movie.title}");
        }

        private void ShowCurrentMovie()
        {
            var movie = moviesToRank[currentMovieIndex];
            MovieInfoUIController.Instance.Show(movie);

            messageText.text = $"Ranking {currentMovieIndex + 1} of {moviesToRank.Count}";
        }

        private void OnRankButtonClicked(int rank)
        {
            if (currentMode == RankingMode.InitialRanking)
                HandleInitialRanking(rank);
            else if (currentMode == RankingMode.FinalAdjustments)
                HandleFinalAdjustments(rank);
        }

        private void HandleInitialRanking(int rank)
        {
            if (assignedRanks.ContainsKey(rank))
            {
                Debug.LogWarning($"Rank {rank} already assigned!");
                return;
            }

            var movie = moviesToRank[currentMovieIndex];

            //Assign the seelcted movie to the rank inside SessionManager
            GameManager.Instance.AssignRank(rank, movie);

            //Update button text and disable
            rankButtonTexts[rank - 1].text = movie.title;
            rankButtons[rank - 1].interactable = false;

            currentMovieIndex++;

            if (currentMovieIndex < moviesToRank.Count)
            {
                ShowCurrentMovie();
            }
            else
            {
                OnRankingComplete();
            }
        }

        private void HandleFinalAdjustments(int rank)
        {
            if (firstSelectedRank == null)
            {
                firstSelectedRank = rank;
                blindSwapButton.gameObject.SetActive(true);
            }
            else
            {
                int secondRank = rank;
                if (firstSelectedRank == secondRank)
                {
                    firstSelectedRank = null;
                    return;
                }

                ConfirmSwap(firstSelectedRank.Value, secondRank);
            }
        }

        private void ConfirmSwap(int a, int b)
        {
            var movieA = GameManager.Instance.FinalRankings[a];
            var movieB = GameManager.Instance.FinalRankings[b];

            string message = $"Swap \"{movieA.title}\" with \"{movieB.title}\"?";

            UIManager.Instance.confirmationPopup.Show(message, () =>
            {
                GameManager.Instance.FinalRankings[a] = movieB;
                GameManager.Instance.FinalRankings[b] = movieA;
                Debug.Log($"[RankingStageManager][ConfirmSwap] Swapped {movieA.title} and {movieB.title}");

                remainingAdjustments--;
                firstSelectedRank = null;
                UpdateRankButton(a);
                UpdateRankButton(b);
                UpdateRemainingAdjustmentsUI();

                if (remainingAdjustments <= 0)
                    FinishFinalAdjustments();
            },
            () =>
            {
                Debug.Log($"[RankingStageManager][ConfirmSwap] Swap canceled");
                firstSelectedRank = null;
            });
        }

        private void OnRankingComplete()
        {
            messageText.text = "Ranking Complete!";
            Debug.Log($"[RankingStageManager][OnRankingComplete] All movies Ranked!");

            GameManager.Instance.StartFinalAdjustments();
        }

        private void StartFinalAdjustments()
        {
            currentMode = RankingMode.FinalAdjustments;
            remainingAdjustments = 3;
            firstSelectedRank = null;

            //Unlock all rank buttons
            foreach (Button rankButton in rankButtons)
                rankButton.interactable = true;

            //Enable all info buttons
            foreach (Button infoButton in infoButtons)
                infoButton.gameObject.SetActive(true);

            finalizeRankButton.gameObject.SetActive(true);
            movieInfoPanel.SetActive(false);
            instructionPanel.SetActive(true);

            messageText.text = $"Remaining Adjustments: {remainingAdjustments}/3";
        }
        public void OnBlindSwapClicked()
        {
            if (currentMode != RankingMode.FinalAdjustments || firstSelectedRank == null)
                return;

            int rank = firstSelectedRank.Value;
            var currentMovie = GameManager.Instance.FinalRankings[rank];

            var fullpool = SessionInfoManager.GetMovies();
            var used = new HashSet<MovieInfo>(GameManager.Instance.FinalRankings.Values);
            var unselected = fullpool.FindAll(m => !used.Contains(m));

            if (unselected.Count == 0)
            {
                Debug.LogWarning("No more movies left to swap!");
                return;
            }

            var replacement = unselected[UnityEngine.Random.Range(0, unselected.Count)];

            ConfirmBlindSwap(rank, replacement);

        }

        private void ConfirmBlindSwap(int rank, MovieInfo replacement)
        {
            var currentMovie = GameManager.Instance.FinalRankings[rank];

            string message = $"Replace \"{currentMovie.title}\" with a random new movie from \"{SessionInfoManager.GetCurrentPlaylist().title}\"?";

            UIManager.Instance.confirmationPopup.Show(message, () =>
            {
                GameManager.Instance.FinalRankings[rank] = replacement;
                Debug.Log($"[RankingStageManager][OnBlindSwapClicked] Replaced {currentMovie.title} with {replacement.title}");

                remainingAdjustments--;
                firstSelectedRank = null;
                UpdateRankButton(rank);
                UpdateRemainingAdjustmentsUI();

                if (remainingAdjustments <= 0)
                    FinishFinalAdjustments();
            },
            () =>
            {
                Debug.Log("Blind swap canceled");
                firstSelectedRank = null;
            });
        }

        private void FinishFinalAdjustments()
        {
            Debug.Log($"[RankingStageManager][FinishFinalAdjustments] Final Adjustments complete. Moving to Reveal phase.");
            GameManager.Instance.SetPhase(GamePhase.RevealGameplay);

            finalizeRankButton.gameObject.SetActive(false);
            blindSwapButton.gameObject.SetActive(false);

            ConvertRankButtonsToTitles();
            foreach (Button infoButton in infoButtons)
                infoButton.gameObject.SetActive(false);

            instructionPanel.SetActive(false);
            movieInfoPanel.SetActive(false);

            messageText.text = $"Final Adjustments Compelete. Reveal Phase COMING SOON!";

            GameManager.Instance.StartRevealStage();
        }


        private void UpdateRemainingAdjustmentsUI()
        {
            messageText.text = $"Remaining Adjustments: {remainingAdjustments}/3";
        }

        private void UpdateRankButton(int rank)
        {
            if (rank < 1 || rank > rankButtonTexts.Count) return;

            var movie = GameManager.Instance.FinalRankings.ContainsKey(rank)
                ? GameManager.Instance.FinalRankings[rank]
                : null;

            rankButtonTexts[rank - 1].text = movie != null ? movie.title : "Unassigned";
        }

        private void ConvertRankButtonsToTitles()
        {
            for (int i = 0; i < rankButtonTexts.Count; i++)
            {
                int rank = i + 1;

                if (GameManager.Instance.FinalRankings.TryGetValue(rank, out MovieInfo movie))
                {
                    rankButtonTexts[i].text = movie.title;
                }

                rankButtons[i].interactable = false;
                var image = rankButtons[i].GetComponent<Image>();
                if (image != null) image.enabled = false;
            }

            Debug.Log($"[RankingStageManager][ConvertRankButtonsToTitles] Rank buttons converted to movie titles");
        }

        private void CrossOutRankForMovie(MovieInfo movie)
        {
            foreach (var kvp in GameManager.Instance.FinalRankings)
            {
                if (kvp.Value == movie)
                {
                    var text = rankButtonTexts[kvp.Key - 1];
                    text.text = $"<color=red><s>{movie.title}</s></color>"; // Strike-through using TMP tag
                    return;
                }
            }
        }
    }
}