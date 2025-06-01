using UnityEngine;

public class CameraRecoil : MonoBehaviour
{
    [Header("Recoil Settings")]
    [Tooltip("How much the camera kicks up when firing")]
    public float recoilX = 2f;
    
    [Tooltip("Random horizontal recoil amount")]
    public float recoilY = 1f;
    
    [Tooltip("How much the camera rotates back")]
    public float recoilZ = 0.5f;
    
    [Header("Recoil Behavior")]
    [Tooltip("How fast recoil happens")]
    public float recoilSpeed = 10f;
    
    [Tooltip("How fast camera returns to normal")]
    public float returnSpeed = 5f;
    
    [Tooltip("Maximum recoil accumulation")]
    public float maxRecoil = 15f;
    
    [Header("Advanced Settings")]
    [Tooltip("Recoil pattern curve - controls how recoil builds up")]
    public AnimationCurve recoilCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Tooltip("How much recoil decreases over time")]
    public float recoilDecay = 3f;
    
    [Tooltip("Randomness factor for natural feel")]
    public float randomnessFactor = 0.3f;
    
    [Header("Weapon-Specific Modifiers")]
    [Tooltip("Multiplier for different weapon types")]
    public float weaponRecoilMultiplier = 1f;
    
    // Private variables
    private Vector3 currentRecoil;
    private Vector3 targetRecoil;
    private Vector3 initialRotation;
    private float recoilAccumulation = 0f;
    private int shotsFired = 0;
    private float lastShotTime = 0f;
    
    // Component references
    private Transform cameraTransform;
    
    void Start()
    {
        // Get camera transform
        cameraTransform = transform;
        initialRotation = cameraTransform.localEulerAngles;
        
        // Initialize recoil curve if not set
        if (recoilCurve == null || recoilCurve.keys.Length == 0)
        {
            recoilCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        }
    }
    
    void Update()
    {
        // Gradually return camera to normal position
        UpdateRecoil();
        
        // Decay recoil accumulation over time
        if (Time.time - lastShotTime > 0.5f)
        {
            recoilAccumulation = Mathf.Lerp(recoilAccumulation, 0f, Time.deltaTime * recoilDecay);
            shotsFired = Mathf.Max(0, shotsFired - 1);
        }
    }
    
    void UpdateRecoil()
    {
        // Smoothly move towards target recoil
        currentRecoil = Vector3.Slerp(currentRecoil, targetRecoil, Time.deltaTime * recoilSpeed);
        
        // Return to zero when target is reached
        targetRecoil = Vector3.Lerp(targetRecoil, Vector3.zero, Time.deltaTime * returnSpeed);
        
        // Apply recoil to camera rotation
        Vector3 finalRotation = new Vector3(
            initialRotation.x + currentRecoil.x,
            initialRotation.y + currentRecoil.y,
            initialRotation.z + currentRecoil.z
        );
        
        cameraTransform.localEulerAngles = finalRotation;
    }
    
    /// <summary>
    /// Call this method when the weapon fires
    /// </summary>
    public void FireRecoil()
    {
        FireRecoil(1f);
    }
    
    /// <summary>
    /// Call this method when the weapon fires with custom intensity
    /// </summary>
    /// <param name="intensity">Recoil intensity multiplier (1 = normal)</param>
    public void FireRecoil(float intensity)
    {
        // Update shot tracking
        shotsFired++;
        lastShotTime = Time.time;
        
        // Calculate recoil accumulation
        float accumulationMultiplier = Mathf.Clamp01(recoilAccumulation / maxRecoil);
        float curveValue = recoilCurve.Evaluate(accumulationMultiplier);
        
        // Add randomness for natural feel
        float randomX = Random.Range(-randomnessFactor, randomnessFactor);
        float randomY = Random.Range(-randomnessFactor, randomnessFactor);
        float randomZ = Random.Range(-randomnessFactor, randomnessFactor);
        
        // Calculate recoil amounts
        float finalRecoilX = recoilX * intensity * weaponRecoilMultiplier * (1f + curveValue) + randomX;
        float finalRecoilY = Random.Range(-recoilY, recoilY) * intensity * weaponRecoilMultiplier + randomY;
        float finalRecoilZ = Random.Range(-recoilZ, recoilZ) * intensity * weaponRecoilMultiplier + randomZ;
        
        // Clamp to maximum recoil
        finalRecoilX = Mathf.Clamp(finalRecoilX, 0f, maxRecoil);
        finalRecoilY = Mathf.Clamp(finalRecoilY, -maxRecoil, maxRecoil);
        finalRecoilZ = Mathf.Clamp(finalRecoilZ, -maxRecoil, maxRecoil);
        
        // Apply recoil
        Vector3 recoilToAdd = new Vector3(-finalRecoilX, finalRecoilY, finalRecoilZ);
        targetRecoil += recoilToAdd;
        
        // Update accumulation
        recoilAccumulation += finalRecoilX;
        recoilAccumulation = Mathf.Clamp(recoilAccumulation, 0f, maxRecoil);
    }
    
    /// <summary>
    /// Set weapon-specific recoil values
    /// </summary>
    /// <param name="x">Vertical recoil</param>
    /// <param name="y">Horizontal recoil</param>
    /// <param name="z">Roll recoil</param>
    /// <param name="multiplier">Overall multiplier</param>
    public void SetWeaponRecoil(float x, float y, float z, float multiplier = 1f)
    {
        recoilX = x;
        recoilY = y;
        recoilZ = z;
        weaponRecoilMultiplier = multiplier;
    }
    
    /// <summary>
    /// Reset recoil accumulation (useful when stopping fire or reloading)
    /// </summary>
    public void ResetRecoil()
    {
        recoilAccumulation = 0f;
        shotsFired = 0;
        targetRecoil = Vector3.zero;
        currentRecoil = Vector3.zero;
    }
    
    /// <summary>
    /// Get current recoil amount (useful for UI or other systems)
    /// </summary>
    public float GetRecoilAmount()
    {
        return currentRecoil.magnitude;
    }
    
    /// <summary>
    /// Check if camera is currently recoiling
    /// </summary>
    public bool IsRecoiling()
    {
        return currentRecoil.magnitude > 0.1f;
    }
    
    // Example usage - call this from your weapon script
    void ExampleUsage()
    {
        // Basic recoil
        // FireRecoil();
        
        // Heavy weapon recoil
        // FireRecoil(2.5f);
        
        // Set weapon-specific values
        // SetWeaponRecoil(3f, 1.5f, 0.8f, 1.2f);
    }
}