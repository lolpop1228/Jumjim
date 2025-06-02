using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class NormalMeleeEnemy : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float turnSpeed = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public int attackDamage = 10;

    public float flankOffset = 2f;
    public float offsetChangeInterval = 3f;

    public float separationRadius = 2f;
    public float separationStrength = 2f;

    private Transform player;
    private Rigidbody rb;

    private float lastAttackTime = -Mathf.Infinity;
    private float nextOffsetChangeTime = 0f;
    private Vector3 lateralOffset = Vector3.zero;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float distance = Vector3.Distance(player.position, transform.position);
        if (distance <= attackRange)
        {
            TryAttackPlayer();
            return;
        }

        MoveWithOffsetAndSeparation();
    }

    void MoveWithOffsetAndSeparation()
    {
        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0;
        Vector3 forwardDir = toPlayer.normalized;

        // Randomize lateral offset occasionally
        if (Time.time > nextOffsetChangeTime)
        {
            int side = Random.Range(0, 3) - 1; // -1, 0, 1
            lateralOffset = Quaternion.Euler(0, 90 * side, 0) * forwardDir * flankOffset;
            nextOffsetChangeTime = Time.time + offsetChangeInterval;
        }

        // Separation force
        Vector3 separation = Vector3.zero;
        Collider[] nearby = Physics.OverlapSphere(transform.position, separationRadius);
        foreach (var col in nearby)
        {
            if (col.gameObject != gameObject && col.CompareTag("Enemy"))
            {
                Vector3 away = transform.position - col.transform.position;
                separation += away.normalized / away.magnitude;
            }
        }

        Vector3 desiredDirection = (toPlayer + lateralOffset + separation * separationStrength).normalized;

        Quaternion targetRotation = Quaternion.LookRotation(desiredDirection);
        Quaternion smoothRotation = Quaternion.Slerp(rb.rotation, targetRotation, turnSpeed * Time.fixedDeltaTime);
        rb.MoveRotation(smoothRotation);

        Vector3 move = rb.transform.forward * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + move);
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
