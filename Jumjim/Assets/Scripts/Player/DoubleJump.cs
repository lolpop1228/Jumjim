using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class DoubleJump : MonoBehaviour
{
    [Header("Double Jump Settings")]
    public float doubleJumpForce = 14f;
    public bool enabled = true;
    
    [Header("Audio")]
    public AudioClip doubleJumpClip;
    
    [Header("Visual Effects")]
    public GameObject doubleJumpEffect; // Optional particle effect
    public Transform effectSpawnPoint; // Where to spawn the effect (usually at feet)
    
    private PlayerMovement playerMovement;
    private CharacterController controller;
    private bool hasDoubleJumped = false;
    private bool wasGroundedLastFrame = false;
    
    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        controller = GetComponent<CharacterController>();
        
        if (effectSpawnPoint == null)
            effectSpawnPoint = transform; // Default to player transform if not set
    }
    
    void Update()
    {
        if (!enabled) return;
        
        bool isGrounded = controller.isGrounded;
        
        // Reset double jump when touching ground
        if (isGrounded && !wasGroundedLastFrame)
        {
            hasDoubleJumped = false;
        }
        
        // Handle double jump input
        if (Input.GetButtonDown("Jump"))
        {
            // If we're not grounded and haven't used our double jump yet
            if (!isGrounded && !hasDoubleJumped)
            {
                PerformDoubleJump();
            }
        }
        
        wasGroundedLastFrame = isGrounded;
    }
    
    void PerformDoubleJump()
    {
        hasDoubleJumped = true;
        
        // Access the private yVelocity field using reflection or modify PlayerMovement to expose it
        // For now, we'll use a public method approach (see note below)
        SetVerticalVelocity(doubleJumpForce);
        
        // Play audio
        if (doubleJumpClip && playerMovement.footstepAudioSource)
        {
            playerMovement.footstepAudioSource.PlayOneShot(doubleJumpClip);
        }
        
        // Spawn visual effect
        if (doubleJumpEffect)
        {
            GameObject effect = Instantiate(doubleJumpEffect, effectSpawnPoint.position, effectSpawnPoint.rotation);
            // Auto-destroy the effect after 2 seconds
            Destroy(effect, 2f);
        }
    }
    
    // This method requires modification to PlayerMovement script (see below)
    private void SetVerticalVelocity(float newYVelocity)
    {
        // We need to access the private yVelocity from PlayerMovement
        // This requires adding a public method to PlayerMovement
        var method = playerMovement.GetType().GetMethod("SetYVelocity");
        if (method != null)
        {
            method.Invoke(playerMovement, new object[] { newYVelocity });
        }
        else
        {
            Debug.LogWarning("SetYVelocity method not found in PlayerMovement. Please add the method as shown in the instructions.");
        }
    }
    
    // Public getter for other scripts that might need to know if player has double jumped
    public bool HasDoubleJumped()
    {
        return hasDoubleJumped;
    }
    
    // Public getter for UI or other systems
    public bool CanDoubleJump()
    {
        return enabled && !controller.isGrounded && !hasDoubleJumped;
    }
}