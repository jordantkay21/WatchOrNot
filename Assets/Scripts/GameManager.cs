using System.Collections.Generic;
using UnityEngine;
using System;

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


    public GamePhase CurrentPhase = GamePhase.GameConfig;

    //List of the 12 randomly selected movies
    public List<MovieInfo> SelectedMovies { get; private set; } = new();

    //Player's assigned rankings (Rank -> Movie)
    public Dictionary<int, MovieInfo> FinalRankings { get; private set; } = new();

    private void Awake()
    {
        if(Instance != null && Instance != this)
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
        Debug.Log($"[GameManager][SetPhase] Phase changed to {newPhase}");
    }

    public void SelectRandomMovies(List<MovieInfo> sourceMovies, int count = 12)
    {
        SelectedMovies.Clear();
        FinalRankings.Clear();

        Debug.Log($"[GameManager][SelectRandomMovies] Recieved {sourceMovies.Count} source movies");

        var shuffled = new List<MovieInfo>(sourceMovies);



        //Fisher-Yates Shuffle
        for (int i = 0; i <shuffled.Count; i++)
        {
            var temp = shuffled[i];
            int randomIndex = UnityEngine.Random.Range(i, shuffled.Count);
            shuffled[i] = shuffled[randomIndex];
            shuffled[randomIndex] = temp;
        }

        Debug.Log($"[GameManager][SelectRandomMovies] Shuffled {shuffled.Count} source movies");

        for (int i = 0; i < Mathf.Min(count, shuffled.Count); i++)
        {
            SelectedMovies.Add(shuffled[i]);
            Debug.Log($"[GameManager][SelectRandomMovies] {shuffled[i].title} has been randomly selected for this round!");
        }

        Debug.Log($"[GameManager][SelectRandomMovies] Selected {SelectedMovies.Count} random movies");
    }

    public void StartRanking()
    {
        if(SelectedMovies.Count != 12)
        {
            Debug.LogError($"[GameManager][StartRanking] Cannot start ranking - wrong number of selected movies!");
            return;
        }

        SetPhase(GamePhase.Ranking);
        Debug.Log($"[GameManager][StartRanking] Ranking Phase Started!");

        //Fire Event
        OnRankingPhaseStarted?.Invoke();
    }

    public void AssignRank(int rank, MovieInfo movie)
    {
        if (FinalRankings.ContainsKey(rank))
        {
            Debug.LogWarning($"[GameManager][AssignRank] Rank {rank} already assigned!");
            return;
        }

        FinalRankings[rank] = movie;
        Debug.Log($"[GameManager][AssignRank] Movie '{movie.title}' assigned to Rank {rank}");
    }

    public bool IsRankingComplete()
    {
        return FinalRankings.Count == 12;
    }

    public void CompleteRanking()
    {
        if (!IsRankingComplete())
        {
            Debug.LogWarning($"[GameManager][CompleteRanking] Cannot complete ranking - not all ranks assigned!");
            return;
        }

        SetPhase(GamePhase.RevealGameplay);
        Debug.Log($"[GameManager][CompleteRanking] Ranking complete! Ready for reveal stage.");
    }

    public void StartFinalAdjustments()
    {
        if (!IsRankingComplete())
        {
            Debug.LogWarning("[GameManager][StartFinalAdjustments] Cannot begin Final Adjustments - ranking incomplete");
            return;
        }

        SetPhase(GamePhase.FinalAdjustments);
        Debug.Log("[GameManager][StartFinalAdjustments] Final Adjustments phase started!");
        OnFinalAdjustmentPhaseStarted?.Invoke();
    }

    public void StartRevealStage()
    {
        SetPhase(GamePhase.RevealGameplay);
        Debug.Log("[GameManager][StartRevealStage] Reveal Stage started!");
        OnRevealStageStarted?.Invoke();
    }
}
