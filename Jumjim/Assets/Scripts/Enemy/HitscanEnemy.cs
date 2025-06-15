using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HitscanEnemy : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public float moveSpeed = 3f;
    public float stopDistance = 10f;

    [Header("Shooting")]
    public int damage = 5;
    public float fireRate = 2f;
    public float raycastRange = 100f;
    public LayerMask hitLayers;
    public Transform shootOrigin;
    public LineRenderer bulletTrailPrefab;
    public GameObject impactEffectPrefab;

    [Header("Obstacle Avoidance")]
    public float avoidDistance = 3f;
    public float sphereRadius = 0.5f;
    public float avoidStrength = 10f;
    public LayerMask obstacleLayer;

    [Header("Separation")]
    public float separationRadius = 2.5f;
    public float separationStrength = 2f;
    public LayerMask enemyLayer;


    private Rigidbody rb;
    private float fireTimer;

    // ─────────────────────────────────────────────────────────────
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    // ─────────────────────────────────────────────────────────────
    void Update()
    {
        if (player == null) return;

        fireTimer -= Time.deltaTime;
        if (fireTimer <= 0f && Vector3.Distance(transform.position, player.position) <= stopDistance)
        {
            FireRaycast();
            fireTimer = fireRate;
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector3 targetPos = new Vector3(player.position.x, rb.position.y, player.position.z);
        Vector3 direction = (targetPos - rb.position).normalized;

        // ─ Separation
        Vector3 separation = Vector3.zero;
        foreach (Collider c in Physics.OverlapSphere(transform.position, separationRadius, enemyLayer))
        {
            if (c.gameObject == gameObject) continue;
            Vector3 away = transform.position - c.transform.position;
            float dist = away.magnitude;
            if (dist > 0f) separation += away.normalized / dist;
        }

        // ─ Obstacle avoidance
        Vector3 avoidance = Vector3.zero;
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        if (Physics.SphereCast(origin, sphereRadius, direction, out RaycastHit hit, avoidDistance, obstacleLayer))
        {
            Vector3 away = Vector3.Reflect(direction, hit.normal);
            away.y = 0f;
            avoidance = away.normalized;
        }

        Vector3 finalDir = direction + separation * separationStrength;

        if (avoidance != Vector3.zero)
            finalDir = Vector3.Lerp(finalDir, avoidance, avoidStrength * Time.fixedDeltaTime);

        finalDir = finalDir.normalized;

        if (Vector3.Distance(rb.position, player.position) > stopDistance)
        {
            rb.MovePosition(rb.position + finalDir * moveSpeed * Time.fixedDeltaTime);
        }

        // Face the player
        Vector3 look = new Vector3(direction.x, 0f, direction.z);
        if (look.sqrMagnitude > 0f)
            rb.MoveRotation(Quaternion.LookRotation(look));
    }

    void FireRaycast()
    {
        if (!shootOrigin || !player) return;

        Vector3 origin = shootOrigin.position;
        Vector3 direction = (player.position - origin).normalized;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, raycastRange, hitLayers))
        {
            // Apply damage here, e.g., hit.collider.GetComponent<Health>()?.TakeDamage();
            PlayerHealth ph = hit.collider.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                ph.TakeDamage(damage);
            }

            if (impactEffectPrefab)
                {
                    Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                }

            if (bulletTrailPrefab)
            {
                StartCoroutine(SpawnTrail(origin, hit.point));
            }
        }
        else
        {
            if (bulletTrailPrefab)
            {
                StartCoroutine(SpawnTrail(origin, origin + direction * raycastRange));
            }
        }
    }

    IEnumerator SpawnTrail(Vector3 start, Vector3 end)
    {
        LineRenderer trail = Instantiate(bulletTrailPrefab);
        trail.SetPosition(0, start);
        trail.SetPosition(1, start);

        float duration = 0.05f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            trail.SetPosition(1, Vector3.Lerp(start, end, elapsed / duration));
            yield return null;
        }

        trail.SetPosition(1, end);
        Destroy(trail.gameObject, 0.1f);
    }
}
