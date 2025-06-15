using UnityEngine;

public class HealthPick : MonoBehaviour
{
    public int healAmount = 25;
    public float lifeTime = 10f;
    public float attractRange = 5f;
    public float moveSpeed = 10f;
    public AudioClip pickupSound;
    public GameObject pickupEffect;

    private Transform player;
    private PlayerHealth playerHealth;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        Destroy(transform.parent.gameObject, lifeTime);
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        // If player is within range, move toward them
        if (playerHealth.currentHealth < playerHealth.maxHealth && distance <= attractRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.parent.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth health = other.GetComponent<PlayerHealth>();

        if (health != null && health.currentHealth < health.maxHealth)
        {
            health.HealPlayer(healAmount);

            if (pickupSound != null)
            {
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
            }

            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }

            Destroy(transform.parent.gameObject); // Remove the pickup
        }
    }
}
