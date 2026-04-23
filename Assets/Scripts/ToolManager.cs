using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class ToolManager : MonoBehaviour
{
    public List<Tool> tools;
    Dictionary<Tool, int> purchaseCounts = new Dictionary<Tool, int>();

    public TMP_Text jcbCostUI;
    public TMP_Text drillCostUI;

    private WorkerManager workerManager;

    void Start()
    {
        workerManager = FindObjectOfType<WorkerManager>();
    }

    public void BuyTool(int index)
    {
        if (index < 0 || index >= tools.Count) return;

        Tool tool = tools[index];
        int cost = GetToolCost(tool);

        if (workerManager.workers.Count == 0)
        {
            AudioDirector.Instance?.PlayUIFail();
            return;
        }

        if (AllWorkersHaveBetterOrEqual(tool))
        {
            AudioDirector.Instance?.PlayUIFail();
            return;
        }

        if (GameManager.Instance.money < cost)
        {
            AudioDirector.Instance?.PlayUIFail();
            UIManager.Instance.FlashError();

            return;
        }

        AudioDirector.Instance?.PlayToolUpgrade(tool);

        GameManager.Instance.RemoveMoney(cost);
        purchaseCounts[tool] = GetPurchaseCount(tool) + 1;

        AssignTool(tool);
        UpdateCostUI(tool);
    }

    void AssignTool(Tool tool)
    {
        Worker weakest = null;

        foreach (var w in workerManager.workers)
        {
            if (w == null) continue;

            if (weakest == null)
            {
                weakest = w;
                continue;
            }

            int currentTier = w.currentTool != null ? w.currentTool.tier : -1;
            int weakestTier = weakest.currentTool != null ? weakest.currentTool.tier : -1;

            if (currentTier < weakestTier)
            {
                weakest = w;
            }
        }

        if (weakest != null)
        {
            weakest.ApplyTool(tool);
        }
    }

    bool AllWorkersHaveBetterOrEqual(Tool tool)
    {
        foreach (var w in workerManager.workers)
        {
            if (w == null) continue;

            if (w.currentTool == null) return false;

            if (w.currentTool.tier < tool.tier)
                return false;
        }

        return true;
    }

    public int GetToolCost(Tool tool)
    {
        int count = GetPurchaseCount(tool);

        float scaled = tool.baseCost * Mathf.Pow(tool.costMultiplier, count);
        int linear = tool.flatIncrease * count;

        return Mathf.Min(Mathf.RoundToInt(scaled + linear),tool.maxCost);
    }

    int GetPurchaseCount(Tool tool)
    {
        if (!purchaseCounts.ContainsKey(tool))
            purchaseCounts[tool] = 0;

        return purchaseCounts[tool];
    }

    void UpdateCostUI(Tool tool)
    {
        if(tool.toolName=="Drill"){
            drillCostUI.text="$"+GetToolCost(tool);
        } else if(tool.toolName=="Bulldozer"){
            jcbCostUI.text="$"+GetToolCost(tool);
        }
    }
}