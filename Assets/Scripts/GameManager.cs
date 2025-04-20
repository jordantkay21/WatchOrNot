using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public enum GamePhase
{
    ChooseCase,
    Ranking,
    RankAdjustment,
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

    [SerializeField] RankingUIManager rankingUiManager;

    private List<MovieInfo> availableMovies;
    public RankingSystem rankingSystem;
    private MovieInfo currentMovie;

    [SerializeField] CaseUIManager caseUiManager;

    private List<MovieInfo> shuffledCases;
    private MovieInfo chosenCase;
    private MovieInfo revealedCase;
    private bool playerHasChosenCase = false;

    private MovieInfo movieOffer;

    public GamePhase currentPhase = GamePhase.ChooseCase;
    private int casesToRevealThisRound = 0;
    private int revealedThisRound = 0;
    private List<int> revealedCaseIndices = new();

    private int swapCount = 0;

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
        var progress = new Progress<float>(p => LoadingUIController.Instance.UpdateProgress(p));
        System.Action<string> status = msg => LoadingUIController.Instance.UpdateStatus(msg);

        rankingUiManager.HideMovieInfo();
        LoadingUIController.Instance.Show();

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

        LoadingUIController.Instance.Hide();

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
            rankingUiManager.CrossOutByTitle(revealedCase.title);

            if (!revealedCaseIndices.Contains(index))
                revealedCaseIndices.Add(index);

            revealedThisRound++;

            RoundStatusUIManager.Instance.UpdateProgress(revealedThisRound, casesToRevealThisRound);
            Debug.Log($"Revealed case {index + 1}: {revealedCase.title}");

            Debug.Log($"[Reveal] Phase: {currentPhase}, RevealedThisRound: {revealedThisRound} / {casesToRevealThisRound}");

            if (revealedThisRound >= casesToRevealThisRound)
            {
                AdvancePhase();
            }

        }
    }

    private void OnRankButtonClicked(int rank)
    {
        if (rankingSystem.AssignRank(currentMovie, rank))
        {
            rankingUiManager.SetRankLabel(rank, currentMovie.title);
            ShowNextMovie();
        }
        else
        {
            rankingUiManager.ShowError($"Rank {rank} is already used!");
        }
    }

    public void ShowNextMovie()
    {
        currentMovie = rankingSystem.GetNextMovie();
        if(currentMovie == null)
        {
            rankingUiManager.HideMovieInfo();
            rankingUiManager.HideRankButtons();
            rankingUiManager.ShowResults(rankingSystem.GetRankedResults());
            AdvancePhase();
        }
        else
        {

            rankingUiManager.DisplayMovie(currentMovie);
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

    private async void ShowMovieOffer()
    {
        caseUiManager.SetCaseButtonsInteractable(false);

        //Show suspense message first
        RoundStatusUIManager.Instance.UpdateRoundStatus(
            $"{currentPhase}",
            $"The Banker is considering an offer...",
            ""
            );

        //Wait 3 seconds
        await Task.Delay(3000);

        //Now show the actual offer
        RoundStatusUIManager.Instance.Hide();

        movieOffer = loadedMovies[UnityEngine.Random.Range(0, loadedMovies.Count)];

        PopUpUI.Instance.ShowPopup(
            $"The Banker offers you this movie!",
            "Accept",
            () =>
            {
                Debug.Log("Player Accepted Bankers Offer");
                ShowFinalMovie(movieOffer);
            },
            "Decline",
            () =>
            {
                AdvancePhase();
            },
            movieOffer);
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
                RoundStatusUIManager.Instance.UpdateRoundStatus(
                    $"{currentPhase}",
                    $"Choose your lucky case!",
                    ""
                );
                break;
            case GamePhase.Ranking:
                BeginRanking();
                break;
            case GamePhase.RankAdjustment:
                BeginRankAdjustment();
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
                ShowFinalMovie(chosenCase);
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

        Debug.Log($"[Reveal Start] Phase: {currentPhase}, CasesToReveal: {casesToRevealThisRound}");

        RoundStatusUIManager.Instance.UpdateRoundStatus(
            $"Reveal Round {GetRevealRoundIndex()}",
            $"Pick {casesToRevealThisRound} cases to reveal.",
            $"Revealed: 0/{casesToRevealThisRound}"
            );

        caseUiManager.Show();

        int chosenIndex = shuffledCases.IndexOf(chosenCase);
        caseUiManager.SetCaseButtonsForReveal(revealedCaseIndices, chosenIndex);
    }

    private void BeginRanking()
    {
        RoundStatusUIManager.Instance.UpdateRoundStatus(
    $"{currentPhase}",
    $"Rank the movies from 1 (Want to watch the most) to 12 (want to watch the least)",
    ""
        );

        rankingSystem = new RankingSystem(availableMovies);
        rankingUiManager.ShowMovieInfo();
        rankingUiManager.ShowRankButtons();

        for (int i = 0; i < rankingUiManager.rankButtons.Length; i++)
        {
            int rank = i + 1;
            rankingUiManager.rankButtons[i].onClick.RemoveAllListeners();
            rankingUiManager.rankButtons[i].onClick.AddListener(() => OnRankButtonClicked(rank));
        }

        ShowNextMovie();
    }

    private void ShowSwitchChoice()
    {
        int chosenIndex = shuffledCases.IndexOf(chosenCase);

        var remainingIndex = Enumerable.Range(0, shuffledCases.Count)
            .Except(new[] { chosenIndex })
            .Except(revealedCaseIndices)
            .ToList();

        if (remainingIndex.Count != 1)
        {
            Debug.LogError($"Expected exactly one unrevealed case, found: {remainingIndex.Count}");
            return;
        }

        int finalIndex = remainingIndex[0];
        var finalCase = shuffledCases[finalIndex];

        PopUpUI.Instance.ShowPopup(
            $"Would you like to keep your original case or switch with the Case {finalIndex + 1}?",
            "Keep",
            () =>
            {
                AdvancePhase();
            },
            "Switch",
            () =>
            {
                chosenCase = finalCase;
                AdvancePhase();
            });
    }

    private void ShowFinalMovie(MovieInfo finalMovie)
    {
        var ranked = rankingSystem.GetRankedResults();
        int rank = ranked.FindIndex(m => m.movie.title == finalMovie.title);

        FinalRevealPanel.Instance.ShowFinalMovie(finalMovie, rank);
    }

    private int GetRevealRoundIndex()
    {
        return currentPhase switch
        {
            GamePhase.Reveal1 => 1,
            GamePhase.Reveal2 => 2,
            GamePhase.Reveal3 => 3,
            GamePhase.Reveal4 => 4,
            _ => 0
        };
    }

    private void BeginRankAdjustment()
    {
        RoundStatusUIManager.Instance.UpdateRoundStatus(
            $"Adjust Rankings",
            $"You have 3 chances to swap any two ranked movies.",
            "Tap two entries to swap them.");

        rankingUiManager.EnableSwappingUI();
    }

    public void OnRankSwapped()
    {
        swapCount++;
        if (swapCount >= 3)
        {
            FinalizeRanking();
        }
            
    }

    public void FinalizeRanking()
   {
        rankingUiManager.DisableAllSwapButtons();
        AdvancePhase();
    }
}
