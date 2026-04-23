using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance;

    public float shakeAmount = 0.2f;

    Vector3 originalPos;
    bool shaking;

    void Awake()
    {
        Instance = this;
        originalPos = transform.localPosition;
    }

    public void StartShake(float amount, float duration)
    {
        shakeAmount = amount;
        shaking = true;
        StartCoroutine(Shake(duration));
    }

    System.Collections.IEnumerator Shake(float duration)
    {
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            transform.localPosition = originalPos + (Vector3)Random.insideUnitCircle * shakeAmount;

            yield return null;
        }

        transform.localPosition = originalPos;
        shaking = false;
    }
}