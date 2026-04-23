using UnityEngine;

public class RescueVehicle : MonoBehaviour, IPoolable
{
    public float speed = 8f;

    private Animal target;
    private float returnX;

    private bool carrying;

    public void BeginPickup(Animal animal, float spawnX)
    {
        target = animal;
        carrying = false;
        returnX = spawnX;

        transform.position = new Vector3(returnX, animal.transform.position.y, 0f);
    }

    void Update()
    {
        if (target == null) return;

        if (!carrying)
        {
            // move TO animal
            Vector3 targetPos = new Vector3(target.transform.position.x, transform.position.y, 0f);

            transform.position = Vector3.MoveTowards(transform.position, targetPos, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos) < 0.2f)
            {
                PickUp();
            }
        }
        else
        {
            // move BACK to left
            transform.position = Vector3.MoveTowards(
                transform.position,
                new Vector3(returnX, transform.position.y, 0f),
                speed * Time.deltaTime
            );

            if (Mathf.Abs(transform.position.x - returnX) < 0.1f)
            {
                Finish();
            }
        }
    }

    void PickUp()
    {
        carrying = true;

        target.AttachToVehicle(transform);
        transform.Rotate(0,180,0);
    }

    void Finish()
    {
        target.ReleaseAfterRescue();
        target = null;

        GameObjectPoolManager.Instance.Release(gameObject);
    }

    public void OnSpawnedFromPool()
    {
        carrying = false;
        target = null;
    }

    public void OnReturnedToPool()
    {
        carrying = false;
        target = null;
    }
}