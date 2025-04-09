using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GamePhase
{
    ChooseCase,
    Ranking,
    Reveal1,
    Offer1,
    Reveal2,
    Offer2,
    Reveal3,
    Offer3,
    Reveal4,
    Switch,
    FinalReveal
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public string plexToken = "YOUR_PLEX_TOKEN";
    public string plexIp = "SERVER_IP";
    public int plexPort = 32400;
    public string playlistName = "PLAYLIST_NAME";
    public List<MovieInfo> loadedMovies;

    [SerializeField] RankingUIManager RankingUiManager;

    private List<MovieInfo> availableMovies;
    public RankingSystem rankingSystem;
    private MovieInfo currentMovie;

    [SerializeField] CaseUIManager caseUiManager;

    private List<MovieInfo> shuffledCases;
    private MovieInfo chosenCase;
    private MovieInfo revealedCase;
    private bool playerHasChosenCase = false;

    public MovieOfferUIManager movieOfferUi;
    private MovieInfo movieOffer;

    private GamePhase currentPhase = GamePhase.ChooseCase;
    private int casesToRevealThisRound = 0;
    private int revealedThisRound = 0;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    async void Start()
    {
        PlexPlaylistFetcher fetcher = new PlexPlaylistFetcher(plexToken, plexIp, plexPort, playlistName);
        var progress = new Progress<float>(p => LoadingUIManager.Instance.UpdateProgress(p));
        System.Action<string> status = msg => LoadingUIManager.Instance.UpdateStatus(msg);

        RankingUiManager.HideMovieInfo();
        LoadingUIManager.Instance.Show();

        if (fetcher.CacheExists())
        {
            loadedMovies = await fetcher.LoadFromCache(progress, status);
            Debug.Log($"{loadedMovies.Count} movies loaded from cache.");
        }
        else
        {
            loadedMovies = await fetcher.FetchFromPlex(true,progress, status);
            Debug.Log($"{loadedMovies?.Count} movies loaded from Plex.");
        }

        LoadingUIManager.Instance.Hide();

        if (loadedMovies == null || loadedMovies.Count == 0)
        {
            Debug.LogError("No Movies loaded from playlist");
            return;
        }

        if (loadedMovies.Count >= 12)
        {
            availableMovies = loadedMovies
                .OrderBy(_ => UnityEngine.Random.value)
                .Take(12)
                .ToList();
        }
        else
        {
            Debug.LogError("Not enough movies to play. Need at least 12.");
        }

        shuffledCases = availableMovies.OrderBy(_ => UnityEngine.Random.value).ToList();

        caseUiManager.OnCaseSelected += HandlePlayerCaseSelection;

        HandlePhaseStart();
    }

    private void HandlePlayerCaseSelection(int index)
    {
        if (!playerHasChosenCase) //Player is choosing their case
        {
            caseUiManager.Hide();
            playerHasChosenCase = true;
            chosenCase = shuffledCases[index];
            Debug.Log($"Player chosen case {index + 1} (movie hidden): {chosenCase.title}");

            currentPhase = GamePhase.Ranking;
            HandlePhaseStart();
        }
        else if (IsInRevealRound())
        {
            revealedCase = shuffledCases[index];
            RankingUiManager.CrossOutByTitle(revealedCase.title);
            revealedThisRound++;
            Debug.Log($"Revealed case {index + 1}: {revealedCase.title}");

            if (revealedThisRound >= casesToRevealThisRound)
            {
                AdvancePhase();
            }
            else
            {
                ShowNextMovie();
            }
        }
    }

    private void OnRankButtonClicked(int rank)
    {
        if (rankingSystem.AssignRank(currentMovie, rank))
        {
            RankingUiManager.SetRankLabel(rank, currentMovie.title);
            ShowNextMovie();
        }
        else
        {
            RankingUiManager.ShowError($"Rank {rank} is already used!");
        }
    }

    public void ShowNextMovie()
    {
        currentMovie = rankingSystem.GetNextMovie();
        if(currentMovie == null)
        {
            RankingUiManager.HideMovieInfo();
            RankingUiManager.HideRankButtons();
            RankingUiManager.ShowResults(rankingSystem.GetRankedResults());
            AdvancePhase();
        }
        else
        {

            RankingUiManager.DisplayMovie(currentMovie);
        }


    }

    public bool HasPlayerChosenCase()
    {
        return playerHasChosenCase;
    }

    public MovieInfo GetRevealedMovieInfo()
    {
        return revealedCase;
    }

    public void ShowMovieOffer()
    {
        movieOffer = loadedMovies[UnityEngine.Random.Range(0, loadedMovies.Count)];
        movieOfferUi.ShowOffer(
            $"The Banker offers you this movie!",
            movieOffer,
            "Decline",
            "Accept",
            (bool accepted) =>
            {
                if (accepted)
                    Debug.Log("Player Accepted Bankers Offer");
                //End Game Logic
                else
                    AdvancePhase();
            }); 
    }

    public MovieInfo GetMovieOfferInfo()
    {
        if (movieOffer != null)
            return movieOffer;
        else return null;
    }

    private void AdvancePhase()
    {
        currentPhase++;
        Debug.Log($"Game Phase advanced to: {currentPhase}");
        HandlePhaseStart();
    }

    private void HandlePhaseStart()
    {
        switch (currentPhase)
        {
            case GamePhase.ChooseCase:
                caseUiManager.Show();
                break;
            case GamePhase.Ranking:
                BeginRanking();
                break;
            case GamePhase.Reveal1:
                StartRevealRound(4);
                break;
            case GamePhase.Offer1:
                ShowMovieOffer();
                break;
            case GamePhase.Reveal2:
                StartRevealRound(3);
                break;
            case GamePhase.Offer2:
                ShowMovieOffer();
                break;
            case GamePhase.Reveal3:
                StartRevealRound(2);
                break;
            case GamePhase.Offer3:
                ShowMovieOffer();
                break;
            case GamePhase.Reveal4:
                StartRevealRound(1);
                break;
            case GamePhase.Switch:
                ShowSwitchChoice();
                break;
            case GamePhase.FinalReveal:
                RevealPlayerCase();
                break;
        }
    }

    private bool IsInRevealRound()
    {
        return currentPhase == GamePhase.Reveal1 ||
            currentPhase == GamePhase.Reveal2 ||
            currentPhase == GamePhase.Reveal3 ||
            currentPhase == GamePhase.Reveal4;
    }

    private void StartRevealRound(int count)
    {
        revealedThisRound = 0;
        casesToRevealThisRound = count;
        Debug.Log($"Reveal round started. Reveal {count} cases.");
        caseUiManager.Show();
    }

    private void BeginRanking()
    {
        rankingSystem = new RankingSystem(availableMovies);
        RankingUiManager.ShowMovieInfo();
        RankingUiManager.ShowRankButtons();

        for (int i = 0; i < RankingUiManager.rankButtons.Length; i++)
        {
            int rank = i + 1;
            RankingUiManager.rankButtons[i].onClick.RemoveAllListeners();
            RankingUiManager.rankButtons[i].onClick.AddListener(() => OnRankButtonClicked(rank));
        }

        ShowNextMovie();
    }

    private void ShowSwitchChoice()
    {
        int remainingIndex = Enumerable.Range(0, shuffledCases.Count)
            .Except(new[] { shuffledCases.IndexOf(chosenCase) }) // Player’s original case
            .Except(rankingSystem.rankings.Values.Select(m => shuffledCases.IndexOf(m))) // Already revealed
            .FirstOrDefault(); // Only one left should remain

        movieOfferUi.ShowOffer(
            $"Would you like to keep your original case or switch with the Case {remainingIndex + 1}?",
            null,
            "Keep",
            "Switch",
            (bool switchIt) =>
            {
                if (switchIt)
                    chosenCase = shuffledCases[remainingIndex];

                AdvancePhase();
            });
    }

    private void RevealPlayerCase()
    {
        RankingUiManager.RevealFinalMovie(chosenCase);
        Debug.Log($"Final reveal: The movie in your case was {chosenCase.title}");

        // Optional: Cross out final result or compare to player's rankings
    }
}
