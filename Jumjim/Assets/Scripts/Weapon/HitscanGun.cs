using UnityEngine;

public class HitscanGun : MonoBehaviour
{
    [Header("Gun Settings")]
    public float fireRate = 0.2f;
    public float damage = 25f;
    public float range = 100f;
    public bool isAutomatic = true;

    [Header("Effects")]
    public ParticleSystem muzzleFlash;
    public GameObject hitEffectPrefab;
    public GameObject bulletTracerPrefab; // 🎯 New: LineRenderer prefab

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

    private float nextTimeToFire = 0f;

    void Start()
    {
        cameraRecoil.recoilSpeed = recoilSpeed;
        cameraRecoil.returnSpeed = returnSpeed;
        cameraRecoil.maxRecoil = maxRecoil;
        cameraRecoil.recoilDecay = recoilDecay;
        cameraRecoil.randomnessFactor = randomnessFactor;
        
        cameraRecoil.SetWeaponRecoil(recoilX, recoilY, recoilZ, 1.0f);
    }

    void Update()
    {
        bool shouldFire = isAutomatic ? Input.GetButton("Fire1") : Input.GetButtonDown("Fire1");

        if (shouldFire && Time.time >= nextTimeToFire)
        {
            nextTimeToFire = Time.time + fireRate;
            Fire();
        }
    }

    void Fire()
    {
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

            EnemyHealth enemy = hit.collider.GetComponent<EnemyHealth>();
            if (enemy != null)
                enemy.TakeDamage(damage);

            if (hitEffectPrefab != null)
            {
                GameObject impactGO = Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
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
}
