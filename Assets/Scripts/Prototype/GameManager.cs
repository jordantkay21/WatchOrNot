using System.Collections.Generic;
using UnityEngine;
using System;

namespace KayosMedia.WatchOrNot.Prototype
{
    [System.Serializable]
    public enum GamePhase
    {
        GameConfig,
        MovieSelection,
        Ranking,
        FinalAdjustments,
        RevealGameplay
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public static event Action OnRankingPhaseStarted;
        public static event Action OnFinalAdjustmentPhaseStarted;
        public static event Action OnRevealStageStarted;
        public static Action<MovieInfo> OnMovieRevealed;
        public static Action OnSessionCleared;



        public GamePhase CurrentPhase = GamePhase.GameConfig;

        //List of the 12 randomly selected movies
        public List<MovieInfo> SelectedMovies { get; private set; } = new();

        //Player's assigned rankings (Rank -> Movie)
        public Dictionary<int, MovieInfo> FinalRankings { get; private set; } = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }

        public void SetPhase(GamePhase newPhase)
        {
            CurrentPhase = newPhase;
            Debug.Log($"[SessionManager][SetPhase] Phase changed to {newPhase}");
        }

        public void SelectRandomMovies(List<MovieInfo> sourceMovies, int count = 12)
        {
            SelectedMovies.Clear();
            FinalRankings.Clear();

            Debug.Log($"[SessionManager][SelectRandomMovies] Recieved {sourceMovies.Count} source movies");

            var shuffled = new List<MovieInfo>(sourceMovies);



            //Fisher-Yates Shuffle
            for (int i = 0; i < shuffled.Count; i++)
            {
                var temp = shuffled[i];
                int randomIndex = UnityEngine.Random.Range(i, shuffled.Count);
                shuffled[i] = shuffled[randomIndex];
                shuffled[randomIndex] = temp;
            }

            Debug.Log($"[SessionManager][SelectRandomMovies] Shuffled {shuffled.Count} source movies");

            for (int i = 0; i < Mathf.Min(count, shuffled.Count); i++)
            {
                SelectedMovies.Add(shuffled[i]);
                Debug.Log($"[SessionManager][SelectRandomMovies] {shuffled[i].title} has been randomly selected for this round!");
            }

            Debug.Log($"[SessionManager][SelectRandomMovies] Selected {SelectedMovies.Count} random movies");
        }

        public void StartRanking()
        {
            if (SelectedMovies.Count != 12)
            {
                Debug.LogError($"[SessionManager][StartRanking] Cannot start ranking - wrong number of selected movies!");
                return;
            }

            SetPhase(GamePhase.Ranking);
            Debug.Log($"[SessionManager][StartRanking] Ranking Phase Started!");

            //Fire Event
            OnRankingPhaseStarted?.Invoke();
        }

        public void AssignRank(int rank, MovieInfo movie)
        {
            if (FinalRankings.ContainsKey(rank))
            {
                Debug.LogWarning($"[SessionManager][AssignRank] Rank {rank} already assigned!");
                return;
            }

            FinalRankings[rank] = movie;
            Debug.Log($"[SessionManager][AssignRank] Movie '{movie.title}' assigned to Rank {rank}");
        }

        public bool IsRankingComplete()
        {
            return FinalRankings.Count == 12;
        }

        public void CompleteRanking()
        {
            if (!IsRankingComplete())
            {
                Debug.LogWarning($"[SessionManager][CompleteRanking] Cannot complete ranking - not all ranks assigned!");
                return;
            }

            SetPhase(GamePhase.RevealGameplay);
            Debug.Log($"[SessionManager][CompleteRanking] Ranking complete! Ready for reveal stage.");
        }

        public void StartFinalAdjustments()
        {
            if (!IsRankingComplete())
            {
                Debug.LogWarning("[SessionManager][StartFinalAdjustments] Cannot begin Final Adjustments - ranking incomplete");
                return;
            }

            SetPhase(GamePhase.FinalAdjustments);
            Debug.Log("[SessionManager][StartFinalAdjustments] Final Adjustments phase started!");
            OnFinalAdjustmentPhaseStarted?.Invoke();
        }

        public void StartRevealStage()
        {
            SetPhase(GamePhase.RevealGameplay);
            Debug.Log("[SessionManager][StartRevealStage] Reveal Stage started!");
            OnRevealStageStarted?.Invoke();
        }
    }
}