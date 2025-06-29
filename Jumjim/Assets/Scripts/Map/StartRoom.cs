using UnityEngine;

public class StartRoom : MonoBehaviour
{
    public GameObject portalPrefab;               // The portal to spawn
    public Transform[] portalSpawnPoints;         // List of positions to spawn portals

    void Start()
    {
        SpawnPortals();
    }

    void SpawnPortals()
    {
        if (portalPrefab == null || portalSpawnPoints == null) return;

        foreach (Transform spawnPoint in portalSpawnPoints)
        {
            if (spawnPoint != null)
            {
                Instantiate(portalPrefab, spawnPoint.position, spawnPoint.rotation);
            }
        }
    }
}
