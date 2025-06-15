using UnityEngine;

public class SkullCrusher : MonoBehaviour
{
    [Header("Gun Settings")]
    public int pelletsPerShot = 12;
    public float horizontalSpread = 15f;
    public float verticalSpread = 2f;
    public float fireRate = 1f;

    [Header("References")]
    public Transform firePoint;
    public GameObject pelletPrefab; // Assign your pellet projectile prefab
    public ParticleSystem muzzleFlash;

    [Header("Recoil")]
    public CameraRecoil cameraRecoil;
    public float recoilSpeed;
    public float returnSpeed;
    public float maxRecoil;
    public float recoilDecay;
    public float randomnessFactor;
    public float recoilX;
    public float recoilY;
    public float recoilZ;

    private float nextFireTime = 0f;

    void Start()
    {
        cameraRecoil = transform.parent?.parent?.parent?.GetComponent<CameraRecoil>();
        if (cameraRecoil != null)
        {
            cameraRecoil.recoilSpeed = recoilSpeed;
            cameraRecoil.returnSpeed = returnSpeed;
            cameraRecoil.maxRecoil = maxRecoil;
            cameraRecoil.recoilDecay = recoilDecay;
            cameraRecoil.randomnessFactor = randomnessFactor;

            cameraRecoil.SetWeaponRecoil(recoilX, recoilY, recoilZ, 1.0f);
        }
    }

    void OnEnable()
    {
        if (cameraRecoil != null)
        {
            cameraRecoil.recoilSpeed = recoilSpeed;
            cameraRecoil.returnSpeed = returnSpeed;
            cameraRecoil.maxRecoil = maxRecoil;
            cameraRecoil.recoilDecay = recoilDecay;
            cameraRecoil.randomnessFactor = randomnessFactor;

            cameraRecoil.SetWeaponRecoil(recoilX, recoilY, recoilZ, 1.0f);
        }
    }

    void Update()
    {
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    void Fire()
    {
        if (muzzleFlash) muzzleFlash.Play();
        if (cameraRecoil != null)
        {
            cameraRecoil.FireRecoil();
        }

        for (int i = 0; i < pelletsPerShot; i++)
        {
            Vector3 direction = GetHorizontalSpreadDirection();
            GameObject pellet = Instantiate(pelletPrefab, firePoint.position, Quaternion.LookRotation(direction));
        }
    }

    Vector3 GetHorizontalSpreadDirection()
    {
        float yaw = Random.Range(-horizontalSpread, horizontalSpread);
        float pitch = Random.Range(-verticalSpread, verticalSpread);
        Quaternion spreadRot = Quaternion.Euler(pitch, yaw, 0);
        return spreadRot * firePoint.forward;
    }
}
