using UnityEngine;

[RequireComponent (typeof(PlayerCam))]
public class Shooting : MonoBehaviour
{
    public PlayerMovement playerMovement;
    public GameObject sphere;
    public Transform player;
    public float spread = 5, timeSpreadDir = 1f, lateralSpread;
    public AnimationCurve curve;
    private Vector2 spreadDir = Vector2.up;
    private float timer, waveTimer, fireTimer = 0;
    private int count = 0;
    private bool printed;
    private PlayerCam cam;

    private void Start()
    {
        cam = GetComponent<PlayerCam>();
        timer = timeSpreadDir;
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Shoot();
        }

        if(Input.GetMouseButtonUp(0))
        {
            timer = timeSpreadDir;
            waveTimer = 0;
            fireTimer = 0;
            spreadDir = Vector2.up;
            cam.gunRecoil = Vector2.zero;
            count = 0;
            printed = false;
            cam.shooting = false;
        }
    }

    public void Shoot()
    {
        cam.shooting = true;

        fireTimer -= Time.deltaTime;
        timer -= Time.deltaTime;

        if (fireTimer <= 0)
        {
            if(playerMovement.PlayerVelocity.magnitude > 1)
            {
                
            }
            else
            {
                if (timer <= 0)
                {
                    if (!printed)
                    {
                        print(count);
                        printed = true;
                    }

                    float newRecoilX = Random.Range(-spread * 0.2f, spread * 0.2f);
                    float newRecoilY = curve.Evaluate(waveTimer) * spread;

                    // Smoothly interpolate between the current and new recoil values
                    cam.gunRecoil.x = Mathf.Lerp(cam.gunRecoil.x, newRecoilX, 0.05f);
                    cam.gunRecoil.y = Mathf.Lerp(cam.gunRecoil.y, newRecoilY, 0.05f);

                    waveTimer += 1 / 9.75f;
                }
                else
                {
                    float newRecoilX = spread * 0.40f;
                    float newRecoilY = Random.Range(-spread / 2, spread / 2);

                    // Smoothly interpolate between the current and new recoil values
                    cam.gunRecoil.x = Mathf.Lerp(cam.gunRecoil.x, newRecoilX, 0.05f);
                    cam.gunRecoil.y = Mathf.Lerp(cam.gunRecoil.y, newRecoilY, 0.05f);
                }
            }

            RaycastHit hit;

            if (Physics.Raycast(transform.position, transform.forward, out hit, 100f))
            {
                Instantiate(sphere, hit.point, Quaternion.identity);
            }

            fireTimer = 1 / 9.75f;

            count++;
        }
        else
        {
            // Smoothly return to zero recoil
            cam.gunRecoil.x = Mathf.Lerp(cam.gunRecoil.x, 0f, 0.1f);
            cam.gunRecoil.y = Mathf.Lerp(cam.gunRecoil.y, 0f, 0.1f);
        }

        if (waveTimer >= 1) waveTimer = 0;
    }

    #region Utility

    private static float WrapAngle(float angle)
    {
        angle %= 360;
        if (angle > 180)
            return angle - 360;

        return angle;
    }

    #endregion
}
