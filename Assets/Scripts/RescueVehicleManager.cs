using UnityEngine;

public class RescueVehicleManager : MonoBehaviour
{
    public static RescueVehicleManager Instance;

    public GameObject rescuePrefab;

    void Awake()
    {
        Instance = this;
    }

    public void RequestPickup(Animal animal)
    {
        if (animal == null) return;

        float spawnX = ProgressCameraController.Instance.GetLeftSpawnX();

        float y = animal.transform.position.y;

        Vector3 pos = new Vector3(spawnX, y, 0f);

        GameObject obj = GameObjectPoolManager.Instance.Spawn(rescuePrefab, pos, Quaternion.identity);

        RescueVehicle vehicle = obj.GetComponent<RescueVehicle>();

        vehicle.BeginPickup(animal, spawnX);
    }
}