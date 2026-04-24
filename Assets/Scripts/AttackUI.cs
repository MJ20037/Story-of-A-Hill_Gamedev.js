using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class AttackUI : MonoBehaviour
{
    void OnEnable()
    {
        InputHandler.OnClicked += HandleClick;
    }

    void OnDisable()
    {
        InputHandler.OnClicked -= HandleClick;
    }

    void HandleClick(Vector3 pos)
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (PlayerToolManager.Instance == null)
            return;

        PlayerTool tool = PlayerToolManager.Instance.currentTool;

        if (tool == PlayerTool.Pickaxe)
            return;

        bool paid = false;

        if (tool == PlayerTool.Gun)
            paid = PlayerToolManager.Instance.TryUseGun();
        else if (tool == PlayerTool.Tranq)
            paid = PlayerToolManager.Instance.TryUseTranq();

        if (!paid)
            return;
        
        if (tool == PlayerTool.Gun){
            ShotVfxSpawner.Instance.SpawnGun(pos);
            AudioDirector.Instance.PlayGunShot();
        }
        else if (tool == PlayerTool.Tranq){
            ShotVfxSpawner.Instance.SpawnTranq(pos);
            AudioDirector.Instance.PlayTranqShot();
        }

        Collider2D hit;

        if (IsTouchInput())
        {
            hit = Physics2D.OverlapCircle(pos, 0.25f);
        }
        else
        {
            hit = Physics2D.OverlapPoint(pos);
        }
        if (hit == null) return;

        ShootTarget target = hit.GetComponent<ShootTarget>();

        if (target != null)
        {
            target.OnHit(PlayerToolManager.Instance.currentTool);
            return;
        }

        Animal animal = hit.GetComponent<Animal>();
        if (animal == null) return;



        if (tool == PlayerTool.Gun)
            animal.KillAnimal();
        else if (tool == PlayerTool.Tranq)
            animal.Tranquilize();  
    }

    bool IsTouchInput()
    {
        return Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed;
    }
}