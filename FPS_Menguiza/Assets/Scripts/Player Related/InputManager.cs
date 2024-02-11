using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    //Utility
    private PlayerInput playerInput;
    private InputAction moveAction, lookAction, jumpAction, walkAction, crouchAction, shootAction, aimAction;

    //Access
    public Vector2 MoveAxis {  get; private set; }
    public Vector2 CameraAxis { get; private set; }
    public bool Jump {  get; private set; }
    public bool Walk { get; private set; }
    public bool Crouch {  get; private set; }
    public bool Shoot { get; private set; }
    public bool Aim { get; private set; }

    private void Awake()
    {
        //Get references
        playerInput = GetComponent<PlayerInput>();

        //Set Up
        SetUpInputActions();
    }

    void Update()
    {
        UpdateInputs();
    }

    #region Behavior

    private void UpdateInputs()
    {
        MoveAxis = moveAction.ReadValue<Vector2>();
        CameraAxis = lookAction.ReadValue<Vector2>();
        Jump = jumpAction.triggered;
        Walk = walkAction.IsPressed();
        Crouch = crouchAction.IsPressed();
        Shoot = shootAction.IsPressed();
        Aim = aimAction.IsPressed();
    }

    #endregion

    #region Utility

    private void SetUpInputActions()
    {
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        jumpAction = playerInput.actions["Jump"];
        walkAction = playerInput.actions["Walk"];
        crouchAction = playerInput.actions["Crouch"];
        shootAction = playerInput.actions["Shoot"];
        aimAction = playerInput.actions["Aim"];
    }

    #endregion
}
