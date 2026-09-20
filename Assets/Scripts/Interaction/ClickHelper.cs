using UnityEngine;
using UnityEngine.InputSystem;

// Click (o tap táctil) sobre un objeto del mundo y sus hijos.
// Uso: ClickHelper.WasClickedOn(transform) — respeta colliders sólidos y triggers.
public static class ClickHelper
{
    public static bool WasClickedOn(Transform root, float maxDistance = 1000f)
    {
        if (root == null || !WasClickPressed())
            return false;

        var cam = Camera.main;
        if (cam == null)
            return false;

        Ray ray = cam.ScreenPointToRay(PointerPosition());
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide))
        {
            var t = hit.transform;
            if (t == root || t.IsChildOf(root))
                return true;
        }
        return false;
    }

    public static bool WasClickPressed()
    {
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
            return true;

        var touch = Touchscreen.current;
        if (touch != null && touch.primaryTouch.press.wasPressedThisFrame)
            return true;

        return false;
    }

    static Vector2 PointerPosition()
    {
        var mouse = Mouse.current;
        if (mouse != null)
            return mouse.position.ReadValue();

        var touch = Touchscreen.current;
        if (touch != null)
            return touch.primaryTouch.position.ReadValue();

        return Vector2.zero;
    }
}
