using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmationPopup : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text messageText;
    public Button confirmButton;
    public Button cancelButton;

    private Action onConfirm;
    private Action onCancel;

    private void Awake()
    {
        panel.SetActive(false);
        confirmButton.onClick.AddListener(HandleConfirm);
        cancelButton.onClick.AddListener(HandleCancel);
    }

    public void Show(string message, Action onConfirmCallback, Action onCancelCallback = null)
    {
        messageText.text = message;
        onConfirm = onConfirmCallback;
        onCancel = onCancelCallback;

        panel.SetActive(true);
    }

    private void HandleConfirm()
    {
        onConfirm?.Invoke();
        panel.SetActive(false);
    }

    private void HandleCancel()
    {
        onCancel?.Invoke();
        panel.SetActive(false);
    }

}
