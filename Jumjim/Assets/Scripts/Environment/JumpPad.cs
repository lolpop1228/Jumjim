using UnityEngine;

public class JumpPad : MonoBehaviour
{
    public float jumpForce = 20f; // How strong the jump pad boost is
    public AudioClip jumpPadSound; // Optional sound
    public ParticleSystem jumpEffect; // Optional effect

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();
        if (player != null)
        {
            // Apply vertical velocity (replace with your own method if needed)
            player.SetYVelocity(jumpForce);

            // Optional: play sound
            if (jumpPadSound)
                AudioSource.PlayClipAtPoint(jumpPadSound, transform.position);

            // Optional: play particle effect
            if (jumpEffect)
                Instantiate(jumpEffect, transform.position, Quaternion.identity);
        }
    }
}
