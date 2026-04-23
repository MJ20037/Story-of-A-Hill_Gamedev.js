using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum PlayerTool
{
    Pickaxe,
    Gun,
    Tranq
}

public class PlayerToolManager : MonoBehaviour
{
    public static PlayerToolManager Instance;

    public PlayerTool currentTool = PlayerTool.Pickaxe;

    [Header("Cursor")]
    public Texture2D pickaxeCursor;
    public Texture2D crosshairGunCursor;
    public Texture2D crosshairTranqCursor;

    [Header("Pickaxe Upgrade")]
    public float basePickaxePower = 5f;
    public float pickaxeMultiplier = 1f;

    public int pickaxeLevel = 0;
    public int baseUpgradeCost = 20;
    public int costIncrease = 15;
    public int maxPickLvl=5;
    public Button pickUpgradeButton;

    public TMP_Text pickUpgradeCostUI;

    public int maxGunShotCost=50;
    public int maxTranqShotCost=100;

    int gunShots = 0;
    int tranqShots = 0;

    void Awake()
    {
        Instance = this;
        ApplyCursor();
    }

    void Start(){
        pickUpgradeCostUI.text=GetPickaxeUpgradeCost().ToString();
    }

    public void SelectPickaxe()
    {
        currentTool = PlayerTool.Pickaxe;
        AudioDirector.Instance?.PlayPickSelect();
        ApplyCursor();
    }

    public void SelectGun()
    {
        currentTool = PlayerTool.Gun;
        AudioDirector.Instance?.PlayWeaponSelect();

        ApplyCursor();
    }

    public void SelectTranq()
    {
        currentTool = PlayerTool.Tranq;
        AudioDirector.Instance?.PlayWeaponSelect();

        ApplyCursor();
    }

    void ApplyCursor()
    {
        if (currentTool == PlayerTool.Pickaxe)
        {
            Cursor.SetCursor(pickaxeCursor, new Vector2(4,4), CursorMode.Auto);
        }
        else if(currentTool == PlayerTool.Gun)
        {
            Vector2 hotspot = new Vector2(
                crosshairGunCursor.width / 2f,
                crosshairGunCursor.height / 2f
            );

            Cursor.SetCursor(crosshairGunCursor, hotspot, CursorMode.Auto);
        }
        else
        {
            Vector2 hotspot = new Vector2(
                crosshairTranqCursor.width / 2f,
                crosshairTranqCursor.height / 2f
            );

            Cursor.SetCursor(crosshairTranqCursor, hotspot, CursorMode.Auto);
        }
    }

    public int GetGunCost()
    {
        return Mathf.Min(4 + gunShots , maxGunShotCost);
    }

    public int GetTranqCost()
    {
        return Mathf.Min(8 + tranqShots*2, maxTranqShotCost);
    }

    // 💸 ALWAYS deduct (hit or miss)
    public bool TryUseGun()
    {
        int cost = GetGunCost();

        if (GameManager.Instance.money < cost)
        {
            UIManager.Instance.FlashError();
            AudioDirector.Instance?.PlayUIFail();
            return false;
        }

        GameManager.Instance.AddMoney(-cost);
        gunShots++;
        CameraShake.Instance.StartShake(0.05f,0.1f);
        return true;
    }

    public bool TryUseTranq()
    {
        int cost = GetTranqCost();

        if (GameManager.Instance.money < cost)
        {
            UIManager.Instance.FlashError();
            AudioDirector.Instance?.PlayUIFail();
            return false;
        }

        GameManager.Instance.AddMoney(-cost);
        tranqShots++;
        CameraShake.Instance.StartShake(0.03f,0.1f);
        return true;
    }

    public float GetPickaxePower()
    {
        return basePickaxePower + pickaxeMultiplier*pickaxeLevel;
    }

    public int GetPickaxeUpgradeCost()
    {
        return (int)(baseUpgradeCost * Mathf.Pow(costIncrease, pickaxeLevel));
    }

    public void UpgradePickaxe()
    {
        if(pickaxeLevel>=maxPickLvl) return;

        int cost = GetPickaxeUpgradeCost();

        if (GameManager.Instance.money < cost)
        {
            UIManager.Instance.FlashError();
            AudioDirector.Instance?.PlayUIFail();
            return;
        }

        GameManager.Instance.AddMoney(-cost);
        AudioDirector.Instance?.PlayUISuccess();

        pickaxeLevel++;

        if(pickaxeLevel==maxPickLvl){
            pickUpgradeButton.interactable=false;
            pickUpgradeCostUI.text="MAX";
            return;
        }

        pickUpgradeCostUI.text=GetPickaxeUpgradeCost().ToString();
    }
}