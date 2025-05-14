using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlexAuthSubPanel : MonoBehaviour
{
    [Header("Plex Auth Sub-Panel Components")]
    public TextMeshProUGUI authInstructionText;
    public TextMeshProUGUI codeText;
    public TextMeshProUGUI linkMessageText;
    public Button connectButton;
    public Button retryConnectionButton;

    string authInstructions = "Ready to roll? Head to plex.tv/link and plug in the code below to connect your Plex account.";
    string code = "Need";
    string linkMessage = "Attempting to communicate with Plex... \n (25)";
}
