using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject[] enemyPrefabs;
    public Transform spawnCenter;
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
                GameObject prefab = enemyPrefabs[prefabIndex];
                prefabIndex = (prefabIndex + 1) % enemyPrefabs.Length;

                Instantiate(prefab, spawnPos, Quaternion.identity);
                usedSpawnPoints.Add(spawnPos);
                spawned++;

                yield return new WaitForSeconds(spawnDelay);
            }
        }
    }

    bool TryGetValidGroundPosition(out Vector3 groundPos)
    {
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 rayStart = new Vector3(spawnCenter.position.x + randomCircle.x, spawnCenter.position.y + spawnRayHeight, spawnCenter.position.z + randomCircle.y);

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

    void OnDrawGizmosSelected()
    {
        if (spawnCenter != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(spawnCenter.position, spawnRadius);
        }
    }
}
