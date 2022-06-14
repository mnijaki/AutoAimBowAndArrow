using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class InputReader : MonoBehaviour, GameInput.IGameplayActions
{
    // TODO: change to actions?
    public event UnityAction<Vector3> moveEvent = delegate { };
    public event UnityAction shootEvent = delegate { };  
    public event UnityAction<Vector2> cameraMoveEvent = delegate { };
    public bool Jumped { get; private set; }

    private GameInput gameInput;

    private void OnEnable()
    {
        if (gameInput == null)
        {
            gameInput = new GameInput();
            gameInput.Gameplay.SetCallbacks(this);
        }
    
        EnableGameplayInput();
    }
    
    private void OnDisable()
    {
        DisableAllInput();
    }
    
    public void EnableGameplayInput()
    {
        gameInput.Gameplay.Enable();
    }
    
    public void DisableAllInput()
    {
        gameInput.Gameplay.Disable();
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 movement2D = context.ReadValue<Vector2>();
        Vector3 movement3D = new(movement2D.x, 0.0F, movement2D.y);
        moveEvent.Invoke(movement3D);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        Jumped = context.action.triggered;
    }

    public void OnShoot(InputAction.CallbackContext context)
    {
        if (context.phase == InputActionPhase.Performed)
            shootEvent.Invoke();
    }

    public void OnCameraMove(InputAction.CallbackContext context)
    {
        cameraMoveEvent.Invoke(context.ReadValue<Vector2>());
    }
}
