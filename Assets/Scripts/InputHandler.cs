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

        if (cam == null)
            cam = Camera.main;
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

        Vector2 screenPos = GetPointerScreenPosition();

        if (cam == null)
            return;

        Vector3 worldPos = cam.ScreenToWorldPoint(
            new Vector3(screenPos.x, screenPos.y, -cam.transform.position.z)
        );

        if (digController != null)
            digController.HandleClick(worldPos);

        OnClicked?.Invoke(worldPos);
    }

    Vector2 GetPointerScreenPosition()
    {
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            return Touchscreen.current.primaryTouch.position.ReadValue();

        if (Mouse.current != null)
            return Mouse.current.position.ReadValue();

        return Vector2.zero;
    }

    bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        Vector2 screenPos = GetPointerScreenPosition();

        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = screenPos;

        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }
}