using UnityEngine;

public class PelletProjectile : MonoBehaviour
{
    public float speed = 50f;
    public float lifetime = 5f;
    public GameObject hitEffect;
    public string[] destroyOnTags = { "Ground", "Wall", "Enemy" };

    private Vector3 direction;

    void Start()
    {
        direction = transform.forward;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnCollisionEnter(Collision collision)
    {
        string tag = collision.gameObject.tag;

        foreach (string validTag in destroyOnTags)
        {
            if (tag == validTag)
            {
                // Instantiate hit effect at contact point
                ContactPoint contact = collision.contacts[0];
                if (hitEffect != null)
                    Instantiate(hitEffect, contact.point, Quaternion.LookRotation(contact.normal));

                Destroy(gameObject);
                break;
            }
        }
    }
}
