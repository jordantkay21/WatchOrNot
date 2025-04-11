using TMPro;
using UnityEngine;

public class RoundStatusUIManager : MonoBehaviour
{
    public static RoundStatusUIManager Instance;

    public GameObject roundStatusPanel;
    public TMP_Text roundTitleText;
    public TMP_Text roundInstructionText;
    public TMP_Text roundProgressText;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void UpdateRoundStatus(string title, string instruction, string progress = "")
    {
        roundStatusPanel.SetActive(true);
        Debug.Log($"{this} : Round title is {title}");
        roundTitleText.text = title;
        roundInstructionText.text = instruction;

        if (progress == "")
        {
            roundProgressText.gameObject.SetActive(false);
        }
        else
        {
            roundProgressText.gameObject.SetActive(true);
            roundProgressText.text = progress;
        }
    }

    public void UpdateProgress(int revealed, int total)
    {
        roundProgressText.text = $"Revealed: {revealed} / {total}";
    }

    public void Hide()
    {
        roundStatusPanel.SetActive(false);
    }
}
