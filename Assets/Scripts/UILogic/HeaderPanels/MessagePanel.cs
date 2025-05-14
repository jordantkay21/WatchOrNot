using UnityEngine;
using TMPro;

public class MessagePanel : MonoBehaviour
{
    [Header("Message Panel Components")]
    public TextMeshProUGUI messageText;

    public void SetMessage(string message)
    {
        messageText.text = message;
    }
}
