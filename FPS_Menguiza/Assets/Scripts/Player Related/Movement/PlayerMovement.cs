using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public enum PlayerState
{
    Walking, Running, Crouching
}

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(InputManager))]
public class PlayerMovement : MonoBehaviour
{
    //Changeable
    [Header("Movement")]
    [SerializeField][Range(0f, 100f)] private float crouchSpeed = 2.0f;
    [SerializeField][Range(0f, 100f)] private float walkSpeed = 3.0f;
    [SerializeField][Range(0f, 100f)] private float runSpeed = 6.0f;
    [SerializeField][Range(0f, 10f)] private float jumpHeight = 1.0f;
    [SerializeField][Range(0f, 3f)] private float jumpSmoothRatio = 3.0f;

    [Header("Parameters")]
    [SerializeField][Range(0.5f, 0.9f)] private float shrinkRatio = 0.75f;
    [SerializeField][Range(0f, 100f)] private float shrinkSpeed = 10f;
    [SerializeField][Range(0.01f, 0.1f)] private float headCollisionOffset = 0.05f;
    [SerializeField] private LayerMask headCheckLayers;
    [SerializeField] private Transform headCheckers;

    [Header("Physics")]
    [SerializeField][Range(-20f, 0f)] private float gravity = -9.81f;
    [SerializeField][Range(0f, 10f)] private float gravityMultiplier = 1.0f;

    [Header("References")]
    [SerializeField] private Transform camerHolder;

    //Utility
    private InputManager inputManager;
    private Vector3 playerVelocity;
    private sbyte minusOne = -1;
    private float currentSpeed = 0f, originalPlayerSize, distance;
    private bool doneCrouching = true;

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
    }

    void Update()
    {
        //Physics
        ApplyGravity();

        //Movement Handlers
        Crouch();
        Walk();
        Move();
        Jump();

        //Behaviors
        ShrinkPlayerToggle();
        ShrinkCheck();

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
                currentSpeed = crouchSpeed;

                doneCrouching = false;
            }
        }
        else
        {
            //Check if it is crouching
            if (PlayerState == PlayerState.Crouching)
            {
                PlayerState = PlayerState.Running;

                doneCrouching = false;
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
        //Check if crouch process done
        if (doneCrouching) return;

        //Check state
        if(PlayerState == PlayerState.Crouching)
        {
            //Lerp height
            UpdatePlayerHeight(originalPlayerSize * shrinkRatio, true);

            //Not exact values fix
            if (Math.Round(CharacterController.height, 1) <= originalPlayerSize * shrinkRatio)
            {
                UpdatePlayerHeight(originalPlayerSize * shrinkRatio, false);

                doneCrouching = true;
            }
        }
        else
        {
            //Check if distance store some value
            if(distance != 0f)
            {
                //Lerp height until head offset
                if (distance - CharacterController.height / 2 > headCollisionOffset)
                {
                    UpdatePlayerHeight((originalPlayerSize * shrinkRatio) + distance, true);

                    return;
                }
            }
            else
            {
                //Stand player
                UpdatePlayerHeight(originalPlayerSize, true);
            }

            //Not exact values fix
            if (Math.Round(CharacterController.height, 1) >= originalPlayerSize)
            {
                UpdatePlayerHeight(originalPlayerSize, false);

                currentSpeed = runSpeed;
                doneCrouching = true;
            }
        }
    }

    private void ShrinkCheck()
    {
        //Check if player is shrinked
        if (CharacterController.height != originalPlayerSize)
        {
            //Local variable
            RaycastHit hit;

            //Reset data variable
            distance = 0f;

            //Loop checkers
            foreach (Transform checker in headCheckers)
            {
                //Check for any collision detected
                if (Physics.Raycast(checker.position, checker.up, out hit, originalPlayerSize, headCheckLayers))
                {
                    //Check for first collision
                    if (distance == 0) distance = hit.distance;
                    else if (distance > hit.distance && hit.distance != 0) distance = hit.distance;
                }
            }
        }
    }

    private void UpdatePlayerHeight(float newHeight, bool lerp)
    {
        //Lerp mode condition
        if(lerp) CharacterController.height = Mathf.Lerp(CharacterController.height, newHeight, shrinkSpeed * Time.deltaTime);
        else CharacterController.height = newHeight;

        //Re position CC center and camera
        CharacterController.center = new Vector3(CharacterController.center.x, CharacterController.height / 2, CharacterController.center.z);
        camerHolder.position = new Vector3(camerHolder.position.x, (CharacterController.height / 2) + 0.5f, camerHolder.position.z);
    }

    #endregion

#if UNITY_EDITOR

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;

        foreach (Transform checker in headCheckers)
        {
            Gizmos.DrawRay(checker.position, checker.up * 2);
        }
    }

#endif
}
