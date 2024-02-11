using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    //Changeable
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private InputManager playerInputs;

    [Header("Parameters")]
    [SerializeField] [Range(0.01f, 10f)] private float sens = 0.8f;

    //Utility
    private float sensMultiplier = 10f;
    private Vector2 rotation;

    //Access
    public float Sens {  get { return sens; } }

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
        rotation.y += playerInputs.CameraAxis.x * Time.deltaTime * (sens * sensMultiplier);
        rotation.x -= playerInputs.CameraAxis.y * Time.deltaTime * (sens * sensMultiplier);

        rotation.x = Mathf.Clamp(rotation.x, -90f, 90f);

        transform.rotation = Quaternion.Euler(rotation.x, rotation.y, 0);
        player.rotation = Quaternion.Euler(0, rotation.y, 0);
    }

    #endregion

    #region Utility

    private void SetUpMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    #endregion
}
