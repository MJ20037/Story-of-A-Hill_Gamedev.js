using UnityEngine;
using UnityEngine.UI;

public class ToolButtonUI : MonoBehaviour
{
    public PlayerTool tool;
    public Button button;

    void Update()
    {
        if (PlayerToolManager.Instance == null) return;

        bool isActive = PlayerToolManager.Instance.currentTool == tool;

        button.interactable = !isActive;
    }
}