using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RevealStageManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject CasePanel;

    [Header("Case Buttons")]
    public List<Button> caseButtons;
    public List<TMP_Text> caseTexts;
    public List<RawImage> caseImages;

    [Header("Other")]
    public Color selectedOutlineColor;

    private Dictionary<int, MovieInfo> caseAssignments = new();

    private bool hasPlayerChosenCase = false;
    private int? playerChosenCaseNumber = null;

    private MovieInfo currentOffer;

    private HashSet<int> revealedCaseNumbers = new();
    private int currentRevealRound = 1;
    private int revealsRemainingThisRound = 4;

    private bool isInSwitchPhase = false;
    private int? remainingCaseNumber = null;

    private void Start()
    {
        CasePanel.SetActive(false);

        GameManager.OnRevealStageStarted += BeginRevealPhase;
    }

    public void BeginRevealPhase()
    {
        AssignCases();

        InitilizeCaseButtons();

        CasePanel.SetActive(true);
    }
    public MovieInfo GetPlayerCaseMovie()
    {
        return playerChosenCaseNumber != null ? caseAssignments[(int)playerChosenCaseNumber] : null;
    }

    public MovieInfo GetMovieForCase(int caseNumber)
    {
        return caseAssignments.TryGetValue(caseNumber, out var movie) ? movie : null;
    }

    private void AssignCases()
    {
        caseAssignments.Clear();

        //Get the ranked movies
        List<MovieInfo> rankedMovies = new List<MovieInfo>(GameManager.Instance.FinalRankings.Values);

        //Shuffle the ranked movies
        for (int i = 0; i < rankedMovies.Count; i++)
        {
            int randomIndex = UnityEngine.Random.Range(i, rankedMovies.Count);
            (rankedMovies[i], rankedMovies[randomIndex]) = (rankedMovies[randomIndex], rankedMovies[i]);
        }

        //Assign each shuffled movie to a case
        for (int i = 0; i < caseButtons.Count && i < rankedMovies.Count; i++)
        {
            int caseNumber = i + 1;
            caseAssignments[caseNumber] = rankedMovies[i];

            Debug.Log($"[RevealStageManager][AssignCases] \"{rankedMovies[i].title}\" has been randomly assigned to case {caseNumber}");
        }
    }
    private void InitilizeCaseButtons()
    {
        for (int i = 0; i < caseButtons.Count; i++)
        {
            int caseNumber = i + 1;

            if (caseTexts[i] != null)
                caseTexts[i].text = caseNumber.ToString();

            //Setup click listener
            caseButtons[i].onClick.RemoveAllListeners();
            caseButtons[i].onClick.AddListener(() => OnCaseSelected(caseNumber));

        }

        Debug.Log("[RevealPhaseManager][InitilizeCaseButtons] Case buttons initialized for selection.");
    }

    private void OnCaseSelected(int caseNumber)
    {
        if (playerChosenCaseNumber == null)
        {
            playerChosenCaseNumber = caseNumber;

            var chosenButton = caseButtons[caseNumber - 1];
            caseTexts[caseNumber - 1].text = "CHOSEN CASE";
            caseTexts[caseNumber - 1].enableAutoSizing = true;

            var hoverLogic = chosenButton.GetComponent<ButtonHoverOutline>();

            if (hoverLogic != null)
            {
                hoverLogic.SetOriginalOutlineColor(selectedOutlineColor);
            }

            var outline = chosenButton.GetComponent<Outline>();

            if (outline != null)
            {
                outline.effectColor = selectedOutlineColor;
            }
         
            Debug.Log($"[RevealStageManager][OnCaseSelected] Player selected case # {playerChosenCaseNumber} with movie: {caseAssignments[caseNumber].title}");

            StartRevealRound(1);
        }

        if (!isInSwitchPhase)
        {
            if (revealsRemainingThisRound <= 0 || revealedCaseNumbers.Contains(caseNumber) || caseNumber == playerChosenCaseNumber)
                return;

            GameManager.OnMovieRevealed?.Invoke(caseAssignments[caseNumber]);
            ShowRevealedPoster(caseNumber);

            revealedCaseNumbers.Add(caseNumber);
            revealsRemainingThisRound--;

            Debug.Log($"[RevealStageManager][OnCaseSelected] Player has revealed case {caseNumber} and has revealed movie {caseAssignments[caseNumber].title}");

            int totalCases = caseAssignments.Count;
            int revealedCount = revealedCaseNumbers.Count;

            if (revealedCount == totalCases - 2) //Only 2 unrevealed: player + 1
            {
                isInSwitchPhase = true;
                remainingCaseNumber = GetRemainingCaseNumber();

                Debug.Log($"[RevealStageManager][OnCaseSelected] Entering switch phase. Remaining case: #{remainingCaseNumber}");

                StartCoroutine(ShowSwitchPromptAfterDelay(1.5f));
                return;
            }

            if (revealsRemainingThisRound <= 0)
            {
                Debug.Log($"[RevealStageManager][OnCaseSelected] Reveal round completed. Preparing offer...");

                StartCoroutine(ShowOfferAfterDelay(2f));
            }
        }
    }

    private int GetRemainingCaseNumber()
    {
        foreach (var caseNum in caseAssignments.Keys)
        {
            if (!revealedCaseNumbers.Contains(caseNum) && caseNum != playerChosenCaseNumber)
                return caseNum;
        }

        return -1; // Should never happen
    }

    private IEnumerator ShowSwitchPromptAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        

        string chosenTitle = caseAssignments[playerChosenCaseNumber.Value].title;
        string otherTitle = caseAssignments[remainingCaseNumber.Value].title;

        UIManager.Instance.confirmationPopup.Show(
            $"Would you like to switch your case?\n\nCurrent Case: \"{playerChosenCaseNumber}\"\nOther Case: \"{remainingCaseNumber}\"",
            OnSwitchConfirmed,
            OnSwitchDeclined
        );
    }

    private void OnSwitchConfirmed()
    {
        Debug.Log("[RevealStageManager] Player chose to switch cases.");

        int original = playerChosenCaseNumber.Value;
        playerChosenCaseNumber = remainingCaseNumber;

        remainingCaseNumber = original;

        RevealFinalMovie(caseAssignments[playerChosenCaseNumber.Value]);
    }

    private void OnSwitchDeclined()
    {
        Debug.Log("[RevealStageManager] Player kept original case.");

        RevealFinalMovie(caseAssignments[playerChosenCaseNumber.Value]);
    }

    private void StartRevealRound(int roundNumber)
    {
        currentRevealRound = roundNumber;

        revealsRemainingThisRound = roundNumber switch
        {
            1 => 4,
            2 => 3,
            3 => 2,
            4 => 1,
            _ => 0
        };

        Debug.Log($"[RevealStageManager][StartRevealRound] Reveal Round {roundNumber} started. Player must reveal {revealsRemainingThisRound} cases.");


    }

    private void ShowRevealedPoster(int caseNumber)
    {
        Button button = caseButtons[caseNumber - 1];
        button.interactable = false;

        caseTexts[caseNumber - 1].gameObject.SetActive(false);
        caseImages[caseNumber - 1].texture = caseAssignments[caseNumber].posterTexture;
        caseImages[caseNumber - 1].gameObject.SetActive(true);
    }

    private IEnumerator ShowOfferAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CasePanel.SetActive(false);
        ShowOffer();
    }
    private void ShowOffer()
    {
        List<MovieInfo> movies = SessionInfoManager.GetMovies();

        currentOffer = movies[UnityEngine.Random.Range(0,movies.Count)];

        Debug.Log($"[RevealStageManager][ShowOffer] Player has been offered: {currentOffer.title}");

        MovieInfoUIController.Instance.Show(currentOffer, DisplayType.Offer, OnAcceptOffer, OnDeclineOffer);
    }

    private void OnAcceptOffer()
    {
        Debug.Log($"[RevealStageManager][OnAcceptOffer] Player Accepted the offer: {currentOffer.title}");

        RevealFinalMovie(currentOffer);
    }

    private void OnDeclineOffer()
    {
        Debug.Log($"[RevealStageManager][OnDeclineOffer] Player Declined the offer: {currentOffer.title}");

        CasePanel.SetActive(true);
        StartRevealRound(currentRevealRound + 1);
    }

    private void RevealFinalMovie(MovieInfo movie)
    {
        CasePanel.SetActive(false);
        MovieInfoUIController.Instance.Show(movie, DisplayType.Final);
        Debug.Log($"[EndGame] Game over. Player selected: {movie.title}");
    }
}
