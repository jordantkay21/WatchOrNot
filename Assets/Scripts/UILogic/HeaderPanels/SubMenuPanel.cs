using TMPro;
using UnityEngine;

public class SubMenuPanel : MonoBehaviour
{
    [Header("SubMenu Header Components")]
    public TextMeshProUGUI subMenuTitle;

    public void SetSubMenuTitle(string title)
    {
        subMenuTitle.text = title;
    }
}
