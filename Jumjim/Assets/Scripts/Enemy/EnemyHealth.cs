using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;
    private bool isDead = false; // 🔒 Prevent multiple deaths

    public GameObject bloodEffect;
    public Transform effectPoint;

    [Header("Drop Prefabs")]
    public GameObject healthDropPrefab;
    [Range(0f, 1f)] public float healthDropChance = 0.5f;

    public GameObject armorDropPrefab;
    [Range(0f, 1f)] public float armorDropChance = 0.3f;
    [Header("Drop Amount Settings")]
    public Vector2Int healthDropAmountRange = new Vector2Int(1, 3); // Drop 1–3 health pickups
    public Vector2Int armorDropAmountRange = new Vector2Int(1, 2);  // Drop 1–2 armor pickups


    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return; // 🛑 Already dead, ignore damage

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
        if (isDead) return; // 🛑 Double check before dying again
        isDead = true;

        DropItems();
        Destroy(gameObject);
    }

    void DropItem(GameObject prefab, Vector3 basePosition)
    {
        Vector3 randomOffset = new Vector3(Random.Range(-0.3f, 0.3f), 0f, Random.Range(-0.3f, 0.3f));
        Vector3 dropPos = basePosition + randomOffset;

        GameObject drop = Instantiate(prefab, dropPos, Random.rotation);

        // Ensure not clipped through ground
        Collider dropCollider = drop.GetComponent<Collider>();
        if (dropCollider != null)
        {
            float offsetY = dropCollider.bounds.extents.y;
            drop.transform.position += Vector3.up * offsetY;
        }

        // Physics-safe spawn
        Rigidbody rb = drop.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero; // Stop any unwanted launch
            rb.angularVelocity = Vector3.zero;
            rb.WakeUp(); // Ensure physics works
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
