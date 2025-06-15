using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 16f;
    public float moveSnapSpeed = 100f;
    public float gravity = -35f;
    public float jumpForce = 16f;

    [Header("Air Control")]
    [Range(0f, 1f)]
    public float airControlFactor = 0.1f;

    private Vector3 velocity = Vector3.zero;
    private float yVelocity = 0f;
    private CharacterController controller;

    [Header("Head bobbing")]
    public Transform cameraHolder;
    public float bobFrequency = 10f;
    public float bobAmplitude = 0.05f;
    public float bobLerpSpeed = 10f;

    private float bobTimer = 0f;
    private Vector3 camStartLocalPos;
    private float previousBobOffset = 0f;

    [Header("Camera Tilt")]
    public float tiltAmount = 15f;
    public float tiltLerpSpeed = 8f;
    public bool invertTilt = false;
    private float currentTilt = 0f;

    [Header("Landing Effect")]
    public LandingBounce landingBounce;

    [Header("Audio")]
    public AudioSource footstepAudioSource;
    public AudioClip footstepClip;
    public AudioClip jumpClip;
    private bool wasGroundedLastFrame = true;

    [Header("FOV Kick")]
    public float normalFOV = 90f;
    public float dashFOV = 110f;
    public float fovLerpSpeed = 8f;
    private PlayerDash dash;

    // Better ground detection for slopes
    private float groundCheckDistance = 0.2f;
    private bool isNearGround = false;

    // Flag to track initialization
    private bool isInitialized = false;
    private Vector3 externalVelocity;

    // Initialize components as early as possible
    void Awake()
    {
        InitializeComponents();
    }

    void Start()
    {
        // Ensure initialization happened
        if (!isInitialized)
            InitializeComponents();
    }

    void OnEnable()
    {
        // Re-initialize when script is enabled (helps with older Unity versions)
        if (!isInitialized)
            InitializeComponents();
    }

    private void InitializeComponents()
    {
        try
        {
            // Get required components
            if (controller == null)
                controller = GetComponent<CharacterController>();

            if (dash == null)
                dash = GetComponent<PlayerDash>();

            // Store camera start position
            if (cameraHolder != null)
                camStartLocalPos = cameraHolder.localPosition;

            // Ensure footstep audio source exists
            if (footstepAudioSource == null)
                footstepAudioSource = GetComponent<AudioSource>();

            isInitialized = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"PlayerMovement initialization failed: {e.Message}", this);
            isInitialized = false;
        }
    }

    void Update()
    {
        // Safety check - ensure components are initialized
        if (!isInitialized || controller == null)
        {
            InitializeComponents();
            if (!isInitialized || controller == null)
                return;
        }

        bool grounded = controller.isGrounded;

        // Better ground detection for slopes - check slightly below the controller
        RaycastHit hit;
        isNearGround = Physics.Raycast(transform.position, Vector3.down, out hit, controller.height * 0.5f + groundCheckDistance);

        // Input
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        Vector3 inputDir = transform.TransformDirection(input);

        // Movement
        Vector3 targetVelocity = inputDir * moveSpeed;
        float controlFactor = grounded ? 1f : airControlFactor;
        velocity = Vector3.MoveTowards(velocity, targetVelocity, moveSnapSpeed * controlFactor * Time.deltaTime);

        PlayerDash dashComponent = GetComponent<PlayerDash>();
        if (Camera.main != null && dashComponent != null)
        {
            float targetFOV = dashComponent.IsDashing() ? dashFOV : normalFOV;
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFOV, fovLerpSpeed * Time.deltaTime);
        }

        // Gravity + Jump
        if (grounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                yVelocity = jumpForce;
                if (footstepAudioSource && jumpClip)
                    footstepAudioSource.PlayOneShot(jumpClip);
            }
            else if (yVelocity < 0f)
            {
                yVelocity = -1f;
            }
        }
        else
        {
            yVelocity += gravity * Time.deltaTime;
        }

        Vector3 move = velocity + externalVelocity + Vector3.up * yVelocity;
        controller.Move(move * Time.deltaTime);
        externalVelocity = Vector3.zero; // Reset after applying

        // Camera tilt - FIXED VERSION
        if (cameraHolder != null)
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float targetTilt = horizontalInput * tiltAmount;
            if (invertTilt) targetTilt = -targetTilt;

            // Smooth interpolation to target tilt
            currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltLerpSpeed * Time.deltaTime);

            // Apply the tilt rotation - preserving existing X and Y rotations
            Vector3 currentRotation = cameraHolder.localEulerAngles;
            cameraHolder.localRotation = Quaternion.Euler(currentRotation.x, currentRotation.y, currentTilt);
        }

        // Head bobbing + footstep - use better ground detection
        if (cameraHolder != null)
        {
            // Use isNearGround instead of just grounded for more consistent footsteps on slopes
            bool isMoving = input.magnitude > 0.1f && (grounded || isNearGround);

            if (isMoving)
            {
                bobTimer += Time.deltaTime * bobFrequency;
                float bobOffset = Mathf.Sin(bobTimer) * bobAmplitude;

                Vector3 targetPos = camStartLocalPos + new Vector3(0, bobOffset, 0);
                cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, targetPos, bobLerpSpeed * Time.deltaTime);

                if (previousBobOffset <= 0f && bobOffset > 0f)
                {
                    if (footstepAudioSource && footstepClip && (dash == null || !dash.IsDashing()))
                        footstepAudioSource.PlayOneShot(footstepClip);
                }

                previousBobOffset = bobOffset;
            }
            else
            {
                cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, camStartLocalPos, bobLerpSpeed * Time.deltaTime);
                bobTimer = 0f;
                previousBobOffset = 0f;
            }
        }

        if (landingBounce)
            landingBounce.SetGrounded(grounded);
    }

    // Methods for DoubleJump script to use
    public void SetYVelocity(float newYVelocity)
    {
        yVelocity = newYVelocity;
    }

    public float GetYVelocity()
    {
        return yVelocity;
    }

    // Additional helper methods for better integration
    public bool IsGrounded()
    {
        return controller != null ? controller.isGrounded : false;
    }

    public bool IsNearGround()
    {
        return isNearGround;
    }

    public CharacterController GetController()
    {
        if (controller == null)
            InitializeComponents();
        return controller;
    }

    public void SetExternalVelocity(Vector3 velocity)
    {
        externalVelocity = velocity;
    }
}