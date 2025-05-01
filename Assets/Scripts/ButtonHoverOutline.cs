using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonHoverOutline : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Color hoverColor;

    private Outline outline;
    private Color originalColor;

    private void Awake()
    {
        outline = GetComponent<Outline>();

        if(outline == null)
        {
            outline = gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(2, 2);
        }

        originalColor = outline.effectColor;
    }

    public void SetOriginalOutlineColor(Color newColor)
    {
        originalColor = newColor;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        outline.effectColor = hoverColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        outline.effectColor = originalColor;
    }

}
