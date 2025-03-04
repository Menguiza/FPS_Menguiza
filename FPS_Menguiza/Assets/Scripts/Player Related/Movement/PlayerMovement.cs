using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public enum PlayerState
{
    Running, 
    Walking, 
    Crouching
}

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    //Editor Variables
    [Header("REFERENCES")]
    [Header("Input")]
    [SerializeField] private InputReader inputReader;
    [Header("Camera")]
    [SerializeField] private Transform camerHolder;
    [Space]
    [Header("PARAMETERS")]
    [Header("Profile")]
    [SerializeField] private MovementProfile movementProfile;

    [Header("Shrink")]
    [SerializeField][Range(0.5f, 0.9f)] private float shrinkRatio = 0.75f;
    [SerializeField][Range(0f, 100f)] private float shrinkSpeed = 10f;

    [Header("Collisions")]
    [SerializeField][Range(0.01f, 0.5f)] private float headCollisionOffset = 0.05f;
    [SerializeField] private Transform headCheckers;
    [SerializeField] private LayerMask headCheckLayers;

    [Header("Physics")]
    [SerializeField][Range(-20f, 0f)] private float gravity = -9.81f;
    [SerializeField][Range(0f, 10f)] private float gravityMultiplier = 1.0f;

    //Utility Variables
    private Vector3 _playerVelocity;

    private float _currentSpeed;
    private float _originalPlayerSize;
    private float _distance;

    private bool _doneCrouching = true;

    //Input Variables
    private Vector2 _moveAxis;
    private bool _walking, _crouching;

    #region InputHandlers
    private void HandleMove(Vector2 moveAxis) => this._moveAxis = moveAxis;

    private void HandleWalk() => _walking = true;
    private void HandleWalkCancelled() => _walking = false;

    private void HandleCrouch() => _crouching = true;
    private void HandleCrouchCancelled() => _crouching = false;
    #endregion

    //Accessors
    public CharacterController CC {  get; private set; }
    public bool Grounded {  get; private set; }
    public PlayerState PlayerState { get; private set; }

    //Events

#if UNITY_EDITOR
    private void OnValidate()
    {
        if(!CC)
        {
            //Get Character Controller reference
            CC = GetComponent<CharacterController>();
        }
    }
#endif

    private void Awake()
    {
#if UNITY_STANDALONE
        //Get Character Controller reference
        CC = GetComponent<CharacterController>();
#endif
        //Set Up
        SetPlayerState(PlayerState.Running);
        _originalPlayerSize = CC.height;

        //Bind Inputs
        inputReader.MoveEvent += HandleMove;

        inputReader.JumpEvent += Jump;

        inputReader.WalkEvent += HandleWalk;
        inputReader.WalkCancelledEvent += HandleWalkCancelled;

        inputReader.CrouchEvent += HandleCrouch;
        inputReader.CrouchCancelledEvent += HandleCrouchCancelled;
    }

    void Update()
    {
        //Grounded
        GroundCheck();

        //Physics
        ApplyGravity();

        //Movement
        Crouch();
        Walk();
        Move();

        //Shrink
        ShrinkCheck();
        ShrinkPlayerToggle();

        //Displacement
        ApplyMovement();
    }

    #region Behavior

    private void Move()
    {
        //Determine move direction
        Vector3 moveDir = transform.right * _moveAxis.x + transform.forward * _moveAxis.y;

        //Apply values to velocity vector
        _playerVelocity.x = moveDir.x * _currentSpeed;
        _playerVelocity.z = moveDir.z * _currentSpeed;
    }

    private void Jump()
    {
        //Check grounded
        if(Grounded)
        {
            _playerVelocity.y += Mathf.Sqrt(movementProfile.JumpHeight * (-movementProfile.JumpSmoothRatio) * gravity); 
        }
    }

    private void Walk()
    {
        //If player crouching
        if (PlayerState == PlayerState.Crouching) return;

        //If trying to walk, set state
        if (_walking && Grounded && PlayerState != PlayerState.Walking)
        {
            SetPlayerState(PlayerState.Walking);
        }
        // If trying to stop walking
        else if ((!_walking || !Grounded) && PlayerState == PlayerState.Walking)
        {
            SetPlayerState(PlayerState.Running);
        }
    }

    private void Crouch()
    {
        // If already crouching and still wants to, do nothing
        if (_crouching && PlayerState == PlayerState.Crouching) return;

        // If trying to crouch, set state
        if (_crouching)
        {
            SetPlayerState(PlayerState.Crouching);
            _doneCrouching = false;
        }
        // If trying to stand up, first check if there's space
        else if (PlayerState == PlayerState.Crouching)
        {
            if (_distance == 0f || (_distance - (_originalPlayerSize * shrinkRatio)) >= headCollisionOffset)
            {
                SetPlayerState(PlayerState.Running);
            }

            _doneCrouching = false;
        }
    }

    #endregion

    #region Utility

    private void GroundCheck() 
    {
        //Is Player Grounded
        Grounded = CC.isGrounded;
    }

    private void ApplyGravity()
    {
        if (Grounded)
        {
            _playerVelocity.y = Mathf.Max(_playerVelocity.y, -0.3f);
        }
        else
        {
            //Apply gravity velocity
            _playerVelocity.y += gravity * gravityMultiplier * Time.deltaTime;
        }
    }

    private void ApplyMovement()
    {
        //Move by character controller
        CC.Move(_playerVelocity * Time.deltaTime);
    }

    private void SetPlayerState(PlayerState newState)
    {
        PlayerState = newState;

        switch (PlayerState)
        {
            case PlayerState.Running:
                _currentSpeed = movementProfile.RunSpeed;
                break;
            case PlayerState.Walking:
                if(Grounded) _currentSpeed = movementProfile.WalkSpeed;
                else StartCoroutine(RestoreSpeed());
                break;
            case PlayerState.Crouching:
                if (Grounded) _currentSpeed = movementProfile.CrouchSpeed;
                else StartCoroutine(RestoreSpeed());
                break;
        }
    }

    private void ShrinkPlayerToggle()
    {
        //Check if crouch process done
        if (_doneCrouching) return;

        //Check state
        if (PlayerState == PlayerState.Crouching)
        {
            //Check for input
            if(_crouching)
            {
                //Lerp height
                AdjustPlayerHeight(_originalPlayerSize * shrinkRatio, true);

                //Not exact values fix
                if (Math.Round(CC.height, 2) <= _originalPlayerSize * shrinkRatio)
                {
                    AdjustPlayerHeight(_originalPlayerSize * shrinkRatio, false);
                    _doneCrouching = true;
                }

                return;
            }

            // Check if distance has been stored and provides enough clearance
            if (_distance != 0f)
            {
                float fixedDistance = (float)Math.Round((_distance - headCollisionOffset), 2, MidpointRounding.ToEven);

                AdjustPlayerHeight((_originalPlayerSize * shrinkRatio) + fixedDistance, true);
                return;
            }
        }

        // Stand up normally
        AdjustPlayerHeight(_originalPlayerSize, true);

        // Fix for exact height
        if (Math.Round(CC.height, 2) >= _originalPlayerSize)
        {
            AdjustPlayerHeight(_originalPlayerSize, false);
            SetPlayerState(PlayerState.Running);
            _doneCrouching = true;
        }
    }

    private void ShrinkCheck()
    {
        //Check if player is shrinked
        if (CC.height == _originalPlayerSize) return;

        //Reset data variable
        _distance = 0f;

        //Local variable
        RaycastHit hit;

        //Loop checkers
        foreach (Transform checker in headCheckers)
        {
            //Check for any collision detected
            if (Physics.Raycast(checker.position, checker.up, out hit, _originalPlayerSize, headCheckLayers))
            {
                _distance = _distance == 0 ? hit.distance : Mathf.Min(_distance, hit.distance);
            }
        }
    }

    private void AdjustPlayerHeight(float newHeight, bool lerp)
    {
        //Lerp mode condition
        CC.height = lerp ? Mathf.Lerp(CC.height, newHeight, shrinkSpeed * Time.deltaTime) : newHeight;

        //Re position CC center and camera
        CC.center = Vector3.up * (CC.height/2);

        camerHolder.localPosition = Vector3.up * (CC.center.y + 0.5f);
    }

    private IEnumerator RestoreSpeed()
    {
        yield return new WaitUntil(() => Grounded);

        SetPlayerState(PlayerState);
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