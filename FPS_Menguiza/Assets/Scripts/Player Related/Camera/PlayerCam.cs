using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    //Changeable
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private InputReader inputReader;

    [Header("Parameters")]
    [SerializeField] [Range(0.01f, 10f)] private float sens = 0.8f;

    //Utility
    private float sensMultiplier = 10f;
    private Vector2 rotation;

    private Vector2 lookAxis;

    private void HandleLook(Vector2 lookAxis) => this.lookAxis = lookAxis;

    //Access
    public float Sens {  get { return sens; } }

    private void Awake()
    {
        inputReader.LookEvent += HandleLook;
    }

    void Start()
    {
        SetUpMouse();
    }

    void Update()
    {
        RotationByInputs();
    }

    #region Behavior

    private void RotationByInputs()
    {
        rotation.y += lookAxis.x * Time.deltaTime * (sens * sensMultiplier);
        rotation.x -= lookAxis.y * Time.deltaTime * (sens * sensMultiplier);

        rotation.x = Mathf.Clamp(rotation.x, -90f, 90f);

        transform.rotation = Quaternion.Euler(rotation.x, rotation.y, 0);
        player.rotation = Quaternion.Euler(0, rotation.y, 0);
    }

    #endregion

    #region Utility

    private void SetUpMouse()
    {
        //Cursor.lockState = CursorLockMode.Locked;
        //Cursor.visible = false;
    }

    #endregion
}
