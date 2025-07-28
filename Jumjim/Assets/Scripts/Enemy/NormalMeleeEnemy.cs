using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NormalMeleeEnemy : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public float moveSpeed = 3f;
    public float stopDistance = 2f;

    [Header("Attack")]
    public float attackCooldown = 1f;
    public int damage = 10;

    [Header("Separation")]
    public float separationRadius = 2.5f;
    public float separationStrength = 2f;
    public LayerMask enemyLayer;

    [Header("Wobble")]
    public float wobbleAmount = 0.5f;
    public float wobbleSpeed = 2f;

    [Header("Obstacle Avoidance")]
    public float avoidDistance = 2.5f;
    public float avoidStrength = 8f;
    public float sphereRadius = 0.5f;
    public LayerMask obstacleLayer;

    [Header("Climbing")]
    public float climbCheckDistance = 1.2f;
    public float climbSpeed = 3f;

    [Header("Animation")]
    public Animator animator;

    [Header("Ground Detection")]
    public LayerMask groundLayer;
    public float rayDistance = 3f;

    private Rigidbody rb;
    private float attackTimer;
    private float wobbleTimer;
    private Vector3 groundNormal = Vector3.up;
    private bool isClimbing;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

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

    void Update()
    {
        attackTimer -= Time.deltaTime;
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        animator.SetBool("isMoving", dist > stopDistance);

        if (dist <= stopDistance && attackTimer <= 0f)
        {
            MeleeAttack();
            attackTimer = attackCooldown;
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;

        Vector3 targetPos = new Vector3(player.position.x, rb.position.y, player.position.z);
        Vector3 dirToPlayer = (targetPos - rb.position).normalized;
        Vector3 climbOrigin = transform.position + Vector3.up * 0.5f;

        // ---- Wall Climbing Check ----
        if (Physics.Raycast(climbOrigin, dirToPlayer, out RaycastHit climbHit, climbCheckDistance, obstacleLayer))
        {
            isClimbing = true;
            Debug.Log("Climbing Triggered! Hit: " + climbHit.collider.name);
            Debug.DrawRay(climbOrigin, dirToPlayer * climbCheckDistance, Color.green);
        }
        else
        {
            isClimbing = false;
            Debug.DrawRay(climbOrigin, dirToPlayer * climbCheckDistance, Color.red);
        }

        // ---- PRIORITIZE CLIMBING ----
        if (isClimbing)
        {
            // Move straight forward and up, ignoring everything else
            Vector3 climbDirection = (Vector3.up + dirToPlayer).normalized;
            rb.MovePosition(rb.position + climbDirection * climbSpeed * Time.fixedDeltaTime);

            // Face the wall/player direction
            Vector3 flatLookDir = new Vector3(dirToPlayer.x, 0f, dirToPlayer.z);
            if (flatLookDir.sqrMagnitude > 0f)
                rb.MoveRotation(Quaternion.LookRotation(flatLookDir));

            return; // Skip all other logic
        }

        // ---- NORMAL MOVEMENT (if not climbing) ----
        Vector3 separation = Vector3.zero;
        foreach (Collider c in Physics.OverlapSphere(transform.position, separationRadius, enemyLayer))
        {
            if (c.gameObject == gameObject) continue;
            Vector3 away = transform.position - c.transform.position;
            float dist = away.magnitude;
            if (dist > 0f) separation += away.normalized / dist;
        }

        wobbleTimer += Time.fixedDeltaTime * wobbleSpeed;
        Vector3 right = Vector3.Cross(dirToPlayer, Vector3.up);
        float offset = Mathf.Sin(wobbleTimer) * wobbleAmount;

        Vector3 moveDir = dirToPlayer + right * offset + separation * separationStrength;

        // Obstacle Avoidance
        Vector3 avoidance = Vector3.zero;
        Vector3[] castDirs =
        {
            moveDir.normalized,
            Quaternion.Euler(0f, 30f, 0f) * moveDir.normalized,
            Quaternion.Euler(0f,-30f, 0f) * moveDir.normalized
        };

        foreach (Vector3 d in castDirs)
        {
            if (Physics.SphereCast(climbOrigin, sphereRadius, d, out RaycastHit hit, avoidDistance, obstacleLayer))
            {
                Vector3 away = Vector3.Reflect(d, hit.normal);
                away.y = 0f;
                avoidance += away;
                Debug.DrawRay(climbOrigin, d * avoidDistance, Color.yellow);
            }
        }

        if (avoidance.sqrMagnitude > 0f)
        {
            moveDir = Vector3.Lerp(moveDir, avoidance.normalized, avoidStrength * Time.fixedDeltaTime);
        }

        // Ground Slope Detection
        Vector3 rayOrigin = transform.position + Vector3.up * 1f;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit groundHit, rayDistance, groundLayer))
        {
            groundNormal = groundHit.normal;
            Debug.DrawRay(rayOrigin, Vector3.down * rayDistance, Color.blue);
        }

        moveDir = Vector3.ProjectOnPlane(moveDir, groundNormal).normalized;

        // Movement
        if (Vector3.Distance(rb.position, player.position) > stopDistance)
        {
            rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);
        }

        // Face Player
        Vector3 flatLookDirFinal = new Vector3(dirToPlayer.x, 0f, dirToPlayer.z);
        if (flatLookDirFinal.sqrMagnitude > 0f)
            rb.MoveRotation(Quaternion.LookRotation(flatLookDirFinal));
    }

    void MeleeAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("attack");
        }

        if (Vector3.Distance(transform.position, player.position) <= stopDistance + 0.5f)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }
    }
}
