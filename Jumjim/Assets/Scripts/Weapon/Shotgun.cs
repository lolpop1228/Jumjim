using TMPro;
using UnityEngine;

public class Shotgun : MonoBehaviour
{
    [Header("Gun Settings")]
    public float fireRate = 0.8f;
    public float damage = 10f;
    public float range = 100f;
    public int pelletCount = 8;
    public float spreadAngle = 5f;
    public bool isAutomatic = false;
    public string weaponName = "";

    [Header("Shotgun Jump")]
    public float jumpForce = 20f;
    public float minAngleForJump = 45f; // Minimum downward angle to trigger jump
    public float maxJumpDistance = 5f; // Maximum distance to ground for jump to work
    public LayerMask groundLayerMask = -1; // What counts as ground for shotgun jumping
    public bool enableShotgunJump = true;

    [Header("Effects")]
    public ParticleSystem muzzleFlash;
    public GameObject bulletTracerPrefab;
    public GameObject enemyHitEffectPrefab;
    public GameObject wallHitEffectPrefab;
    public GameObject defaultHitEffectPrefab;

    public AudioSource gunAudio;
    public AudioClip fireSound;
    public AudioClip shotgunJumpSound; // Optional separate sound for shotgun jumps
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
    public int maxAmmo = 20;
    public int currentAmmo = 20;
    public bool infiniteAmmo = false;
    public TextMeshProUGUI ammoText;

    private float nextTimeToFire = 0f;
    private PlayerMovement playerMovement;

    void Start()
    {
        currentAmmo = maxAmmo;

        gunAudio = GetComponentInParent<AudioSource>();
        cameraRecoil = transform.parent?.parent?.parent?.GetComponent<CameraRecoil>();
        playerCam = transform.parent?.parent?.GetComponent<Camera>();
        
        // Find PlayerMovement component
        playerMovement = GetComponentInParent<PlayerMovement>();
        if (playerMovement == null)
        {
            // Try to find it in the scene if not found in parent
            playerMovement = FindObjectOfType<PlayerMovement>();
        }

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

        // Check for shotgun jump before firing effects
        bool performedShotgunJump = CheckAndPerformShotgunJump();

        gunAnimator?.SetTrigger("Fire");
        muzzleFlash?.Play();
        
        // Play appropriate sound
        AudioClip soundToPlay = (performedShotgunJump && shotgunJumpSound != null) ? shotgunJumpSound : fireSound;
        gunAudio?.PlayOneShot(soundToPlay);
        
        cameraRecoil?.FireRecoil();

        for (int i = 0; i < pelletCount; i++)
        {
            Vector3 origin = playerCam.transform.position;
            Vector3 direction = GetSpreadDirection();
            Ray ray = new Ray(origin, direction);
            Vector3 hitPoint;

            if (Physics.Raycast(ray, out RaycastHit hit, range))
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
                    selectedEffect = enemyHitEffectPrefab;
                else if (hitObject.CompareTag("Wall"))
                    selectedEffect = wallHitEffectPrefab;
                else if (hitObject.CompareTag("Ground"))
                    selectedEffect = defaultHitEffectPrefab;

                if (selectedEffect != null)
                {
                    GameObject impactGO = Instantiate(selectedEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(impactGO, 1f);
                }
            }
            else
            {
                hitPoint = origin + direction * range;
            }

            // Always spawn tracer
            if (bulletTracerPrefab != null && firePoint != null)
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
        }
    }

    bool CheckAndPerformShotgunJump()
    {
        if (!enableShotgunJump || playerMovement == null)
            return false;

        // Check if player is looking down enough
        float downwardAngle = Vector3.Angle(playerCam.transform.forward, Vector3.down);
        if (downwardAngle > minAngleForJump)
            return false;

        // Raycast to check if there's ground below within jump distance
        Vector3 rayOrigin = playerCam.transform.position;
        Vector3 rayDirection = playerCam.transform.forward;
        
        if (Physics.Raycast(rayOrigin, rayDirection, out RaycastHit hit, maxJumpDistance, groundLayerMask))
        {
            // Check if the hit surface is relatively horizontal (ground-like)
            float surfaceAngle = Vector3.Angle(hit.normal, Vector3.up);
            if (surfaceAngle < 45f) // Surface is relatively flat
            {
                // Perform the shotgun jump
                ApplyShotgunJump();
                return true;
            }
        }

        return false;
    }

    void ApplyShotgunJump()
    {
        if (playerMovement == null)
            return;

        // Apply upward velocity
        float currentYVelocity = playerMovement.GetYVelocity();
        float newYVelocity = Mathf.Max(currentYVelocity, 0f) + jumpForce;
        playerMovement.SetYVelocity(newYVelocity);

        // Optional: Add some backward momentum to make it feel more realistic
        Vector3 horizontalForward = Vector3.ProjectOnPlane(playerCam.transform.forward, Vector3.up).normalized;
        Vector3 backwardForce = -horizontalForward * (jumpForce * 0.3f);
        
        // You might want to add this to PlayerMovement script if you want horizontal momentum
        // For now, we'll just do the vertical jump
        
        Debug.Log("Shotgun Jump performed!");
    }

    Vector3 GetSpreadDirection()
    {
        float angle = spreadAngle;
        Vector3 direction = playerCam.transform.forward;
        direction += Random.insideUnitSphere * Mathf.Tan(angle * Mathf.Deg2Rad);
        return direction.normalized;
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

        if (infiniteAmmo)
        {
            ammoText.text = "∞";
        }
        else
        {
            ammoText.text = $"Ammo: {currentAmmo}";
        }
    }
}