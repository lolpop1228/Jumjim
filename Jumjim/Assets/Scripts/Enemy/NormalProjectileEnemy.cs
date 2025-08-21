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
    public float avoidDistance = 3f;
    public float avoidStrength = 10f;
    public float sphereRadius = 0.5f;
    public LayerMask obstacleLayer;

    [Header("Climbing")]
    public float climbCheckDistance = 1.2f;
    public float climbSpeed = 3f;

    [Header("Line of Sight")]
    public LayerMask visionBlockLayers; // walls or objects blocking shooting

    [Header("Animation")]
    public Animator animator;

    [Header("Audio")]
    public AudioClip chaseClip;
    public AudioClip shootClip;

    // ─────────────────────────────────────────────────────────────
    private Rigidbody rb;
    public AudioSource audioSource;
    private float attackTimer;
    private float wobbleTimer;
    private bool isClimbing;
    private bool isChasing;

    // ─────────────────────────────────────────────────────────────
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        
        audioSource.loop = true;
        audioSource.playOnAwake = false;

        // Auto-find player if not set
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
    }

    // ─────────────────────────────────────────────────────────────
    void Update()
    {
        if (player == null) return;

        attackTimer -= Time.deltaTime;
        float dist = Vector3.Distance(transform.position, player.position);
        bool shouldChase = dist > stopDistance;

        // Animation for movement
        if (animator != null)
        {
            animator.SetBool("isMoving", shouldChase);
        }

        // Handle chase audio
        if (shouldChase && !isChasing)
        {
            PlayChaseSound();
        }
        else if (!shouldChase && isChasing)
        {
            StopChaseSound();
        }

        // Shoot only if within range and has line of sight
        if (attackTimer <= 0f && dist <= stopDistance && CanShootPlayer())
        {
            ShootProjectile();
            attackTimer = attackCooldown;

            if (animator != null)
            {
                animator.SetTrigger("shoot");
            }
        }
    }

    // ─────────────────────────────────────────────────────────────
    void FixedUpdate()
    {
        if (player == null) return;

        Vector3 targetPos = new Vector3(player.position.x, rb.position.y, player.position.z);
        Vector3 dirToPlayer = (targetPos - rb.position).normalized;

        // ─ Check for climbable wall first
        Vector3 climbOrigin = transform.position + Vector3.up * 0.5f;
        isClimbing = Physics.Raycast(climbOrigin, dirToPlayer, climbCheckDistance, obstacleLayer);

        if (player.position.y - transform.position.y < 0.5f)
            isClimbing = false;

        if (isClimbing)
        {
            Vector3 climbDir = (Vector3.up + dirToPlayer).normalized;
            rb.MovePosition(rb.position + climbDir * climbSpeed * Time.fixedDeltaTime);

            // Face wall while climbing
            Vector3 look = new Vector3(dirToPlayer.x, 0f, dirToPlayer.z);
            if (look.sqrMagnitude > 0f)
                rb.MoveRotation(Quaternion.LookRotation(look));
            return; // Ignore everything else when climbing
        }

        // ─ Separation from other enemies
        Vector3 separation = Vector3.zero;
        foreach (Collider c in Physics.OverlapSphere(transform.position, separationRadius, enemyLayer))
        {
            if (c.gameObject == gameObject) continue;
            Vector3 away = transform.position - c.transform.position;
            float dist = away.magnitude;
            if (dist > 0f) separation += away.normalized / dist;
        }

        // ─ Wobble
        wobbleTimer += Time.fixedDeltaTime * wobbleSpeed;
        Vector3 right = Vector3.Cross(dirToPlayer, Vector3.up);
        float offset = Mathf.Sin(wobbleTimer) * wobbleAmount;

        // Base movement direction
        Vector3 moveDir = dirToPlayer + right * offset + separation * separationStrength;

        // ─ Obstacle avoidance
        Vector3 avoidance = Vector3.zero;
        Vector3 origin = transform.position + Vector3.up * 0.5f;

        Vector3[] castDirs =
        {
            moveDir.normalized,
            Quaternion.Euler(0f, 30f, 0f) * moveDir.normalized,
            Quaternion.Euler(0f, -30f, 0f) * moveDir.normalized
        };

        foreach (Vector3 d in castDirs)
        {
            if (Physics.SphereCast(origin, sphereRadius, d, out RaycastHit hit, avoidDistance, obstacleLayer))
            {
                Vector3 away = Vector3.Reflect(d, hit.normal);
                away.y = 0f;
                avoidance += away;
            }
        }

        if (avoidance.sqrMagnitude > 0f)
        {
            moveDir = Vector3.Lerp(moveDir, avoidance.normalized, avoidStrength * Time.fixedDeltaTime);
        }

        moveDir = moveDir.normalized;

        // ─ Move if outside stopping distance
        if (Vector3.Distance(rb.position, player.position) > stopDistance)
        {
            rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);
        }

        // ─ Face player
        Vector3 lookFinal = new Vector3(dirToPlayer.x, 0f, dirToPlayer.z);
        if (lookFinal.sqrMagnitude > 0f)
            rb.MoveRotation(Quaternion.LookRotation(lookFinal));
    }

    // ─────────────────────────────────────────────────────────────
    void ShootProjectile()
    {
        if (!projectilePrefab || !projectileSpawnPoint) return;

        GameObject proj = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        Vector3 dir = (player.position - projectileSpawnPoint.position).normalized;
        Rigidbody projRb = proj.GetComponent<Rigidbody>();

        if (projRb != null)
        {
            projRb.velocity = dir * projectileSpeed;
        }

        // Stop chase loop while shooting
        StopChaseSound();

        // Play shooting SFX once
        if (shootClip != null)
        {
            audioSource.PlayOneShot(shootClip);
        }
    }

    // ─────────────────────────────────────────────────────────────
    bool CanShootPlayer()
    {
        if (!player) return false;

        Vector3 origin = projectileSpawnPoint ? projectileSpawnPoint.position : transform.position + Vector3.up * 0.5f;
        Vector3 target = player.position + Vector3.up * 0.5f;
        Vector3 dir = (target - origin).normalized;

        if (Physics.Raycast(origin, dir, out RaycastHit hit, stopDistance, visionBlockLayers))
        {
            return hit.transform == player;
        }

        return true; // No obstacle, can shoot
    }

    // ─────────────────────────────────────────────────────────────
    void PlayChaseSound()
    {
        if (chaseClip != null)
        {
            audioSource.clip = chaseClip;
            audioSource.loop = true;
            audioSource.Play();
            isChasing = true;
        }
    }

    void StopChaseSound()
    {
        if (isChasing)
        {
            audioSource.Stop();
            isChasing = false;
        }
    }
}
