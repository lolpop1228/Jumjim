using UnityEngine;

public class ArmorPick : MonoBehaviour
{
    public int armorAmount = 25;
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
        if (playerHealth.currentArmor < playerHealth.maxArmor && distance <= attractRange)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            transform.parent.position += direction * moveSpeed * Time.deltaTime;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerHealth health = other.GetComponent<PlayerHealth>();

        if (health != null && health.currentArmor < health.maxArmor)
        {
            health.AddArmor(armorAmount);

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
