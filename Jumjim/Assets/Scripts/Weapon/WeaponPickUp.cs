using UnityEngine;

public class WeaponPickup : MonoBehaviour
{
    public GameObject weaponPrefab;
    public AudioClip pickupSound;

    void OnTriggerEnter(Collider other)
    {
        WeaponInventory inv = other.GetComponentInChildren<WeaponInventory>();
        if (inv != null)
        {
            inv.AddWeapon(weaponPrefab);

            if (pickupSound)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            Destroy(gameObject);
        }
    }
}
