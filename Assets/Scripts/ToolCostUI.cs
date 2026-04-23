using UnityEngine;
using TMPro;

public class ToolCostUI : MonoBehaviour
{
    public TextMeshProUGUI gunCostText;
    public TextMeshProUGUI tranqCostText;

    void Update()
    {
        if (PlayerToolManager.Instance == null) return;

        gunCostText.text = "$" + PlayerToolManager.Instance.GetGunCost();
        tranqCostText.text = "$" + PlayerToolManager.Instance.GetTranqCost();
    }
}