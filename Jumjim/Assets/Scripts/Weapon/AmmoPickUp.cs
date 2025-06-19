using UnityEngine;

public class AmmoPickup : MonoBehaviour
{
    public string[] targetWeaponNames; // Accepts multiple weapon names
    public int ammoAmount = 10;
    public float lifeTime = 10f;
    public float attractRange = 5f;
    public float moveSpeed = 10f;
    public AudioClip pickupSound;
    private Transform player;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        Destroy(transform.parent.gameObject, lifeTime);
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= attractRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.parent.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        WeaponInventory inventory = other.GetComponentInChildren<WeaponInventory>();
        if (inventory == null) return;

        bool ammoAdded = false;

        foreach (var weapon in inventory.GetOwnedWeapons())
        {
            string weaponName = "";
            System.Action<int> addAmmoMethod = null;

            if (weapon is HitscanGun hitscanGun)
            {
                weaponName = hitscanGun.weaponName;
                addAmmoMethod = hitscanGun.AddAmmo;
            }
            else if (weapon is Shotgun shotgun)
            {
                weaponName = shotgun.weaponName;
                addAmmoMethod = shotgun.AddAmmo;
            }
            else if (weapon is RocketLauncher rocketLauncher)
            {
                weaponName = rocketLauncher.weaponName;
                addAmmoMethod = rocketLauncher.AddAmmo;
            }
            else if (weapon is SkullCrusher skullCrusher)
            {
                weaponName = skullCrusher.weaponName;
                addAmmoMethod = skullCrusher.AddAmmo;
            }

            if (addAmmoMethod != null && System.Array.Exists(targetWeaponNames, name => name == weaponName))
            {
                addAmmoMethod.Invoke(ammoAmount);
                ammoAdded = true;
            }
        }

        if (ammoAdded)
        {
            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);

            Destroy(transform.parent.gameObject);
        }
    }
}
