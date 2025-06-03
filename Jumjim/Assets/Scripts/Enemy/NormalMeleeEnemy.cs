using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class NormalMeleeEnemy : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 10f;
    public float attackRange = 2f;
    public float preAttackRange = 3f;
    public float attackCooldown = 1.5f;
    public int attackDamage = 10;

    public float flankOffset = 2f;
    public float offsetChangeInterval = 3f;

    public float separationRadius = 3f;
    public float separationStrength = 5f;
    public float minSeparationDistance = 1.5f;
    public int maxNearbyEnemies = 3; // Limit how many enemies can be near player

    private Transform player;
    private Rigidbody rb;

    private float lastAttackTime = -Mathf.Infinity;
    private float nextOffsetChangeTime = 0f;
    private Vector3 lateralOffset = Vector3.zero;
    private Vector3 personalOffset; // Unique offset for this enemy

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        // Give each enemy a unique personal offset to prevent identical positioning
        personalOffset = new Vector3(
            Random.Range(-1f, 1f),
            0,
            Random.Range(-1f, 1f)
        ).normalized * Random.Range(0.5f, 1.5f);
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        
        // Check if too many enemies are already close to player
        if (distance > attackRange && ShouldWaitTurn())
        {
            CircleAroundPlayer();
            return;
        }

        if (distance <= attackRange)
        {
            // Face the player directly when in attack range
            FacePlayer();
            TryAttackPlayer();
            return;
        }

        MoveWithOffsetAndSeparation();
    }

    bool ShouldWaitTurn()
    {
        Collider[] nearPlayer = Physics.OverlapSphere(player.position, preAttackRange);
        int enemiesNearPlayer = 0;
        
        foreach (var col in nearPlayer)
        {
            if (col.CompareTag("Enemy") && col.gameObject != gameObject)
            {
                enemiesNearPlayer++;
            }
        }
        
        return enemiesNearPlayer >= maxNearbyEnemies;
    }

    void CircleAroundPlayer()
    {
        // Make excess enemies circle around at a distance
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0;
        
        Vector3 circlePosition = player.position + (toPlayer.normalized + personalOffset).normalized * (preAttackRange + 1f);
        Vector3 moveDirection = (circlePosition - transform.position).normalized;
        
        // Add some perpendicular movement for circling
        Vector3 perpendicular = Vector3.Cross(moveDirection, Vector3.up);
        moveDirection += perpendicular * 0.3f;
        
        Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
        Quaternion smoothRotation = Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(smoothRotation);

        Vector3 move = moveDirection * (moveSpeed * 0.7f) * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }

    void FacePlayer()
    {
        Vector3 toPlayer = (player.position - transform.position);
        toPlayer.y = 0; // Keep rotation on horizontal plane only
        
        if (toPlayer.magnitude > 0.01f) // Avoid issues when very close
        {
            Quaternion targetRotation = Quaternion.LookRotation(toPlayer.normalized);
            Quaternion smoothRotation = Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
            rb.MoveRotation(smoothRotation);
        }
    }

    void MoveWithOffsetAndSeparation()
    {
        Vector3 toPlayer = player.position - transform.position;
        float distanceToPlayer = toPlayer.magnitude;
        toPlayer.y = 0;
        Vector3 forwardDir = toPlayer.normalized;

        // Only apply lateral offset when outside of pre-attack range
        Vector3 offset = Vector3.zero;
        if (distanceToPlayer > preAttackRange + 0.5f) // Add a small buffer
        {
            if (Time.time > nextOffsetChangeTime)
            {
                int side = Random.Range(0, 3) - 1; // -1, 0, 1
                lateralOffset = Quaternion.Euler(0, 90 * side, 0) * forwardDir * flankOffset;
                nextOffsetChangeTime = Time.time + offsetChangeInterval;
            }
            offset = lateralOffset + personalOffset;
        }
        else
        {
            // Use personal offset when close to prevent identical paths
            offset = personalOffset * 0.5f;
        }

        // Enhanced separation force - always apply but stronger when close
        Vector3 separation = CalculateSeparation(distanceToPlayer);

        Vector3 desiredDirection = (toPlayer + offset + separation).normalized;

        Quaternion targetRotation = Quaternion.LookRotation(desiredDirection);
        Quaternion smoothRotation = Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(smoothRotation);

        Vector3 move = rb.transform.forward * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
    }

    Vector3 CalculateSeparation(float distanceToPlayer)
    {
        Vector3 separation = Vector3.zero;
        Collider[] nearby = Physics.OverlapSphere(transform.position, separationRadius);
        
        foreach (var col in nearby)
        {
            if (col.gameObject != gameObject && col.CompareTag("Enemy"))
            {
                Vector3 away = transform.position - col.transform.position;
                float distance = away.magnitude;
                
                if (distance < minSeparationDistance)
                {
                    // Much stronger separation when too close
                    float strength = separationStrength * 3f * (minSeparationDistance - distance);
                    separation += away.normalized * strength;
                }
                else if (distance > 0.01f)
                {
                    // Normal separation
                    float strength = separationStrength / distance;
                    separation += away.normalized * strength;
                }
            }
        }
        
        // Reduce separation when very close to player (but don't disable completely)
        if (distanceToPlayer <= preAttackRange + 0.5f)
        {
            separation *= 0.3f;
        }
        
        return separation;
    }

    void TryAttackPlayer()
    {
        if (Time.time - lastAttackTime < attackCooldown) return;

        lastAttackTime = Time.time;

        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
        if (playerHealth != null)
            playerHealth.TakeDamage(attackDamage);
    }
}