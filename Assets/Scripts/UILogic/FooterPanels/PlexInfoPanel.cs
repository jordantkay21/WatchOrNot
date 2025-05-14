using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlexInfoPanel : MonoBehaviour
{
    [Header("Text Component")]
    public TextMeshProUGUI usernameText;

    private void OnEnable()
    {
        usernameText.text = "Username";
        Debug.Log($"[PlexInfoPanel][OnEnable] Panel Initilized \n Username = {usernameText.text}");
    }
}
