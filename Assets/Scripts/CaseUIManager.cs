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
            int index = i + 1;
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
        OnCaseSelected?.Invoke(index);

        var btn = caseButtons[index - 1];

        btn.interactable = false;

        var label = btn.GetComponentInChildren<TMP_Text>();
        label.text = $"YOUR CASE \n {index}";
        Hide();
    }
}
