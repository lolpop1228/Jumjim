using UnityEngine;

public class Rocket : MonoBehaviour
{
    public float lifetime = 5f;
    public GameObject explosionEffect;
    public float explosionRadius = 5f;
    public float explosionDamage = 100f;
    public int playerDamage = 50;
    public LayerMask damageableLayers;
    public string[] explodeOnTags = { "Ground", "Wall", "Enemy" };

    public AudioClip explosionSound;
    public float explosionVolume = 1f;

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
        // Play explosion sound loud & clear
        if (explosionSound != null)
        {
            GameObject soundObj = new GameObject("ExplosionSound");
            soundObj.transform.position = transform.position;

            AudioSource source = soundObj.AddComponent<AudioSource>();
            source.clip = explosionSound;
            source.volume = explosionVolume;
            source.spatialBlend = 1f; // 3D
            source.minDistance = 5f;
            source.maxDistance = 30f;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.Play();

            Destroy(soundObj, explosionSound.length);
        }

        // Spawn explosion effect
        if (explosionEffect != null)
            Instantiate(explosionEffect, transform.position, Quaternion.identity);

        // Damage logic...
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, explosionRadius, damageableLayers);
        foreach (Collider hit in hitColliders)
        {
            EnemyHealth enemy = hit.GetComponent<EnemyHealth>();
            PlayerHealth player = hit.GetComponent<PlayerHealth>();

            if (enemy != null)
                enemy.TakeDamage(explosionDamage);

            if (player != null)
                player.TakeDamage(playerDamage);
        }

        Destroy(gameObject);
    }
}
