using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DealPopup : MonoBehaviour
{
    public static DealPopup Instance { get; private set; }

    [Header("UI References")]
    public GameObject popupPanel;
    public TMP_Text messageText;
    public Button primaryButton;
    public TMP_Text primaryButtonText;
    public Button secondaryButton;
    public TMP_Text secondaryButtonText;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Used for simple "Deal" messages with one button
    public void ShowDealPopup(string message, string buttonText, Action onContinue)
    {
        popupPanel.SetActive(true);
        messageText.text = message;

        primaryButtonText.text = buttonText;
        secondaryButton.gameObject.SetActive(false);

        primaryButton.onClick.RemoveAllListeners();
        primaryButton.onClick.AddListener(() =>
        {
            popupPanel.SetActive(false);
            onContinue?.Invoke();
        });

        primaryButton.gameObject.SetActive(true);
    }

    // Used for switch offer — 2 choices
    public void ShowSwitchOffer(string message, string keepText, string switchText, Action<bool> onChoice)
    {
        popupPanel.SetActive(true);
        messageText.text = message;

        primaryButtonText.text = keepText;
        secondaryButtonText.text = switchText;

        primaryButton.onClick.RemoveAllListeners();
        secondaryButton.onClick.RemoveAllListeners();

        primaryButton.onClick.AddListener(() =>
        {
            popupPanel.SetActive(false);
            onChoice?.Invoke(false); // Keep
        });

        secondaryButton.onClick.AddListener(() =>
        {
            popupPanel.SetActive(false);
            onChoice?.Invoke(true); // Switch
        });

        primaryButton.gameObject.SetActive(true);
        secondaryButton.gameObject.SetActive(true);
    }
}
