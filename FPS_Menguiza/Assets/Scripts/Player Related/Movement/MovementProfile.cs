using UnityEngine;

[CreateAssetMenu(fileName = "NewMovementProfile", menuName = "MovementProfile", order = 0)]
public class MovementProfile : ScriptableObject
{
    [Header("Parameters")]
    [SerializeField][Range(0f, 100f)] private float crouchSpeed = 2.0f;
    [SerializeField][Range(0f, 100f)] private float walkSpeed = 3.0f;
    [SerializeField][Range(0f, 100f)] private float runSpeed = 6.0f;
    [SerializeField][Range(0f, 100f)] private float airSpeed = 7.0f;
    [SerializeField][Range(0f, 10f)] private float jumpHeight = 1.0f;
    [SerializeField][Range(0f, 3f)] private float jumpSmoothRatio = 3.0f;

    public float CrouchSpeed => crouchSpeed;
    public float WalkSpeed => walkSpeed;
    public float RunSpeed => runSpeed;
    public float AirSpeed => airSpeed;
    public float JumpHeight => jumpHeight;
    public float JumpSmoothRatio => jumpSmoothRatio;
}
