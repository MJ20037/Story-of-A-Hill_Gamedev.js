using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class WorkerManager : MonoBehaviour
{
    public GameObject workerPrefab;
    public Transform spawnPoint;

    public int workerCount = 0;
    public int maxWorkerCount = 50;
    public List<Worker> workers = new List<Worker>();
    public Tool defaultTool;
    public Button hireWorkerButton;
    public TMP_Text workerCostUI;

    public int baseCost = 10;
    public int maxCost = 2000;
    public float costMultiplier = 1.6f;
    
    void Start()
    {
        UpdateUICost();
    }

    public int GetWorkerCost()
    {
        return (int)Mathf.Min(baseCost * Mathf.Pow(costMultiplier,workerCount),maxCost);
    }

    public void HireWorker()
    {
        if(workerCount >= maxWorkerCount) return;

        int cost = GetWorkerCost();

        if (GameManager.Instance.money < cost)
        {
            UIManager.Instance.FlashError();
            AudioDirector.Instance?.PlayUIFail();
            return;
        }

        GameManager.Instance.RemoveMoney(cost);
        AudioDirector.Instance?.PlayUISuccess();
        AttackManager.Instance?.OnWorkerHired();

        SpawnWorker();
        workerCount++;
        UpdateUICost();
        if(workerCount == maxWorkerCount)
        {
            hireWorkerButton.interactable=false;
            workerCostUI.text="MAX";

        }
    }

    public void UpdateUICost(){
        workerCostUI.text="$"+GetWorkerCost();
    }

    void SpawnWorker()
    {
        Worker w = Instantiate(workerPrefab, spawnPoint.position, Quaternion.identity)
            .GetComponent<Worker>();

        workers.Add(w);

        // Assign default tool (pickaxe)
        w.ApplyTool(defaultTool);
    }

    public void EnableHiring()
    {
        if(workerCount <= maxWorkerCount)
        {
            hireWorkerButton.interactable=true;
            UpdateUICost();
        }
    }
}