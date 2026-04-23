using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using System;

public class InputHandler : MonoBehaviour
{
    public static Action<Vector3> OnClicked;
    private PlayerInputActions inputActions;

    public Camera cam;
    public DigController digController;

    void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    void OnEnable()
    {
        inputActions.Enable();
        inputActions.Player.Click.performed += OnClick;
    }

    void OnDisable()
    {
        inputActions.Player.Click.performed -= OnClick;
        inputActions.Disable();
    }

    void OnClick(InputAction.CallbackContext ctx)
    {
        if (IsPointerOverUI())
            return;
        Vector2 screenPos = Mouse.current.position.ReadValue();

        Vector3 worldPos = cam.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z)
        );

        digController.HandleClick(worldPos);
        OnClicked?.Invoke(worldPos);
    }

    bool IsPointerOverUI()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Mouse.current.position.ReadValue();

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }
}