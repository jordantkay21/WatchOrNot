using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LoadingUIManager : MonoBehaviour
{
    public static LoadingUIManager Instance;

    public GameObject loadingPanel;
    public Slider progressBar;
    public TMP_Text statusText;

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

        Hide();
    }

    public void Show()
    {
        loadingPanel.SetActive(true);
        UpdateProgress(0f);
        UpdateStatus("Starting...");
    }

    public void UpdateStatus(string message)
    {
        statusText.text = message;
    }

    public void UpdateProgress(float value)
    {
        progressBar.value = Mathf.Clamp01(value);
    }

    public void Hide()
    {
        loadingPanel.SetActive(false);
    }
}
