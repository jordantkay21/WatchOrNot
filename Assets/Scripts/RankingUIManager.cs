using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class RankingUIManager : MonoBehaviour
{
    public GameObject movieInfoPanel;
    public TMP_Text titleText;
    public RawImage posterImage;
    public TMP_Text summaryText;
    public TMP_Text genreText;
    public TMP_Text durationText;
    public Button watchTrailerButton;

    public GameObject rankButtonPanel;
    public Button[] rankButtons;

    public GameObject resultsPanel;
    public Transform resultsListContainer;
    public GameObject resultEntryPrefab;

    private GameObject firstSwapEntry;
    public Button finalizeRankingButton;
    private int maxSwapsAllowed = 3;

    private TMP_Text firstHighlightedTextComponent;
    private TMP_Text secondHighlightedTextComponent;
    private Color originalTextColor;
    private FontStyles originalFontStyle;

    private Dictionary<int, GameObject> resultEntries = new();

    public void DisplayMovie(MovieInfo movie)
    {
        titleText.text = $"{movie.title} ({movie.year})";
        posterImage.texture = movie.posterTexture;
        summaryText.text = $"{movie.summary}";


        if (!string.IsNullOrEmpty(movie.trailerUrl))
        {
            watchTrailerButton.gameObject.SetActive(true);
            watchTrailerButton.onClick.RemoveAllListeners();
            watchTrailerButton.onClick.AddListener(() =>
            {
                Application.OpenURL(movie.trailerUrl);
            });
        }
        else
        {
            watchTrailerButton.gameObject.SetActive(false);
        }

        genreText.text = $"GENRE: {movie.genres}";
        durationText.text = $"DURATION: {movie.duration}";
    }

    public void ShowMovieInfo()
    {
        movieInfoPanel.SetActive(true);
    }
    public void ShowResults(List<(int rank, MovieInfo movie)> ranked)
    {
        resultsPanel.SetActive(true);

        foreach (Transform child in resultsListContainer)
            Destroy(child.gameObject);

        resultEntries.Clear();

        foreach (var entry in ranked)
        {
            GameObject resultGO = Instantiate(resultEntryPrefab, resultsListContainer);
            resultGO.name = $"Rank {entry.rank} Result Entry";

            var texts = resultGO.GetComponentsInChildren<TMP_Text>();

            var rankText = texts.First(t => t.name == "RankText");
            var titleText = texts.First(t => t.name == "TitleText");

            rankText.text = entry.rank.ToString();
            titleText.text = entry.movie.title;

            resultEntries[entry.rank] = resultGO;
        }
    }

    public void ShowRankButtons()
    {
        rankButtonPanel.SetActive(true);
    }

    public void HideRankButtons()
    {
        rankButtonPanel.SetActive(false);
    }

    public void HideMovieInfo()
    {
        movieInfoPanel.SetActive(false);
    }

    public void SetRankLabel(int rank, string movieTitle)
    {
        if (rank - 1 >= rankButtons.Length) return;

        var btn = rankButtons[rank - 1];
        btn.interactable = false;

        var label = btn.GetComponentInChildren<TMP_Text>();
        label.text = $"{rank}\n{movieTitle}";
    }

    public void ShowError(string msg)
    {
        Debug.LogWarning(msg);
    }

    public void CrossOutByTitle(string title)
    {
        foreach (var entry in resultEntries.Values)
        {
            var texts = entry.GetComponentsInChildren<TMP_Text>();
            var titleText = texts.FirstOrDefault(t => t.name == "TitleText");

            if (titleText != null && titleText.text == title)
            {
                titleText.fontStyle |= FontStyles.Bold;
                titleText.fontStyle |= FontStyles.Strikethrough;
                break;
            }
        }
    }

    public void EnableSwappingUI()
    {
        foreach (var entry in resultEntries.Values)
        {
            var button = entry.transform.Find("SwapButton")?.GetComponent<Button>();
            
            if(button == null)
            {
                Debug.LogWarning($"No 'SwapButton' found in result entry: {entry.name}");
                continue;
            }
                
            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => OnResultEntryClicked(entry));
        }

        finalizeRankingButton.gameObject.SetActive(true);
        finalizeRankingButton.onClick.RemoveAllListeners();
        finalizeRankingButton.onClick.AddListener(() => GameManager.Instance.FinalizeRanking());
    }

    private void OnResultEntryClicked(GameObject clickedEntry)
    {
        if (firstSwapEntry == null)
        {
            firstSwapEntry = clickedEntry;
            Debug.Log($"RankingUIManager.OnResultEntryClicked(): firstSwapEntry assigned to {clickedEntry.name}.");

            var titleText = clickedEntry.GetComponentsInChildren<TMP_Text>().FirstOrDefault(t => t.name == "TitleText");

            if(titleText != null)
            {
                firstHighlightedTextComponent = titleText;
                originalTextColor = titleText.color;
                originalFontStyle = titleText.fontStyle;

                titleText.color = Color.black;
                titleText.fontStyle = FontStyles.Bold;
            }
        }
        else if (firstSwapEntry == clickedEntry)
        {
            firstSwapEntry = null;
            Debug.Log("RankingUIManager.OnResultEntryClicked(): firstSwapEntry deselected. Assigned null.");
            ResetTextHighlight();
        }
        else
        {
            var titleText = clickedEntry.GetComponentsInChildren<TMP_Text>().FirstOrDefault(t => t.name == "TitleText");

            if (titleText != null)
            {
                secondHighlightedTextComponent = titleText;

                titleText.color = Color.black;
                titleText.fontStyle = FontStyles.Bold;
            }

            GameObject secondEntry = clickedEntry;
            Debug.Log($"RankingUIManager.OnResultEntryClicked(): firstSwapEntry selected. SecondEntry assigned to {clickedEntry.name}.");

            var firstTexts = firstSwapEntry.GetComponentsInChildren<TMP_Text>();
            if (firstTexts == null) {Debug.LogError ($"RankingUIManager.OnResultEntryClicked(): Text componenets not found within first entry:{firstSwapEntry.name}'s hierarchy."); return; }
            else Debug.Log($"RankingUIManager.OnResultEntryClicked(): First Result Entry Text Components Retrieved: {firstTexts} on {firstSwapEntry.name}");

            var secondTexts = secondEntry.GetComponentsInChildren<TMP_Text>();
            if (firstTexts == null) { Debug.LogError($"RankingUIManager.OnResultEntryClicked(): Text componenets not found within second entry: {secondEntry.name}'s hierarchy."); return; }
            else Debug.Log($"RankingUIManager.OnResultEntryClicked(): First Result Entry Text Components Retrieved: {secondTexts} on {secondEntry.name}");

            var firstTitleTextComponent = firstTexts.FirstOrDefault(t => t.name == "TitleText");
            if (firstTitleTextComponent == null){ Debug.LogError($"TitleText not found on first result entry {firstSwapEntry.name}."); return;}
            else Debug.Log($"RankingUIManager.OnResultEntryClicked(): TitleText retrieved on first result entry {firstTitleTextComponent}");

            var firstTitleText = firstTitleTextComponent.text;
            Debug.Log($"firstTitleText is assigned: {firstTitleText}");

            var secondTitleTextComponent = secondTexts.FirstOrDefault(t => t.name == "TitleText");
            if (secondTitleTextComponent == null) { Debug.LogError($"TitleText not found on second result entry {secondEntry.name}."); return; }
            else Debug.Log($"RankingUIManager.OnResultEntryClicked(): TitleText retrieved on second result entry {secondTitleTextComponent}");

            var secondTitleText = secondTitleTextComponent.text;
            Debug.Log($"secondTitleText is assigned: {secondTitleText}");


            PopUpUI.Instance.ShowPopup(
                $"Swap \"{firstTitleText}\" with \"{secondTitleText}\"?",
                "Confirm",
                () =>
                {
                    ResetTextHighlight();
                    SwapEntries(firstSwapEntry, secondEntry);
                    firstSwapEntry = null;
                    GameManager.Instance.OnRankSwapped();
                },
                 "Cancel",
                () =>
                {
                    ResetTextHighlight();
                    firstSwapEntry = null;
                });
        }
    }
    
    private void SwapEntries(GameObject a, GameObject b)
    {
        var aTexts = a.GetComponentsInChildren<TMP_Text>();
        if (aTexts == null) { Debug.LogError($"RankingUIManager.SwapEntries(): Text components not found within first entry:{a.name}'s hierarchy."); return; }
        else Debug.Log($"RankingUIManager.SwapEntries(): First Result Entry Text Components Retrieved: {aTexts} on {a.name}");

        var bTexts = b.GetComponentsInChildren<TMP_Text>();
        if (bTexts == null) { Debug.LogError($"RankingUIManager.SwapEntries(): Text components not found within second entry:{b.name}'s hierarchy."); return; }
        else Debug.Log($"RankingUIManager.SwapEntries(): Second Result Entry Text Components Retrieved: {bTexts} on {b.name}");

        var aTitleTextComponent = aTexts.FirstOrDefault(t => t.name == "TitleText");
        if (aTitleTextComponent == null) { Debug.LogError($"RankingUIManager.SwapEntries(): TitleText not found on first result entry {a.name}."); return; }
        else Debug.Log($"RankingUIManager.SwapEntries(): TitleText retrieved on first result entry {aTitleTextComponent}");

        var bTitleTextComponent = bTexts.FirstOrDefault(t => t.name == "TitleText");
        if (bTitleTextComponent == null) { Debug.LogError($"RankingUIManager.SwapEntries(): TitleText not found on second result entry {b.name}."); return; }
        else Debug.Log($"RankingUIManager.SwapEntries(): TitleText retrieved on second result entry {bTitleTextComponent}");

        string aTitleText = aTitleTextComponent.text;
        Debug.Log($"RankingUIManager.SwapEntries(): aTitleText is assigned: {aTitleText}");
        string bTitleText = bTitleTextComponent.text;
        Debug.Log($"RankingUIManager.SwapEntries(): bTitleText is assigned: {bTitleText}");


        var aRank = int.Parse(aTexts.FirstOrDefault(t => t.name == "RankText").text);
        var bRank = int.Parse(bTexts.FirstOrDefault(t => t.name == "RankText").text);

        //Swap title text
        aTexts.FirstOrDefault(t => t.name == "TitleText").text = bTitleText;
        bTexts.FirstOrDefault(t => t.name == "TitleText").text = aTitleText;

        //Swap internal rank mapping
        resultEntries[aRank] = b;
        resultEntries[bRank] = a;

        GameManager.Instance.rankingSystem.SwapRanks(aRank, bRank);
    }

    public void DisableAllSwapButtons()
    {
        foreach (var entry in resultEntries.Values)
        {
            var button = entry.GetComponentInChildren<Button>();
            if (button != null)
                button.interactable = false;
        }

        finalizeRankingButton.gameObject.SetActive(false);
    }

    private void ResetTextHighlight()
    {
        if (firstHighlightedTextComponent != null)
        {
            firstHighlightedTextComponent.color = originalTextColor;
            firstHighlightedTextComponent.fontStyle = originalFontStyle;
            firstHighlightedTextComponent = null;
        }

        if (secondHighlightedTextComponent != null)
        {
            secondHighlightedTextComponent.color = originalTextColor;
            secondHighlightedTextComponent.fontStyle = originalFontStyle;
            secondHighlightedTextComponent = null;
        }
    }
}
