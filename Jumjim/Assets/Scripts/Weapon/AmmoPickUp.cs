using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public string targetWeaponName; // Name of the weapon to add ammo to
    public int ammoAmount = 10;
    public AudioClip pickupSound;

    void OnTriggerEnter(Collider other)
    {
        WeaponInventory inventory = other.GetComponentInChildren<WeaponInventory>();
        if (inventory == null) return;

        bool ammoAdded = false;

        foreach (var weapon in inventory.GetOwnedWeapons())
        {
            string weaponName = "";
            bool canAddAmmo = false;

            // Check if it's a HitscanGun
            if (weapon is HitscanGun hitscanGun)
            {
                weaponName = hitscanGun.weaponName;
                if (weaponName == targetWeaponName)
                {
                    hitscanGun.AddAmmo(ammoAmount);
                    canAddAmmo = true;
                }
            }
            // Check if it's a Shotgun
            else if (weapon is Shotgun shotgun)
            {
                weaponName = shotgun.weaponName;
                if (weaponName == targetWeaponName)
                {
                    shotgun.AddAmmo(ammoAmount);
                    canAddAmmo = true;
                }
            }

            if (canAddAmmo)
            {
                ammoAdded = true;
                break; // Stop after adding to one weapon
            }
        }

        // Only play sound and destroy if ammo was actually added
        if (ammoAdded)
        {
            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            Destroy(gameObject);
        }
    }
}