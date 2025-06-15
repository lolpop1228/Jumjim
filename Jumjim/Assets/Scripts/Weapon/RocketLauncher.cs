using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RocketLauncher : MonoBehaviour
{
    public GameObject rocketPrefab;
    public Transform firePoint;
    public float launchForce = 1000f;
    public float fireRate = 1f;
    public string weaponName = "";

    public Animator fireAnimator;         // Reference to Animator
    public string fireTriggerName = "Fire"; // Name of the trigger in Animator

    public AudioSource audioSource;       // Reference to AudioSource
    public AudioClip fireSound;           // Sound to play on fire

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

    [Header("Muzzle Flash")]
    public ParticleSystem muzzleFlash;

    private float nextFireTime = 0f;

    void Start()
    {
        currentAmmo = maxAmmo;

        audioSource = GetComponentInParent<AudioSource>();
        cameraRecoil = transform.parent?.parent?.parent?.GetComponent<CameraRecoil>();

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
        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime && (infiniteAmmo || currentAmmo > 0))
        {
            FireRocket();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    void FireRocket()
    {
        Debug.Log($"Firing rocket at: {Time.time}");

        if (!infiniteAmmo)
        {
            currentAmmo--;
        }

        if (muzzleFlash != null)
        {
            muzzleFlash?.Play();
        }

        currentAmmo = Mathf.Max(0, currentAmmo);
        UpdateAmmoUI();

        if (cameraRecoil != null)
        {
            cameraRecoil.FireRecoil();
        }

        GameObject rocket = Instantiate(rocketPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = rocket.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(firePoint.forward * launchForce);
        }

        if (fireAnimator != null)
        {
            fireAnimator.SetTrigger(fireTriggerName);
        }

        if (audioSource != null && fireSound != null)
        {
            audioSource.PlayOneShot(fireSound);
        }
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
