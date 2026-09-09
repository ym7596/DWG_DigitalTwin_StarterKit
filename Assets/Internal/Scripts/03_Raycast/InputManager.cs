using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private RaycastHit _hit;
    
    public Vector2 Position { get; private set; }

    public event Action<RaycastHit> OnAction_RaycastHit;

    public void LeftClickAction(InputAction.CallbackContext ctx)
    {
        var phase = ctx.phase;

        switch (phase)
        {
            case InputActionPhase.Performed:
            {
                bool isUITouch = IsUITouch(Position);
                if (isUITouch == true)
                    return;
                /*
                bool hasHit = GetRayHit(Position, out _hit);
                if (hasHit == true)
                {
                    Debug.Log($"RaycastHit: {_hit.collider.gameObject.name}");
                    OnAction_RaycastHit?.Invoke(_hit);
                }
                */
                
                break;
            }
            default:
                break;
        }
    }
    
    public void MousePosition(InputAction.CallbackContext ctx)
    {
        Position = ctx.ReadValue<Vector2>();
    }
    
    /*private bool GetRayHit(Vector2 pos, out RaycastHit hit)
    {
        hit = default;
        if (Camera.main != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(pos);
            if (Physics.Raycast(ray, out hit))
            {
                return true;
            }
        }

        return false;
    }*/
    
    private bool IsUITouch(Vector2 point)
    {
        PointerEventData pointData = new PointerEventData(EventSystem.current)
        {
            position = point
        };

        List<RaycastResult> raycastResults = new List<RaycastResult>();
        
        EventSystem.current.RaycastAll(pointData, raycastResults);
        if (raycastResults.Count > 0)
        {
            foreach (var r in raycastResults)
            {
                if (r.gameObject.layer == LayerMask.NameToLayer("UI"))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
