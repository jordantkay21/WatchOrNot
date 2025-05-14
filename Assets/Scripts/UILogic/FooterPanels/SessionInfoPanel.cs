using UnityEngine;
using TMPro;

public class SessionInfoPanel : MonoBehaviour
{
    [Header("Text Componentes")]
    public TextMeshProUGUI playlistSourceText;
    public TextMeshProUGUI playlistTitleText;
    public TextMeshProUGUI playlistCountText;
    public TextMeshProUGUI playlistUpdateText;

    private string source = "Playlist Source";
    private string title = "Playlist Title";
    private string count = "Playlist Count";
    private string updated = "Playlist Last Update";

    private void OnEnable()
    {
        playlistSourceText.text = source;
        playlistTitleText.text = title;
        playlistCountText.text = count;
        playlistUpdateText.text = updated;

        Debug.Log($"[SessionInfoPanel][OnEnable] Panel Initilized \n Source: {source} \n Title: {title} \n Count: {count} \n Last Update: {updated}");
    }
}
