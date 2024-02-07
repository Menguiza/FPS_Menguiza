using UnityEngine;

public enum PlayerState
{
    Walking, Running, Crouching
}

[RequireComponent (typeof(CharacterController))]
[RequireComponent(typeof(InputManager))]
public class PlayerMovement : MonoBehaviour
{
    //Changeable
    [Header("Movement")]
    [SerializeField][Range(0f, 100f)] private float walkSpeed = 2.0f;
    [SerializeField][Range(0f, 100f)] private float runSpeed = 5.0f;
    [SerializeField][Range(0f, 10f)] private float jumpHeight = 1.0f;
    [SerializeField][Range(0f, 3f)] private float jumpSmoothRatio = 3.0f;

    [Header("Parameters")]
    [SerializeField][Range(0.5f, 0.9f)] private float shrinkRatio = 0.75f;

    [Header("Physics")]
    [SerializeField][Range(-20f, 0f)] private float gravity = -9.81f;
    [SerializeField][Range(0f, 10f)] private float gravityMultiplier = 1.0f;

    [Header("References")]
    [SerializeField] private Transform camerHolder;

    //Utility
    private InputManager inputManager;
    private Vector3 playerVelocity;
    private sbyte minusOne = -1;
    private float currentSpeed = 0f, originalPlayerSize, originalPlayerCenter, originalCameraHolderPos;

    //Access
    public CharacterController CharacterController {  get; private set; }
    public bool Grounded {  get; private set; }
    public PlayerState PlayerState {  get; private set; }

    //Events

    private void Awake()
    {
        //Get references
        CharacterController = GetComponent<CharacterController>();
        inputManager = GetComponent<InputManager>();

        //Set Up
        currentSpeed = runSpeed;
        PlayerState = PlayerState.Running;

        originalPlayerSize = CharacterController.height;
        originalPlayerCenter = CharacterController.center.y;
        originalCameraHolderPos = camerHolder.position.y;
    }

    void Update()
    {
        //Physics
        ApplyGravity();

        //Behaviors
        Crouch();
        Walk();
        Move();
        Jump();

        //Movement
        ApplyMovement();
    }

    #region Behavior

    private void Move()
    {
        //Allocate info
        Vector2 moveAxis = inputManager.MoveAxis;

        //Determine move direction
        Vector3 moveDir = transform.right * moveAxis.x + transform.forward * moveAxis.y;

        //Apply values to velocity vector
        playerVelocity.x = moveDir.x * currentSpeed;
        playerVelocity.z = moveDir.z * currentSpeed;
    }

    private void Jump()
    {
        //Check jump input and grounded
        if(inputManager.Jump && Grounded)
        {
            playerVelocity.y += Mathf.Sqrt(jumpHeight * (minusOne * jumpSmoothRatio) * gravity); 
        }
    }

    private void Walk()
    {
        //Check if player crouching
        if (PlayerState == PlayerState.Crouching) return;

        //Check conditions for walk
        if(inputManager.Walk && Grounded)
        {
            //Check if it is already walking
            if(PlayerState != PlayerState.Walking)
            {
                PlayerState = PlayerState.Walking;
                currentSpeed = walkSpeed;
            }
        }
        else
        {
            //Check if it is walking
            if (PlayerState == PlayerState.Walking)
            {
                PlayerState = PlayerState.Running;
                currentSpeed = runSpeed;
            }
        }
    }

    private void Crouch()
    {
        //Check conditions for crouch
        if (inputManager.Crouch)
        {
            //Check if it is already crouching
            if (PlayerState != PlayerState.Crouching)
            {
                PlayerState = PlayerState.Crouching;
                currentSpeed = walkSpeed;

                ShrinkPlayerToggle();
            }
        }
        else
        {
            //Check if it is crouching
            if (PlayerState == PlayerState.Crouching)
            {
                PlayerState = PlayerState.Running;
                currentSpeed = runSpeed;

                ShrinkPlayerToggle();
            }
        }
    }

    #endregion

    #region Utility

    private void GroundCheck() 
    {
        //Is Player Grounded
        Grounded = CharacterController.isGrounded;
    }

    private void ApplyGravity()
    {
        //Checks
        GroundCheck();

        if (Grounded && playerVelocity.y < 0.0f)
        {
            //Reset vertical velocity
            playerVelocity.y = -1.0f;
        }
        else
        {
            //Apply gravity velocity
            playerVelocity.y += gravity * gravityMultiplier * Time.deltaTime;
        }
    }

    private void ApplyMovement()
    {
        //Move by character controller
        CharacterController.Move(playerVelocity * Time.deltaTime);
    }

    private void ShrinkPlayerToggle()
    {
        if(PlayerState == PlayerState.Crouching)
        {
            CharacterController.height *= shrinkRatio;
            CharacterController.center = new Vector3(CharacterController.center.x, CharacterController.center.y * shrinkRatio, CharacterController.center.z);
            camerHolder.position = new Vector3(camerHolder.position.x, camerHolder.position.y * shrinkRatio, camerHolder.position.z);
        }
        else
        {
            CharacterController.height = originalPlayerSize;
            CharacterController.center = new Vector3(CharacterController.center.x, originalPlayerCenter, CharacterController.center.z);
            camerHolder.position = new Vector3(camerHolder.position.x, originalCameraHolderPos, camerHolder.position.z);
        }
    }

    #endregion
}
