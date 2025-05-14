using UnityEngine;
using TMPro;

public class WelcomeSubPanel : MonoBehaviour
{
    [Header("Welcome Sub-Panel Components")]
    public TextMeshProUGUI welcomeText;

    public void UpdateTitle(string title)
    {
        welcomeText.text = title;
    }
}
