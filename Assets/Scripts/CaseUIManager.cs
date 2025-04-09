using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CaseUIManager : MonoBehaviour
{
    public GameObject casePanel;
    public Button[] caseButtons;

    public System.Action<int> OnCaseSelected;

    public void Show()
    {
        casePanel.SetActive(true);

        for (int i = 0; i < caseButtons.Length; i++)
        {
            int index = i;
            caseButtons[i].onClick.RemoveAllListeners();
            caseButtons[i].onClick.AddListener(() => SelectCase(index));
        }
    }

    public void Hide()
    {
        casePanel.SetActive(false);
    }

    private void SelectCase(int index)
    {
        if (!GameManager.Instance.HasPlayerChosenCase())
        {
            OnCaseSelected?.Invoke(index);

            var btn = caseButtons[index];

            btn.interactable = false;

            var label = btn.GetComponentInChildren<TMP_Text>();
            label.text = $"YOUR CASE \n {index + 1}";
            Hide();

        }
        else
        {
            OnCaseSelected?.Invoke(index);

            var btn = caseButtons[index];
            btn.interactable = false;

            var poster = btn.GetComponentInChildren<RawImage>();
            var revealedMovie = GameManager.Instance.GetRevealedMovieInfo();
 
            if (poster != null)
            {
                poster.texture = revealedMovie.posterTexture;
                poster.enabled = true;
            }
            else
            {
                Debug.LogWarning($"CaseUIManager failed to retrieve case {index + 1} RawImage Component");
            }
        }
    }
}
