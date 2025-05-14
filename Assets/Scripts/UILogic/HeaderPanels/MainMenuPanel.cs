using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MainMenuPanel : MonoBehaviour
{
    [Header("Main Menu Header Components")]
    public TextMeshProUGUI mainMenuTitle;

    private void Awake()
    {
        mainMenuTitle.text = Application.productName;
    }
}
