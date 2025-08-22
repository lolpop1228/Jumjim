using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject[] enemyPrefabs;
    public GameObject normalPortalPrefab;
    public GameObject tressurePortalPrefab;
    public GameObject hardPortalPrefab;
    public GameObject violencePortalPrefab;
    public GameObject extremePortalPrefab;
    public GameObject brutalPortalPrefab;
    public GameObject endPortalPrefab;
    public Transform spawnCenter;
    public Transform portalSpawn;
    public float spawnRadius = 10f;
    public int enemiesToSpawn = 5;
    public float spawnDelay = 1f;
    public bool autoSpawnOnStart = true;
    public float minDistanceBetweenSpawns = 4f;

    [Header("Ground Detection")]
    public float spawnRayHeight = 10f;
    public float maxRaycastDistance = 20f;
    public LayerMask groundLayer;

    private List<Vector3> usedSpawnPoints = new List<Vector3>();
    private int prefabIndex = 0;
    private bool portalSpawned = false;
    private int deadEnemies = 0; // Track how many are dead

    void Start()
    {
        if (autoSpawnOnStart)
            StartCoroutine(SpawnEnemies());
    }

    IEnumerator SpawnEnemies()
    {
        int spawned = 0;
        int maxAttempts = enemiesToSpawn * 5;
        int attempts = 0;

        while (spawned < enemiesToSpawn && attempts < maxAttempts)
        {
            attempts++;

            Vector3 spawnPos;
            if (TryGetValidGroundPosition(out spawnPos) && IsFarEnough(spawnPos))
            {
                // Pick a random enemy instead of cycling evenly
                GameObject prefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

                Vector3 spawnAboveGround = spawnPos + Vector3.up * 1f;
                GameObject enemy = Instantiate(prefab, spawnAboveGround, Quaternion.identity);

                // Let EnemyHealth know who spawned it
                EnemyHealth health = enemy.GetComponent<EnemyHealth>();
                if (health != null)
                {
                    health.spawner = this;
                }

                usedSpawnPoints.Add(spawnPos);
                spawned++;

                yield return new WaitForSeconds(spawnDelay);
            }
        }
    }


    bool TryGetValidGroundPosition(out Vector3 groundPos)
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 rayStart = new Vector3(
            spawnCenter.position.x + randomCircle.x,
            spawnCenter.position.y + spawnRayHeight,
            spawnCenter.position.z + randomCircle.y
        );

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, maxRaycastDistance, groundLayer))
        {
            groundPos = hit.point;
            return true;
        }

        groundPos = Vector3.zero;
        return false;
    }

    bool IsFarEnough(Vector3 pos)
    {
        foreach (var used in usedSpawnPoints)
        {
            if (Vector3.Distance(pos, used) < minDistanceBetweenSpawns)
                return false;
        }
        return true;
    }

    public void OnEnemyDied()
    {
        deadEnemies++;
        Debug.Log($"Enemy died. Dead count = {deadEnemies}/{enemiesToSpawn}");

        if (!portalSpawned && deadEnemies >= enemiesToSpawn)
        {
            PlayerLevel playerLevel = FindObjectOfType<PlayerLevel>();
            playerLevel.AddLevel(1);
            Debug.Log("All enemies have been defeated!");

            int level = playerLevel.currentPlayerLevel;
            Debug.Log("Current Level: " + level);

            if (level < 5)
            {
                SpawnPortal(normalPortalPrefab);
            }
            else if (level == 5)
            {
                SpawnPortal(tressurePortalPrefab);
            }
            else if (level > 5 && level < 10)
            {
                SpawnPortal(hardPortalPrefab);
            }
            else if (level == 10)
            {
                SpawnPortal(tressurePortalPrefab);
            }
            else if (level > 10 && level < 15)
            {
                SpawnPortal(violencePortalPrefab);
            }
            else if (level == 15)
            {
                SpawnPortal(tressurePortalPrefab);
            }
            else if (level > 15 && level < 20)
            {
                SpawnPortal(extremePortalPrefab);
            }
            else if (level == 20)
            {
                SpawnPortal(tressurePortalPrefab);
            }
            else if (level > 20)
            {
                SpawnPortal(brutalPortalPrefab);
            }
            else if (level == 25)
            {
                SpawnPortal(endPortalPrefab);
            }

                portalSpawned = true;
        }
    }



    void SpawnPortal(GameObject portalPrefab)
    {
        if (portalPrefab != null && portalSpawn != null)
        {
            Instantiate(portalPrefab, portalSpawn.position, portalSpawn.rotation);
            Debug.Log(portalPrefab.name + " spawned!");
        }
        else
        {
            Debug.LogWarning("Portal prefab or portal spawn point is missing!");
        }
    }

    void OnDrawGizmosSelected()
    {
        if (spawnCenter != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spawnCenter.position, spawnRadius);
        }
    }
}
