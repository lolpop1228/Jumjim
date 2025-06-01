using UnityEngine;

public class WeaponSwayBobbing : MonoBehaviour
{
    [Header("Weapon References")]
    public Transform weaponHolder; // The transform that holds your weapon
    
    [Header("Mouse Sway Settings")]
    public float swayAmount = 0.02f;
    public float swaySpeed = 4f;
    public float swayResetSpeed = 2f;
    public float maxSwayDistance = 0.06f;
    
    [Header("Movement Sway Settings")]
    public float movementSwayAmount = 0.01f;
    public float movementSwaySpeed = 2f;
    
    [Header("Weapon Bobbing")]
    public float bobFrequency = 8f;
    public float bobAmplitude = 0.02f;
    public float bobSpeed = 10f;
    
    [Header("Tilt Settings")]
    public float tiltAmount = 5f;
    public float tiltSpeed = 3f;
    
    [Header("Advanced Settings")]
    public bool enableMouseSway = true;
    public bool enableMovementSway = true;
    public bool enableBobbing = true;
    public bool enableTilt = true;
    public bool smoothMovement = true;
    
    // Private variables
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private Vector2 mouseInput;
    private Vector2 smoothMouseInput;
    private Vector3 movementInput;
    private float bobTimer = 0f;
    private PlayerMovement playerMovement;
    private CharacterController characterController;
    
    // Current sway values
    private Vector3 currentMouseSway;
    private Vector3 currentMovementSway;
    private Vector3 currentBobOffset;
    private Vector3 currentTilt;
    
    void Start()
    {
        // Store initial position and rotation
        if (weaponHolder != null)
        {
            initialPosition = weaponHolder.localPosition;
            initialRotation = weaponHolder.localRotation;
        }
        else
        {
            weaponHolder = transform;
            initialPosition = transform.localPosition;
            initialRotation = transform.localRotation;
        }
        
        // Get player components
        playerMovement = GetComponentInParent<PlayerMovement>();
        if (playerMovement == null)
            playerMovement = FindObjectOfType<PlayerMovement>();
            
        characterController = GetComponentInParent<CharacterController>();
        if (characterController == null)
            characterController = FindObjectOfType<CharacterController>();
    }
    
    void Update()
    {
        // Get inputs
        GetInputs();
        
        // Calculate sway effects
        if (enableMouseSway)
            CalculateMouseSway();
            
        if (enableMovementSway)
            CalculateMovementSway();
            
        if (enableBobbing)
            CalculateBobbing();
            
        if (enableTilt)
            CalculateTilt();
        
        // Apply all effects
        ApplyEffects();
    }
    
    void GetInputs()
    {
        // Mouse input
        Vector2 rawMouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        
        if (smoothMovement)
        {
            smoothMouseInput = Vector2.Lerp(smoothMouseInput, rawMouseInput, Time.deltaTime * swaySpeed);
            mouseInput = smoothMouseInput;
        }
        else
        {
            mouseInput = rawMouseInput;
        }
        
        // Movement input
        movementInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    }
    
    void CalculateMouseSway()
    {
        // Calculate mouse sway - weapon moves opposite to mouse movement for natural feel
        Vector3 targetMouseSway = new Vector3(
            -mouseInput.x * swayAmount,  // Left/Right sway (opposite to mouse X)
            -mouseInput.y * swayAmount,  // Up/Down sway (opposite to mouse Y)
            mouseInput.x * swayAmount * 0.5f  // Forward/Back sway (subtle Z movement)
        );
        
        // Clamp the sway to maximum distance
        targetMouseSway = Vector3.ClampMagnitude(targetMouseSway, maxSwayDistance);
        
        // Smooth the sway
        currentMouseSway = Vector3.Lerp(currentMouseSway, targetMouseSway, Time.deltaTime * swaySpeed);
        
        // Reset sway when not moving mouse
        if (mouseInput.magnitude < 0.01f)
        {
            currentMouseSway = Vector3.Lerp(currentMouseSway, Vector3.zero, Time.deltaTime * swayResetSpeed);
        }
    }
    
    void CalculateMovementSway()
    {
        // Only sway when moving and grounded
        bool isGrounded = characterController != null ? characterController.isGrounded : true;
        bool isMoving = movementInput.magnitude > 0.1f;
        
        if (isMoving && isGrounded)
        {
            // Create movement-based sway
            Vector3 targetMovementSway = new Vector3(
                Mathf.Sin(Time.time * movementSwaySpeed) * movementInput.x * movementSwayAmount,
                Mathf.Sin(Time.time * movementSwaySpeed * 0.5f) * movementSwayAmount,
                Mathf.Sin(Time.time * movementSwaySpeed * 0.8f) * movementInput.z * movementSwayAmount
            );
            
            currentMovementSway = Vector3.Lerp(currentMovementSway, targetMovementSway, Time.deltaTime * movementSwaySpeed);
        }
        else
        {
            currentMovementSway = Vector3.Lerp(currentMovementSway, Vector3.zero, Time.deltaTime * movementSwaySpeed);
        }
    }
    
    void CalculateBobbing()
    {
        // Only bob when moving and grounded
        bool isGrounded = characterController != null ? characterController.isGrounded : true;
        bool isMoving = movementInput.magnitude > 0.1f;
        
        if (isMoving && isGrounded)
        {
            bobTimer += Time.deltaTime * bobFrequency;
            
            // Create bobbing motion
            float bobX = Mathf.Sin(bobTimer * 0.5f) * bobAmplitude * 0.3f;
            float bobY = Mathf.Sin(bobTimer) * bobAmplitude;
            float bobZ = Mathf.Sin(bobTimer * 0.7f) * bobAmplitude * 0.2f;
            
            Vector3 targetBobOffset = new Vector3(bobX, bobY, bobZ);
            currentBobOffset = Vector3.Lerp(currentBobOffset, targetBobOffset, Time.deltaTime * bobSpeed);
        }
        else
        {
            currentBobOffset = Vector3.Lerp(currentBobOffset, Vector3.zero, Time.deltaTime * bobSpeed);
            bobTimer = 0f;
        }
    }
    
    void CalculateTilt()
    {
        // Tilt based on horizontal movement
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        
        Vector3 targetTilt = new Vector3(
            mouseInput.y * tiltAmount * 0.5f,  // Pitch tilt from mouse Y
            0f,
            -horizontalInput * tiltAmount       // Roll tilt from movement
        );
        
        currentTilt = Vector3.Lerp(currentTilt, targetTilt, Time.deltaTime * tiltSpeed);
    }
    
    void ApplyEffects()
    {
        if (weaponHolder == null) return;
        
        // Combine all position effects
        Vector3 finalPosition = initialPosition + currentMouseSway + currentMovementSway + currentBobOffset;
        
        // Combine all rotation effects
        Quaternion finalRotation = initialRotation * Quaternion.Euler(currentTilt);
        
        // Apply to weapon holder
        weaponHolder.localPosition = finalPosition;
        weaponHolder.localRotation = finalRotation;
    }
    
    // Public methods for external control
    public void SetSwayEnabled(bool enabled)
    {
        enableMouseSway = enabled;
        enableMovementSway = enabled;
    }
    
    public void SetBobbingEnabled(bool enabled)
    {
        enableBobbing = enabled;
    }
    
    public void SetTiltEnabled(bool enabled)
    {
        enableTilt = enabled;
    }
    
    public void ResetWeaponPosition()
    {
        if (weaponHolder != null)
        {
            weaponHolder.localPosition = initialPosition;
            weaponHolder.localRotation = initialRotation;
        }
        
        // Reset all current values
        currentMouseSway = Vector3.zero;
        currentMovementSway = Vector3.zero;
        currentBobOffset = Vector3.zero;
        currentTilt = Vector3.zero;
        bobTimer = 0f;
    }
    
    // For debugging - visualize sway limits
    void OnDrawGizmosSelected()
    {
        if (weaponHolder != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(weaponHolder.position, maxSwayDistance);
        }
    }
}