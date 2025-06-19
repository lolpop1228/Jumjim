using TMPro;
using UnityEngine;

public class SkullCrusher : MonoBehaviour
{
    [Header("Gun Settings")]
    public float fireRate = 0.5f;
    public float damage = 10f;
    public float range = 100f;
    public string weaponName = "";
    public bool isAutomatic = false;

    [Header("Spread Shot Settings")]
    public int rayCount = 20;
    public float spreadAngle = 45f; // Horizontal arc

    [Header("Effects")]
    public ParticleSystem muzzleFlash;
    public GameObject bulletTracerPrefab;
    public GameObject enemyHitEffectPrefab;
    public GameObject wallHitEffectPrefab;
    public GameObject defaultHitEffectPrefab;

    public AudioSource gunAudio;
    public AudioClip fireSound;
    public Animator gunAnimator;

    [Header("References")]
    public Transform firePoint;
    public Camera playerCam;

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

    [Header("Ammo")]
    public int maxAmmo = 10;
    public int currentAmmo;
    public bool infiniteAmmo = false;
    public TextMeshProUGUI ammoText;

    private float nextTimeToFire = 0f;

    void Start()
    {
        currentAmmo = maxAmmo;

        gunAudio = GetComponentInParent<AudioSource>();
        cameraRecoil = transform.parent?.parent?.parent?.GetComponent<CameraRecoil>();
        playerCam = transform.parent?.parent?.GetComponent<Camera>();

        GameObject ammoUI = GameObject.FindWithTag("AmmoUI");
        if (ammoUI != null)
        {
            ammoText = ammoUI.GetComponent<TextMeshProUGUI>();
        }
        UpdateAmmoUI();

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
        UpdateAmmoUI();

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
        bool shouldFire = isAutomatic ? Input.GetButton("Fire1") : Input.GetButtonDown("Fire1");

        if (shouldFire && Time.time >= nextTimeToFire && (infiniteAmmo || currentAmmo > 0))
        {
            nextTimeToFire = Time.time + fireRate;
            Fire();
        }
    }

    void Fire()
    {
        if (!infiniteAmmo)
        {
            currentAmmo--;
        }
        currentAmmo = Mathf.Max(0, currentAmmo);
        UpdateAmmoUI();

        gunAnimator?.SetTrigger("Fire");
        muzzleFlash?.Play();
        gunAudio?.PlayOneShot(fireSound);
        cameraRecoil?.FireRecoil();

        for (int i = 0; i < rayCount; i++)
        {
            Vector3 direction = GetHorizontalSpreadDirection(playerCam.transform.forward, spreadAngle);
            Vector3 origin = playerCam.transform.position;
            Vector3 hitPoint = origin + direction * range;

            if (Physics.Raycast(origin, direction, out RaycastHit hit, range))
            {
                hitPoint = hit.point;

                GameObject hitObject = hit.collider.gameObject;
                EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
                if (enemy != null)
                {
                    enemy.TakeDamage(damage);
                }

                GameObject selectedEffect = null;
                if (hitObject.CompareTag("Enemy"))
                {
                    selectedEffect = enemyHitEffectPrefab;
                }
                else if (hitObject.CompareTag("Wall"))
                {
                    selectedEffect = wallHitEffectPrefab;
                }
                else if (hitObject.CompareTag("Ground"))
                {
                    selectedEffect = defaultHitEffectPrefab;
                }

                if (selectedEffect != null)
                {
                    GameObject impactGO = Instantiate(selectedEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impactGO, 1f);
                }
            }

            // Show tracer for first few rays only
            if (bulletTracerPrefab != null && firePoint != null && i < 5)
            {
                GameObject tracer = Instantiate(bulletTracerPrefab);
                LineRenderer line = tracer.GetComponent<LineRenderer>();
                if (line != null)
                {
                    line.SetPosition(0, firePoint.position);
                    line.SetPosition(1, hitPoint);
                    StartCoroutine(DestroyLineAfter(line, 0.05f));
                }
            }

            // Debug Ray (optional)
            // Debug.DrawRay(origin, direction * range, Color.red, 1f);
        }
    }

    Vector3 GetHorizontalSpreadDirection(Vector3 forward, float angle)
    {
        // Only apply yaw (horizontal) spread
        float randomYaw = Random.Range(-angle / 2f, angle / 2f);
        Quaternion rotation = Quaternion.Euler(0f, randomYaw, 0f); // Y-axis only
        return rotation * forward;
    }

    System.Collections.IEnumerator DestroyLineAfter(LineRenderer line, float duration)
    {
        yield return new WaitForSeconds(duration);
        if (line != null)
            Destroy(line.gameObject);
    }

    public void AddAmmo(int amount)
    {
        currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
        if (gameObject.activeInHierarchy)
        {
            UpdateAmmoUI();
        }
    }

    void UpdateAmmoUI()
    {
        if (ammoText == null) return;

        ammoText.text = infiniteAmmo ? "∞" : $"Ammo:{currentAmmo}";
    }
}
