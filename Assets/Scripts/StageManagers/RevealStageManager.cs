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
        else
        {
            if (revealsRemainingThisRound <= 0 || revealedCaseNumbers.Contains(caseNumber) || caseNumber == playerChosenCaseNumber)
                return;

            GameManager.OnMovieRevealed?.Invoke(caseAssignments[caseNumber]);
            ShowRevealedPoster(caseNumber);

            revealedCaseNumbers.Add(caseNumber);
            revealsRemainingThisRound--;

            Debug.Log($"[RevealStageManager][StartRevealRound] Player has revealed case {caseNumber} and has revealed movie {caseAssignments[caseNumber].title}");

        }

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


}
