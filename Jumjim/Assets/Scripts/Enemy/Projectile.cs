using UnityEngine;

public class Projectile : MonoBehaviour
{
    public int damage = 10;
    public float speed = 5f;
    public float lifetime = 5f;

    private Vector3 moveDirection;

    void Start()
    {
        Destroy(gameObject, lifetime);

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            Vector3 targetPosition = playerObj.transform.position;
            moveDirection = (targetPosition - transform.position).normalized;

            transform.rotation = Quaternion.LookRotation(moveDirection); // Make projectile face the direction
        }
        else
        {
            moveDirection = transform.forward; // fallback
        }
    }

    void Update()
    {
        transform.position += moveDirection * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }
}
