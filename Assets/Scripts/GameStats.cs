using UnityEngine;

public class GameStats : MonoBehaviour
{
    public static GameStats Instance;

    public int animalsKilled = 0;
    public int animalsRescued = 0;

    private float startTime;
    private float endTime;
    private int money;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        startTime = Time.time;
    }

    public void RegisterKill()
    {
        animalsKilled++;
    }

    public void RegisterRescue()
    {
        animalsRescued++;
    }

    public void MarkEnd()
    {
        endTime = Time.time;
        money= GameManager.Instance.GetMoney();
    }

    public float GetTimeTaken()
    {
        return endTime - startTime;
    }

    public int GetMoney()
    {
        return money;
    }
}