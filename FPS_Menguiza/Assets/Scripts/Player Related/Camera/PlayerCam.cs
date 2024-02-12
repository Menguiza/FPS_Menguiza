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
    private float sensMultiplier = 10f, localYRot, lastMovedRot;
    private Vector2 rotation;
    public bool shooting;

    //Access
    public float Sens {  get { return sens; } }

    public Vector2 gunRecoil;

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

        if (gunRecoil != Vector2.zero)
        {
            rotation.x -= gunRecoil.x;
            localYRot += gunRecoil.y;
        }

        rotation.x = Mathf.Clamp(rotation.x, -90f, 90f);

        if (playerInputs.CameraAxis.y * Time.deltaTime * (sens * sensMultiplier) != 0)
        {
            lastMovedRot = rotation.x;
        }

        Recenter();

        transform.localRotation = Quaternion.Euler(rotation.x, localYRot, 0);
        player.rotation = Quaternion.Euler(0, rotation.y, 0);
    }

    #endregion

    #region Utility

    private void Recenter()
    {
        if (shooting) return;

        localYRot = Mathf.LerpAngle(localYRot, 0, 0.05f);
        rotation.x = Mathf.LerpAngle(rotation.x, lastMovedRot, 0.05f);
    }

    private void SetUpMouse()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    #endregion
}
