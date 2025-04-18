using UnityEngine;

public class PlaylistSelectionUIController : MonoBehaviour
{
    public static PlaylistSelectionUIController Instance;

    public GameObject PlaylistSelectionPanel;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void StartSelectionPhase()
    {
        PlaylistSelectionPanel.SetActive(true);
    }
}
