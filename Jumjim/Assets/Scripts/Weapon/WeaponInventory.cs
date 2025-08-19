using System.Collections.Generic;
using UnityEngine;

public class WeaponInventory : MonoBehaviour
{
    public Transform weaponHolder;
    private List<MonoBehaviour> ownedWeapons = new();
    private int currentWeaponIndex = 0;

    public void AddWeapon(GameObject weaponPrefab)
    {
        // Check if already owned by prefab name (most efficient check)
        string weaponName = weaponPrefab.name;
        for (int i = 0; i < ownedWeapons.Count; i++)
        {
            if (ownedWeapons[i].name == weaponName)
                return; // Already owned
        }

        // Instantiate
        GameObject weaponGO = Instantiate(weaponPrefab, weaponHolder);

        // Try to get weapon component (order by most common first for efficiency)
        MonoBehaviour weapon = weaponGO.GetComponent<HitscanGun>();
        if (weapon == null)
            weapon = weaponGO.GetComponent<Shotgun>();
        if (weapon == null)
            weapon = weaponGO.GetComponent<RocketLauncher>();
        if (weapon == null)
            weapon = weaponGO.GetComponent<SkullCrusher>();

        if (weapon != null)
        {
            weapon.enabled = false;
            weaponGO.SetActive(false);
            ownedWeapons.Add(weapon);
            EquipWeapon(ownedWeapons.Count - 1);
        }
        else
        {
            // Clean up if no valid weapon component found
            Destroy(weaponGO);
        }
    }

    public void EquipWeapon(int index)
    {
        if (index < 0 || index >= ownedWeapons.Count) return;

        // Disable current weapon first (avoid having multiple active)
        if (currentWeaponIndex < ownedWeapons.Count)
        {
            ownedWeapons[currentWeaponIndex].enabled = false;
            ownedWeapons[currentWeaponIndex].gameObject.SetActive(false);
        }

        // Enable new weapon
        ownedWeapons[index].gameObject.SetActive(true);
        ownedWeapons[index].enabled = true;
        currentWeaponIndex = index;
    }

    void Update()
    {
        if (ownedWeapons.Count == 0) return;

        // --- Mouse scroll wheel ---
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f) // scroll up
        {
            int nextIndex = (currentWeaponIndex + 1) % ownedWeapons.Count;
            EquipWeapon(nextIndex);
        }
        else if (scroll < 0f) // scroll down
        {
            int prevIndex = (currentWeaponIndex - 1 + ownedWeapons.Count) % ownedWeapons.Count;
            EquipWeapon(prevIndex);
        }

        // --- Number keys (1, 2, 3, ...) ---
        for (int i = 0; i < ownedWeapons.Count; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                EquipWeapon(i);
            }
        }
    }


    public List<MonoBehaviour> GetOwnedWeapons()
    {
        return ownedWeapons;
    }

    // Helper method to get weapon of specific type efficiently
    public T GetCurrentWeapon<T>() where T : MonoBehaviour
    {
        if (currentWeaponIndex >= 0 && currentWeaponIndex < ownedWeapons.Count)
            return ownedWeapons[currentWeaponIndex] as T;
        return null;
    }
}