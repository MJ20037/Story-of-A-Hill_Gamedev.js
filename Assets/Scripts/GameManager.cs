using UnityEngine;
public enum GameState
    {
        Playing,
        Attack,
        End
    }

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int money;

    public GameState currentState = GameState.Playing;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SceneFader.Instance.FadeOut();
    }

    public void AddMoney(int amount)
    {
        money += amount;
        if (UIManager.Instance != null)
            UIManager.Instance.SetMoney(money);
    }

    public void RemoveMoney(int amount){
        money -= amount;
        if (UIManager.Instance != null)
            UIManager.Instance.SetMoney(money);
    }

    public int GetMoney()
    {
        return money;
    }
}