using UnityEngine;

public class Landslide : MonoBehaviour
{
    public float speed = 8f;

    void Update()
    {
        transform.position += Vector3.left * speed * Time.deltaTime;
    }
}