using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false;

    public EnemySpawner spawner; // Assigned by EnemySpawner

    public GameObject bloodEffect;
    public Transform effectPoint;

    [Header("Drop Prefabs")]
    public GameObject healthDropPrefab;
    [Range(0f, 1f)] public float healthDropChance = 0.5f;

    public GameObject armorDropPrefab;
    [Range(0f, 1f)] public float armorDropChance = 0.3f;

    [Header("Drop Amount Settings")]
    public Vector2Int healthDropAmountRange = new Vector2Int(1, 3);
    public Vector2Int armorDropAmountRange = new Vector2Int(1, 2);

    [Header("Fall Detection")]
    public float fallThreshold = -10f;
    public LayerMask groundLayers = ~0;
    public float groundCheckDistance = 1.2f;

    private Vector3 lastGroundedPosition;

    void Start()
    {
        currentHealth = maxHealth;
        lastGroundedPosition = transform.position;
    }

    void Update()
    {
        TrackGroundedPosition();
        CheckFall();
    }

    void TrackGroundedPosition()
    {
        if (Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayers))
        {
            lastGroundedPosition = hit.point;
        }
    }

    void CheckFall()
    {
        if (transform.position.y < fallThreshold)
        {
            TeleportToLastGroundedPosition();
        }
    }

    void TeleportToLastGroundedPosition()
    {
        Vector3 safePosition = lastGroundedPosition + Vector3.up * 1f;
        transform.position = safePosition;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;

        if (bloodEffect != null && effectPoint != null)
        {
            GameObject bloodFX = Instantiate(bloodEffect, effectPoint.position, Quaternion.identity);
            Destroy(bloodFX, 2f);
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        DropItems();

        // Notify spawner
        if (spawner != null)
        {
            spawner.OnEnemyDied();
        }

        Destroy(gameObject);
    }

    void DropItem(GameObject prefab, Vector3 basePosition)
    {
        Vector3 randomOffset = new Vector3(Random.Range(-0.3f, 0.3f), 0f, Random.Range(-0.3f, 0.3f));
        Vector3 dropPos = basePosition + randomOffset;

        GameObject drop = Instantiate(prefab, dropPos, Random.rotation);

        Collider dropCollider = drop.GetComponent<Collider>();
        if (dropCollider != null)
        {
            float offsetY = dropCollider.bounds.extents.y;
            drop.transform.position += Vector3.up * offsetY;
        }

        Rigidbody rb = drop.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.WakeUp();
        }

        AddDropForce(drop);
    }

    void DropItems()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 1f;
        Vector3 baseDropPosition = transform.position;

        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hitInfo, 5f, LayerMask.GetMask("Default")))
        {
            baseDropPosition = hitInfo.point + Vector3.up * 0.1f;
        }

        if (healthDropPrefab != null && Random.value <= healthDropChance)
        {
            int dropCount = Random.Range(healthDropAmountRange.x, healthDropAmountRange.y + 1);
            for (int i = 0; i < dropCount; i++)
            {
                DropItem(healthDropPrefab, baseDropPosition);
            }
        }

        if (armorDropPrefab != null && Random.value <= armorDropChance)
        {
            int dropCount = Random.Range(armorDropAmountRange.x, armorDropAmountRange.y + 1);
            for (int i = 0; i < dropCount; i++)
            {
                DropItem(armorDropPrefab, baseDropPosition);
            }
        }
    }

    void AddDropForce(GameObject drop)
    {
        Rigidbody rb = drop.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Vector3 forceDir = (Vector3.up + Random.insideUnitSphere * 0.5f).normalized;
            float forceAmount = Random.Range(3f, 6f);
            rb.AddForce(forceDir * forceAmount, ForceMode.Impulse);
        }
    }
}
