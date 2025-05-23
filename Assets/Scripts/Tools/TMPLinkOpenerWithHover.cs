using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TMPLinkOpenerWithHover : MonoBehaviour, IPointerClickHandler, IPointerExitHandler
{
    private TextMeshProUGUI textMesh;
    private int lastLinkIndex = -1;

    void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        Vector3 mousePos = Input.mousePosition;
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMesh, mousePos, null);

        if (linkIndex != lastLinkIndex)
        {
            RestoreAllLinkColors();
            if (linkIndex != -1)
            {
                HighlightLink(linkIndex, "#FFD700"); // Gold
            }
            lastLinkIndex = linkIndex;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(textMesh, eventData.position, null);
        if (linkIndex != -1)
        {
            var linkInfo = textMesh.textInfo.linkInfo[linkIndex];
            string id = linkInfo.GetLinkID();

            if (id == "plex-link")
            {
                Application.OpenURL("https://plex.tv/link");
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RestoreAllLinkColors();
        lastLinkIndex = -1;
    }

    private void HighlightLink(int index, string hexColor)
    {
        TMP_LinkInfo linkInfo = textMesh.textInfo.linkInfo[index];
        textMesh.text = textMesh.text.Replace(linkInfo.GetLinkText(), $"<color={hexColor}>{linkInfo.GetLinkText()}</color>");
    }

    private void RestoreAllLinkColors()
    {
        // Reset text to original (or regenerate dynamically if needed)
        textMesh.text =
            "<voffset=5><size=110%><b>ALL EYES ON YOU!</b></size></voffset>\n" +
            "Enter your code at <link=\"plex-link\"><color=#00BFFF><u>https://plex.tv/link</u></color></link>";
    }
}