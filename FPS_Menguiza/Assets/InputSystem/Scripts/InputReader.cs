using System;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "NewInputReader", menuName = "InputReader", order = 0)]
public class InputReader : ScriptableObject, BasicControls.IStandardActions
{
    private BasicControls _basicControls;

    private void OnEnable()
    {
        if (_basicControls != null) return;

        _basicControls = new BasicControls();

        _basicControls.Standard.SetCallbacks(this);

        SetStandard();
    }

    public void SetStandard()
    {
        _basicControls.Standard.Enable();
    }

    public event Action<Vector2> MoveEvent;
    public event Action<Vector2> LookEvent;

    public event Action WalkEvent;
    public event Action WalkCancelledEvent;

    public event Action CrouchEvent;
    public event Action CrouchCancelledEvent;

    public event Action JumpEvent;

    public event Action AimEvent;
    public event Action AimCancelledEvent;

    public event Action ShootEvent;
    public event Action ShootCancelledEvent;

    public void OnMove(InputAction.CallbackContext context)
    {
        MoveEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnLook(InputAction.CallbackContext context)
    {
        LookEvent?.Invoke(context.ReadValue<Vector2>());
    }

    public void OnWalk(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                WalkEvent?.Invoke();
                break;
            case InputActionPhase.Canceled:
                WalkCancelledEvent?.Invoke(); 
                break;
        }
    }

    public void OnCrouch(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                CrouchEvent?.Invoke();
                break;
            case InputActionPhase.Canceled:
                CrouchCancelledEvent?.Invoke();
                break;
        }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                JumpEvent?.Invoke();
                break;
        }
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                AimEvent?.Invoke();
                break;
            case InputActionPhase.Canceled:
                AimCancelledEvent?.Invoke();
                break;
        }
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        switch (context.phase)
        {
            case InputActionPhase.Performed:
                ShootEvent?.Invoke();
                break;
            case InputActionPhase.Canceled:
                ShootCancelledEvent?.Invoke();
                break;
        }
    }
}
