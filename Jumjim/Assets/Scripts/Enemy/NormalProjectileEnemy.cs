using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NormalProjectileEnemy : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public float moveSpeed = 3f;
    public float stopDistance = 10f;

    [Header("Projectile")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float projectileSpeed = 10f;
    public float attackCooldown = 2f;

    [Header("Separation")]
    public float separationRadius = 2.5f;
    public float separationStrength = 2f;
    public LayerMask enemyLayer;

    [Header("Wobble")]
    public float wobbleAmount = 0.5f;
    public float wobbleSpeed = 2f;

    [Header("Obstacle Avoidance (SphereCast)")]
    public float avoidDistance = 3f;      // how far ahead to probe
    public float avoidStrength = 10f;     // steering weight
    public float sphereRadius = 0.5f;     // radius of the cast
    public LayerMask obstacleLayer;       // walls / props, etc.

    // ─────────────────────────────────────────────────────────────

    private Rigidbody rb;
    private float attackTimer;
    private float wobbleTimer;

    // ─────────────────────────────────────────────────────────────
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;   // keeps the enemy upright

        // Auto-find player if not set
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    // ─────────────────────────────────────────────────────────────
    void Update()
    {
        // Handle firing in Update (better timing resolution)
        if (player == null) return;

        attackTimer -= Time.deltaTime;
        if (attackTimer <= 0f && Vector3.Distance(transform.position, player.position) <= stopDistance)
        {
            ShootProjectile();
            attackTimer = attackCooldown;
        }
    }

    // ─────────────────────────────────────────────────────────────
    void FixedUpdate()
    {
        if (player == null) return;

        // ── 1.  Direction to player (horizontal only)
        Vector3 targetPos   = new Vector3(player.position.x, rb.position.y, player.position.z);
        Vector3 dirToPlayer = (targetPos - rb.position).normalized;

        // ── 2.  Separation from other enemies
        Vector3 separation = Vector3.zero;
        foreach (Collider c in Physics.OverlapSphere(transform.position, separationRadius, enemyLayer))
        {
            if (c.gameObject == gameObject) continue;
            Vector3 away = transform.position - c.transform.position;
            float dist   = away.magnitude;
            if (dist > 0f) separation += away.normalized / dist;
        }

        // ── 3.  Wobble offset
        wobbleTimer += Time.fixedDeltaTime * wobbleSpeed;
        Vector3 right  = Vector3.Cross(dirToPlayer, Vector3.up);
        float   offset = Mathf.Sin(wobbleTimer) * wobbleAmount;

        // Base desired direction (before avoidance)
        Vector3 moveDir = dirToPlayer + right * offset + separation * separationStrength;

        // ── 4.  SphereCast obstacle avoidance
        Vector3 avoidance = Vector3.zero;
        Vector3 origin    = transform.position + Vector3.up * 0.5f;   // slightly above ground

        Vector3[] castDirs =
        {
            moveDir.normalized,
            Quaternion.Euler(0f, 30f, 0f) * moveDir.normalized,
            Quaternion.Euler(0f,-30f, 0f) * moveDir.normalized
        };

        foreach (Vector3 d in castDirs)
        {
            if (Physics.SphereCast(origin, sphereRadius, d, out RaycastHit hit, avoidDistance, obstacleLayer))
            {
                // Reflect away from obstacle surface and accumulate
                Vector3 away = Vector3.Reflect(d, hit.normal);
                away.y = 0f;                         // keep horizontal
                avoidance += away;
            }
        }

        if (avoidance.sqrMagnitude > 0f)
        {
            // Blend original direction with avoidance
            moveDir = Vector3.Lerp(moveDir, avoidance.normalized, avoidStrength * Time.fixedDeltaTime);
        }

        moveDir = moveDir.normalized;

        // ── 5.  Move the Rigidbody if outside stopping radius
        if (Vector3.Distance(rb.position, player.position) > stopDistance)
        {
            rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);
        }

        // ── 6.  Face the player horizontally
        Vector3 look = new Vector3(dirToPlayer.x, 0f, dirToPlayer.z);
        if (look.sqrMagnitude > 0f)
            rb.MoveRotation(Quaternion.LookRotation(look));
    }

    // ─────────────────────────────────────────────────────────────
    void ShootProjectile()
    {
        if (!projectilePrefab || !projectileSpawnPoint) return;

        GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        Vector3 dir = (player.position - projectileSpawnPoint.position).normalized;
    }
}
