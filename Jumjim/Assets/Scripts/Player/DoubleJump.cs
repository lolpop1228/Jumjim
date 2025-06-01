using UnityEngine;

public class DoubleJump : MonoBehaviour
{
    [Header("Double Jump Settings")]
    public float doubleJumpForce = 14f;
    public bool jumpEnabled = true;
    
    [Header("Audio")]
    public AudioClip doubleJumpClip;
    
    // Don't serialize these - always find them fresh
    [System.NonSerialized]
    private PlayerMovement playerMovement;
    [System.NonSerialized]
    private CharacterController controller;
    
    private bool hasDoubleJumped = false;
    private bool wasGroundedLastFrame = false;
    
    void Awake()
    {
        RefreshComponents();
    }
    
    void Start()
    {
        RefreshComponents();
    }

    void OnEnable()
    {
        RefreshComponents();
    }
    
    private void RefreshComponents()
    {
        // Always get fresh references
        playerMovement = GetComponent<PlayerMovement>();
        controller = GetComponent<CharacterController>();
        
        if (playerMovement == null)
        {
            Debug.LogError($"PlayerMovement component not found on {gameObject.name}!");
        }
        
        if (controller == null)
        {
            Debug.LogError($"CharacterController component not found on {gameObject.name}!");
        }
    }

    void Update()
    {
        // Safety check and refresh if needed
        if (playerMovement == null || controller == null)
        {
            RefreshComponents();
            if (playerMovement == null || controller == null)
                return;
        }
        
        if (!jumpEnabled) return;
        
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
        if (playerMovement == null)
        {
            RefreshComponents();
            if (playerMovement == null) return;
        }
        
        hasDoubleJumped = true;
        
        // Use the SetYVelocity method from PlayerMovement
        playerMovement.SetYVelocity(doubleJumpForce);
        
        // Play audio
        if (doubleJumpClip != null && playerMovement.footstepAudioSource != null)
        {
            playerMovement.footstepAudioSource.PlayOneShot(doubleJumpClip);
        }
    }
    
    // Public methods
    public bool HasDoubleJumped()
    {
        return hasDoubleJumped;
    }
    
    public bool CanDoubleJump()
    {
        if (controller == null) RefreshComponents();
        return jumpEnabled && controller != null && !controller.isGrounded && !hasDoubleJumped;
    }
    
    public void ResetDoubleJump()
    {
        hasDoubleJumped = false;
    }
    
    // Manual refresh for debugging
    [ContextMenu("Refresh Components")]
    public void ManualRefresh()
    {
        RefreshComponents();
        Debug.Log("DoubleJump components refreshed manually.");
    }
}