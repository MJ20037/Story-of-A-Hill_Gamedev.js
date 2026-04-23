using System.Collections.Generic;
using UnityEngine;

public class FloatingTextPool : MonoBehaviour
{
    public static FloatingTextPool Instance;

    public FloatingText prefab;
    public int initialSize = 20;

    private Queue<FloatingText> pool = new Queue<FloatingText>();

    void Awake()
    {
        Instance = this;

        for (int i = 0; i < initialSize; i++)
        {
            var obj = Instantiate(prefab, transform);
            obj.gameObject.SetActive(false);
            pool.Enqueue(obj);
        }
    }

    public FloatingText Spawn(Vector3 pos, int amount)
    {
        FloatingText ft;

        if (pool.Count > 0)
        {
            ft = pool.Dequeue();
        }
        else
        {
            ft = Instantiate(prefab, transform);
        }

        ft.transform.position = pos;
        ft.gameObject.SetActive(true);
        ft.Setup(amount);

        return ft;
    }

    public void Release(FloatingText ft)
    {
        ft.gameObject.SetActive(false);
        pool.Enqueue(ft);
    }
}