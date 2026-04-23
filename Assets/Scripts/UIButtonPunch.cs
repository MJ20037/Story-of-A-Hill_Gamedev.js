using UnityEngine;
using UnityEngine.EventSystems;

public class UIButtonPunch : MonoBehaviour, IPointerDownHandler
{
    [Header("Animation Settings")]
    public float punchScale = 1.1f;
    public float duration = 0.15f;

    private RectTransform rect;
    private Vector3 originalScale;
    private Coroutine animRoutine;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        PlayPunch();
    }

    public void PlayPunch()
    {
        if (animRoutine != null)
            StopCoroutine(animRoutine);

        animRoutine = StartCoroutine(PunchRoutine());
    }

    System.Collections.IEnumerator PunchRoutine()
    {
        float half = duration * 0.5f;
        float timer = 0f;

        Vector3 targetScale = originalScale * punchScale;

        while (timer < half)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / half;
            rect.localScale = Vector3.Lerp(originalScale, targetScale, EaseOut(t));
            yield return null;
        }

        timer = 0f;
        while (timer < half)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / half;
            rect.localScale = Vector3.Lerp(targetScale, originalScale, EaseIn(t));
            yield return null;
        }

        rect.localScale = originalScale;
    }

    float EaseOut(float t)
    {
        return 1 - Mathf.Pow(1 - t, 3);
    }

    float EaseIn(float t)
    {
        return t * t * t;
    }
}
