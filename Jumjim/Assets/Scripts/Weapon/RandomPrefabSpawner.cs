using UnityEngine;
using System.Collections.Generic;

public class RandomPrefabSpawner : MonoBehaviour, IInteractable
{
    [Header("Prefab Settings")]
    public GameObject[] prefabs; // Array of prefabs to choose from
    public GameObject portalPrefab;
    public Transform portalPoint;
    public Transform spawnPoint; // Where to spawn (optional)
    
    private GameObject weaponHolder;

    void Start()
    {
        // Find weapon holder at start
        weaponHolder = GameObject.FindGameObjectWithTag("WeaponHolder");
        if (weaponHolder == null)
        {
            Debug.LogWarning("No GameObject with tag 'WeaponHolder' found in scene!");
        }
    }

    public void SpawnRandomPrefab()
    {
        if (prefabs == null || prefabs.Length == 0)
        {
            Debug.LogWarning("No prefabs assigned!");
            return;
        }

        if (weaponHolder == null)
        {
            Debug.LogWarning("WeaponHolder not found!");
            return;
        }

        // Get all owned weapon names from weapon holder
        HashSet<string> ownedWeaponNames = GetOwnedWeaponNames();

        // Filter out prefabs that match any owned weapon's name
        List<GameObject> availablePrefabs = new List<GameObject>();
        foreach (GameObject prefab in prefabs)
        {
            // Get the weapon name from the prefab
            string prefabWeaponName = GetWeaponNameFromPrefab(prefab);
            
            if (!string.IsNullOrEmpty(prefabWeaponName) && !ownedWeaponNames.Contains(prefabWeaponName))
            {
                availablePrefabs.Add(prefab);
            }
        }

        if (availablePrefabs.Count == 0)
        {
            Debug.Log("All weapons already owned. Nothing to spawn.");
            return;
        }

        // Pick a random prefab from the filtered list
        int randomIndex = Random.Range(0, availablePrefabs.Count);
        GameObject prefabToSpawn = availablePrefabs[randomIndex];

        // Spawn position
        Vector3 position = spawnPoint != null ? spawnPoint.position : transform.position;
        Quaternion rotation = spawnPoint != null ? spawnPoint.rotation : Quaternion.identity;

        // Instantiate prefab
        Instantiate(prefabToSpawn, position, rotation);
    }

    private HashSet<string> GetOwnedWeaponNames()
    {
        HashSet<string> ownedNames = new HashSet<string>();
        
        // Get all child objects of weapon holder
        foreach (Transform child in weaponHolder.transform)
        {
            string weaponName = GetWeaponNameFromGameObject(child.gameObject);
            if (!string.IsNullOrEmpty(weaponName))
            {
                ownedNames.Add(weaponName);
            }
        }
        
        return ownedNames;
    }

    private string GetWeaponNameFromPrefab(GameObject prefab)
    {
        // Try to get weaponName from various possible components
        // You may need to adjust this based on your actual component structure
        
        // Option 1: If it's a direct component on the prefab
        MonoBehaviour weaponComponent = prefab.GetComponent<MonoBehaviour>();
        if (weaponComponent != null)
        {
            System.Reflection.FieldInfo field = weaponComponent.GetType().GetField("weaponName");
            if (field != null && field.FieldType == typeof(string))
            {
                return field.GetValue(weaponComponent) as string;
            }
        }
        
        // Option 2: Check all components on the prefab
        MonoBehaviour[] components = prefab.GetComponents<MonoBehaviour>();
        foreach (var component in components)
        {
            if (component != null)
            {
                System.Reflection.FieldInfo field = component.GetType().GetField("weaponName");
                if (field != null && field.FieldType == typeof(string))
                {
                    return field.GetValue(component) as string;
                }
            }
        }
        
        return null;
    }

    private string GetWeaponNameFromGameObject(GameObject weaponObject)
    {
        // Try to get weaponName from various possible components on the weapon object
        
        // Check all components on the weapon object
        MonoBehaviour[] components = weaponObject.GetComponents<MonoBehaviour>();
        foreach (var component in components)
        {
            if (component != null)
            {
                System.Reflection.FieldInfo field = component.GetType().GetField("weaponName");
                if (field != null && field.FieldType == typeof(string))
                {
                    return field.GetValue(component) as string;
                }
            }
        }
        
        return null;
    }

    public void Interact()
    {
        SpawnRandomPrefab();
        Instantiate(portalPrefab, portalPoint.position, portalPoint.rotation);
        Destroy(gameObject);
    }
}