using UnityEngine;
using TMPro;

public class QuickPlaySubPanel : MonoBehaviour
{
    [Header("Quick Play Sub-Panel Components")]
    public TextMeshProUGUI quickplayInstructionText;
    public TextMeshProUGUI playlistTitle;
    public TextMeshProUGUI playlistSource;
    public TextMeshProUGUI playlistCount;
    public TextMeshProUGUI playlistUpdated;

    string quickplayInstructions = "Launch Game using last played playlist";
    string source = "Playlist Source";
    string title = "Playlist Title";
    string count = "Playlist Count";
    string updated = "Playlist Last Update";
}
