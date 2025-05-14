using UnityEngine;
using TMPro;

public class AppInfoPanel : MonoBehaviour
{
    [Header("Text Components")]
    public TextMeshProUGUI companyNameText;
    public TextMeshProUGUI productNameText;
    public TextMeshProUGUI versionText;

    //Edit app info can be edited through... 
    //Company Name -> Edit > Project Settings > Player > Other Settings
    //Product Name -> Edit > Project Settings > Player > Other Settings
    //Version -> Edit > Project Settings > Player > Identification

    private string companyName;
    private string productName;
    private string version;

    private void Awake()
    {
        companyName = Application.companyName;
        productName = Application.productName;
        version = Application.version;
    }
    private void OnEnable()
    {
        companyNameText.text = companyName;
        productNameText.text = productName;
        versionText.text = version;

        Debug.Log($"[AppInfoPanel][OnEnable] Panel Initilized \n Company = {companyName} \n Product = {productName} \n Version = {version}");

    }
}
