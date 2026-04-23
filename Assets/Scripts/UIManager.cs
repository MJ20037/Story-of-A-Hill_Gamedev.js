using UnityEngine;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    public TextMeshProUGUI moneyText;

    public Color normalColor = Color.black;
    public Color errorColor = Color.red;

    private Coroutine flashRoutine;

    private float displayedMoney;   // float for smooth lerp
    private int targetMoney;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        displayedMoney = GameManager.Instance.money;
        targetMoney = GameManager.Instance.money;
        UpdateText();
    }

    void Update()
    {
        if (Mathf.Abs(displayedMoney - targetMoney) > 0.01f)
        {
            displayedMoney = Mathf.Lerp(displayedMoney, targetMoney, Time.deltaTime * 10f);

            // snap when very close
            if (Mathf.Abs(displayedMoney - targetMoney) < 0.5f)
                displayedMoney = targetMoney;

            UpdateText();
        }
    }

    public void SetMoney(int amount)
    {
        targetMoney = amount;
    }

    void UpdateText()
    {
        moneyText.text = "$ " + ((int)displayedMoney).ToString();
    }

    public void FlashError()
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    IEnumerator FlashRoutine()
    {
        float duration = 0.5f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float lerp = Mathf.PingPong(t * 10f, 1f);
            moneyText.color = Color.Lerp(normalColor, errorColor, lerp);
            yield return null;
        }

        moneyText.color = normalColor;
    }
}