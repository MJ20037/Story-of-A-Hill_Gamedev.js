using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public TextMeshPro text;
    public float moveSpeed = 2f;
    public float lifetime = 1f;

    float timer;

    void Update()
    {
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        timer += Time.deltaTime;

        if (timer >= lifetime)
        {
            FloatingTextPool.Instance.Release(this);
        }
    }

    public void Setup(int amount)
    {
        text.text = "+" + amount;
        transform.position += new Vector3(Random.Range(-0.2f, 0.2f), 0f, 0f);
        timer = 0f;
    }
}