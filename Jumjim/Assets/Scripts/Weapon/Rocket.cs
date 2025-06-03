using UnityEngine;

public class Rocket : MonoBehaviour
{
    public float lifetime = 5f;
    public GameObject explosionEffect;
    public float explosionRadius = 5f;
    public float explosionDamage = 100f;
    public LayerMask damageableLayers;
    public string[] explodeOnTags = { "Ground", "Wall", "Enemy", };

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter(Collision collision)
    {
        string tag = collision.gameObject.tag;
        foreach (string validTag in explodeOnTags)
        {
            if (tag == validTag)
            {
                Explode();
                break;
            }
        }
    }

    void Explode()
    {
        // Spawn explosion effect
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        // Find objects in radius
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, damageableLayers);
        foreach (Collider hit in hitColliders)
        {
            // Try to apply damage if the object has a health script
            EnemyHealth target = hit.GetComponent<EnemyHealth>();
            if (target != null)
            {
                target.TakeDamage(explosionDamage);
            }
        }

        Destroy(gameObject);
    }
}
