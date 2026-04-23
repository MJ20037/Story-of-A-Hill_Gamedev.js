using UnityEngine;
using UnityEngine.UI;

public class AttackFlashUI : MonoBehaviour
{
    public static AttackFlashUI Instance;

    public Image flashImage;
    public float flashSpeed = 4f;
    public float maxAlpha = 0.5f;

    private bool isFlashing;

    void Awake()
    {
        Instance = this;

        SetAlpha(0f);
    }

    void Update()
    {
        if (!isFlashing) return;

        float alpha = Mathf.PingPong(Time.time * flashSpeed, maxAlpha);
        SetAlpha(alpha);
    }

    public void StartFlashing()
    {
        isFlashing = true;
    }

    public void StopFlashing()
    {
        isFlashing = false;
        SetAlpha(0f);
    }

    void SetAlpha(float a)
    {
        if (flashImage == null) return;

        Color c = flashImage.color;
        c.a = a;
        flashImage.color = c;
    }
}