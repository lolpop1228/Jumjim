using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NormalMeleeEnemy : MonoBehaviour
{
    [Header("Target")]
    public Transform player;
    public float moveSpeed = 3f;
    public float stopDistance = 2f; // melee range

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

    private Rigidbody rb;
    private float attackTimer;
    private float wobbleTimer;

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

    void Update()
    {
        attackTimer -= Time.deltaTime;

        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
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

        // ─ Separation
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

        Vector3 moveDir = dirToPlayer + right * offset + separation * separationStrength;

        // ─ Obstacle Avoidance with SphereCast
        Vector3 avoidance = Vector3.zero;
        Vector3 origin = transform.position + Vector3.up * 0.5f;

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

        // ─ Movement
        if (Vector3.Distance(rb.position, player.position) > stopDistance)
        {
            rb.MovePosition(rb.position + moveDir * moveSpeed * Time.fixedDeltaTime);
        }

        // ─ Facing
        Vector3 flatLookDir = new Vector3(dirToPlayer.x, 0f, dirToPlayer.z);
        if (flatLookDir.sqrMagnitude > 0f)
            rb.MoveRotation(Quaternion.LookRotation(flatLookDir));
    }

    void MeleeAttack()
    {
        // Placeholder: you could apply damage here
        Debug.Log("Melee Attack!");

        // Example: Check for player hit
        if (Vector3.Distance(transform.position, player.position) <= stopDistance + 0.5f)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }

        // You could add an animation, sound, or effect here
    }
}
