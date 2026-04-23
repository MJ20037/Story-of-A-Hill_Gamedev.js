using UnityEngine;
using TMPro;

public class EndSceneUI : MonoBehaviour
{
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI timeText;
    public TextMeshProUGUI killedText;
    public TextMeshProUGUI rescuedText;

    void Start()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        if (GameStats.Instance == null) return;

        int money = GameStats.Instance.GetMoney();
        moneyText.text = "Money: " + money;

        float time = GameStats.Instance.GetTimeTaken();
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);

        timeText.text = $"Time: {minutes:00}:{seconds:00}";

        killedText.text = "Animals Killed: " + GameStats.Instance.animalsKilled;

        rescuedText.text = "Animals Rescued: " + GameStats.Instance.animalsRescued;
    }
}