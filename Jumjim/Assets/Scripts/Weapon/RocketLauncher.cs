using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
    public GameObject rocketPrefab;
    public Transform firePoint;
    public float launchForce = 1000f;
    public float fireRate = 1f;

    private float nextFireTime = 0f;

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime)
        {
            FireRocket();
            nextFireTime = Time.time + 1f / fireRate;
        }
    }

    void FireRocket()
    {
        GameObject rocket = Instantiate(rocketPrefab, firePoint.position, firePoint.rotation);
        Rigidbody rb = rocket.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(firePoint.forward * launchForce);
        }
    }
}
