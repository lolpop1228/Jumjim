using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class DoubleJump : MonoBehaviour
{
    [Header("Double Jump Settings")]
    public float doubleJumpForce = 14f;
    public int maxJumps = 2;
    
    [Header("Air Jump Control")]
    [Range(0f, 1f)]
    public float airJumpControlBoost = 0.3f; // Extra air control after double jump
    public float airJumpControlDuration = 0.5f;
    
    [Header("Visual Effects")]
    public ParticleSystem doubleJumpEffect;
    public GameObject doubleJumpTrail;
    
    [Header("Audio")]
    public AudioClip doubleJumpClip;
    
    [Header("Coyote Time")]
    public float coyoteTime = 0.1f; // Grace period after leaving ground
    
    // Private variables
    private PlayerMovement playerMovement;
    private int currentJumps = 0;
    private bool wasGroundedLastFrame = true;
    private float coyoteTimer = 0f;
    private float airJumpBoostTimer = 0f;
    private AudioSource audioSource;
    
    // Ground detection
    private bool isInitialized = false;
    
    void Awake()
    {
        InitializeComponents();
    }
    
    void Start()
    {
        if (!isInitialized)
            InitializeComponents();
    }
    
    private void InitializeComponents()
    {
        try
        {
            if (playerMovement == null)
                playerMovement = GetComponent<PlayerMovement>();
            
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            
            isInitialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"DoubleJump initialization failed: {e.Message}", this);
            isInitialized = false;
        }
    }
    
    void Update()
    {
        // Safety check
        if (!isInitialized || playerMovement == null)
        {
            InitializeComponents();
            if (!isInitialized || playerMovement == null)
                return;
        }
        
        bool isGrounded = playerMovement.IsGrounded();
        bool isNearGround = playerMovement.IsNearGround();
        
        // Update coyote timer
        if (isGrounded || isNearGround)
        {
            coyoteTimer = coyoteTime;
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
        }
        
        // Reset jump count when grounded
        if (isGrounded && !wasGroundedLastFrame)
        {
            currentJumps = 0;
        }
        
        // Handle jump input
        if (Input.GetButtonDown("Jump"))
        {
            HandleJumpInput(isGrounded, isNearGround);
        }
        
        // Update air jump boost timer
        if (airJumpBoostTimer > 0f)
        {
            airJumpBoostTimer -= Time.deltaTime;
        }
        
        wasGroundedLastFrame = isGrounded;
    }
    
    private void HandleJumpInput(bool isGrounded, bool isNearGround)
    {
        // First jump (ground jump or coyote time)
        if ((isGrounded || isNearGround || coyoteTimer > 0f) && currentJumps == 0)
        {
            PerformJump(playerMovement.jumpForce, false);
            currentJumps = 1;
            coyoteTimer = 0f; // Consume coyote time
        }
        // Double jump (and additional air jumps if maxJumps > 2)
        else if (!isGrounded && !isNearGround && currentJumps < maxJumps && currentJumps > 0)
        {
            PerformJump(doubleJumpForce, true);
            currentJumps++;
            
            // Activate air control boost
            airJumpBoostTimer = airJumpControlDuration;
        }
    }
    
    private void PerformJump(float jumpForce, bool isDoubleJump)
    {
        // Set vertical velocity
        playerMovement.SetYVelocity(jumpForce);
        
        if (isDoubleJump)
        {
            // Play double jump effects
            PlayDoubleJumpEffects();
        }
    }
    
    private void PlayDoubleJumpEffects()
    {
        // Particle effect
        if (doubleJumpEffect != null)
        {
            doubleJumpEffect.Play();
        }
        
        // Trail effect
        if (doubleJumpTrail != null)
        {
            GameObject trail = Instantiate(doubleJumpTrail, transform.position, transform.rotation);
            // Destroy trail after a few seconds if it doesn't destroy itself
            Destroy(trail, 2f);
        }
        
        // Audio
        if (audioSource != null && doubleJumpClip != null)
        {
            audioSource.PlayOneShot(doubleJumpClip);
        }
    }
    
    // Public methods for other scripts to use
    public int GetCurrentJumps()
    {
        return currentJumps;
    }
    
    public int GetMaxJumps()
    {
        return maxJumps;
    }
    
    public bool CanDoubleJump()
    {
        return currentJumps < maxJumps && currentJumps > 0 && 
               !playerMovement.IsGrounded() && !playerMovement.IsNearGround();
    }
    
    public float GetAirControlBoost()
    {
        return airJumpBoostTimer > 0f ? airJumpControlBoost : 0f;
    }
    
    public bool HasCoyoteTime()
    {
        return coyoteTimer > 0f;
    }
    
    // Force reset jumps (useful for special abilities or checkpoints)
    public void ResetJumps()
    {
        currentJumps = 0;
        coyoteTimer = 0f;
        airJumpBoostTimer = 0f;
    }
    
    // Add extra jumps temporarily (for power-ups)
    public void AddTempJump(int extraJumps = 1)
    {
        maxJumps += extraJumps;
        // You might want to reset this after a certain time or condition
    }
    
    // Debug visualization
    void OnGUI()
    {
        if (Application.isPlaying && Debug.isDebugBuild)
        {
            GUI.color = Color.white;
            GUI.Label(new Rect(10, 10, 200, 20), $"Jumps: {currentJumps}/{maxJumps}");
            GUI.Label(new Rect(10, 30, 200, 20), $"Coyote: {coyoteTimer:F2}");
            GUI.Label(new Rect(10, 50, 200, 20), $"Air Boost: {airJumpBoostTimer:F2}");
        }
    }
}