using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RevealStageManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject CasePanel;

    [Header("Case Buttons")]
    public List<Button> caseButtons;
    public List<TMP_Text> caseTexts;

    private void Start()
    {
        CasePanel.SetActive(false);

        GameManager.OnRevealStageStarted += BeginRevealPhase;
    }

    public void BeginRevealPhase()
    {
        InitilizeCaseButtons();

        CasePanel.SetActive(true);
    }
    private void InitilizeCaseButtons()
    {
        for (int i = 0; i < caseButtons.Count; i++)
        {
            int caseNumer = i + 1;

            if (caseTexts[i] != null)
                caseTexts[i].text = caseNumer.ToString();

            SetupHoverOutline(caseButtons[i], "#FF008C");
        }

        Debug.Log("[RevealPhaseManager][InitilizeCaseButtons] Case buttons initialized with hover and numbers.");
    }

    private void SetupHoverOutline(Button button, string hexColor)
    {
        Outline outline = button.GetComponent<Outline>();
        if (outline == null)
        {
            outline = button.gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(2, 2);
        }

        EventTrigger trigger = button.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = button.gameObject.AddComponent<EventTrigger>();

        trigger.triggers.Clear();

        AddEventTrigger(trigger, EventTriggerType.PointerEnter, () =>
        {
            if (ColorUtility.TryParseHtmlString(hexColor, out Color highlight))
                outline.effectColor = highlight;
        });

        AddEventTrigger(trigger, EventTriggerType.PointerExit, () =>
        {
            outline.effectColor = Color.white;
        });
    }

    private void AddEventTrigger(EventTrigger trigger, EventTriggerType type, UnityEngine.Events.UnityAction action)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry { eventID = type };
        entry.callback.AddListener((_) => action());
        trigger.triggers.Add(entry);
    }
}
