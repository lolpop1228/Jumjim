using TMPro;
using UnityEngine;

public class HitscanGun : MonoBehaviour
{
    [Header("Gun Settings")]
    public float fireRate = 0.2f;
    public float damage = 25f;
    public float range = 100f;
    public string weaponName = "";
    public bool isAutomatic = true;

    [Header("Effects")]
    public ParticleSystem muzzleFlash;
    public GameObject bulletTracerPrefab; // 🎯 New: LineRenderer prefab
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
    public int maxAmmo;
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

        cameraRecoil.recoilSpeed = recoilSpeed;
        cameraRecoil.returnSpeed = returnSpeed;
        cameraRecoil.maxRecoil = maxRecoil;
        cameraRecoil.recoilDecay = recoilDecay;
        cameraRecoil.randomnessFactor = randomnessFactor;

        cameraRecoil.SetWeaponRecoil(recoilX, recoilY, recoilZ, 1.0f);
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
        if (muzzleFlash != null)
        {
            muzzleFlash?.Play();
        }
        if (gunAudio != null)
        {
            gunAudio.PlayOneShot(fireSound);
        }

        if (cameraRecoil != null)
        {
            cameraRecoil.FireRecoil();
        }

        Vector3 origin = playerCam.transform.position;
        Vector3 direction = playerCam.transform.forward;
        Vector3 hitPoint = origin + direction * range;

        Ray ray = new Ray(origin, direction);
        if (Physics.Raycast(ray, out RaycastHit hit, range))
        {
            hitPoint = hit.point;

            GameObject hitObject = hit.collider.gameObject;

            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
                enemy.TakeDamage(damage);

            //spawn hit effect
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

        // 🎯 Spawn instant bullet tracer
        if (bulletTracerPrefab != null && firePoint != null)
        {
            GameObject tracer = Instantiate(bulletTracerPrefab);
            LineRenderer line = tracer.GetComponent<LineRenderer>();

            if (line != null)
            {
                line.SetPosition(0, firePoint.position);
                line.SetPosition(1, hitPoint);
                StartCoroutine(DestroyLineAfter(line, 0.05f)); // Adjust time for how long tracer stays visible
            }
        }
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
        UpdateAmmoUI();
    }

    void UpdateAmmoUI()
    {
        if (ammoText == null) return;

        if (infiniteAmmo)
        {
            ammoText.text = "∞";
        }
        else
        {
            ammoText.text = $"Ammo:{currentAmmo}";
        }
    }

}
