using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public GameObject weaponPrefab;
    public AudioClip pickupSound;
    public float spinSpeed = 90f; // Degrees per second

    void Update()
    {
        transform.Rotate(Vector3.up, spinSpeed * Time.deltaTime, Space.World);
    }

    void OnTriggerEnter(Collider other)
    {
        WeaponInventory inv = other.GetComponentInChildren<WeaponInventory>();
        if (inv != null)
        {
            inv.AddWeapon(weaponPrefab);

            AudioSource audioSource = other.GetComponent<AudioSource>();
            {
                if (audioSource != null && pickupSound != null)
                {
                    audioSource.PlayOneShot(pickupSound);
                }
            }

            Destroy(gameObject);
        }
    }
}
