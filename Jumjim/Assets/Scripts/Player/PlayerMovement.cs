using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SnappySmoothDashMovement : MonoBehaviour
{
    public float moveSpeed = 16f;
    public float moveSnapSpeed = 100f;
    public float gravity = -35f;
    public float jumpForce = 16f;

    public float dashSpeed = 30f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.5f;

    [Header("Air Control")]
    [Range(0f, 1f)]
    public float airControlFactor = 0.1f;  // How much control you have over movement in air (0 = none, 1 = full ground control)

    private Vector3 velocity = Vector3.zero;
    private float yVelocity = 0f;
    private CharacterController controller;

    // Dash control
    private bool isDashing = false;
    private float dashTimeLeft = 0f;
    private float lastDashTime = -999f;
    private Vector3 dashDirection;

    // FOV kick during dash
    public float normalFOV = 60f;
    public float dashFOV = 80f;
    public float fovLerpSpeed = 8f;

    [Header("Head bobbing")]
    public Transform cameraHolder; // Drag the camera transform here in the Inspector
    public float bobFrequency = 10f;
    public float bobAmplitude = 0.05f;
    public float bobLerpSpeed = 10f;

    private float bobTimer = 0f;
    private Vector3 camStartLocalPos;

    [Header("Camera Tilt")]
    public float tiltAmount = 15f;        // Maximum tilt angle in degrees
    public float tiltLerpSpeed = 8f;      // How fast the camera tilts
    public bool invertTilt = false;       // Invert the tilt direction if needed
    
    private float currentTilt = 0f;       // Current tilt angle

    [Header("Landing Effect")]
    // Landing bounce
    public LandingBounce landingBounce;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (cameraHolder != null)
            camStartLocalPos = cameraHolder.localPosition;
    }

    void Update()
    {
        bool grounded = controller.isGrounded;

        // Input
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        Vector3 inputDir = transform.TransformDirection(input);

        // Start dash
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= lastDashTime + dashCooldown)
        {
            isDashing = true;
            dashTimeLeft = dashDuration;
            lastDashTime = Time.time;
            dashDirection = inputDir != Vector3.zero ? inputDir : transform.forward;
        }

        if (isDashing)
        {
            velocity = dashDirection * dashSpeed;
            dashTimeLeft -= Time.deltaTime;

            if (dashTimeLeft <= 0)
            {
                isDashing = false;
            }
        }
        else
        {
            if (grounded)
            {
                // Full control on ground
                Vector3 targetVelocity = inputDir * moveSpeed;
                velocity = Vector3.MoveTowards(velocity, targetVelocity, moveSnapSpeed * Time.deltaTime);
            }
            else
            {
                // Partial air control
                Vector3 targetVelocity = inputDir * moveSpeed;
                velocity = Vector3.MoveTowards(velocity, targetVelocity, moveSnapSpeed * airControlFactor * Time.deltaTime);
            }
        }

        // Gravity + Jump
        if (grounded)
        {
            if (Input.GetButtonDown("Jump"))
                yVelocity = jumpForce;
            else
                yVelocity = -1f; // small downward force to stay grounded
        }
        else
        {
            yVelocity += gravity * Time.deltaTime;
        }

        Vector3 move = velocity + Vector3.up * yVelocity;
        controller.Move(move * Time.deltaTime);

        // Camera FOV kick during dash
        if (Camera.main != null)
        {
            float targetFOV = isDashing ? dashFOV : normalFOV;
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFOV, fovLerpSpeed * Time.deltaTime);
        }

        // Camera tilt logic
        if (cameraHolder != null)
        {
            // Calculate target tilt based on horizontal input
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float targetTilt = horizontalInput * tiltAmount;
            
            // Invert tilt if needed (some people prefer opposite direction)
            if (invertTilt)
                targetTilt = -targetTilt;
            
            // Reduce tilt during dash for stability
            if (isDashing)
                targetTilt *= 0.3f;
            
            // Smooth tilt transition
            currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltLerpSpeed * Time.deltaTime);
        }

        // Head bobbing logic
        if (cameraHolder != null)
        {
            bool isMoving = input.magnitude > 0.1f && grounded && !isDashing;

            if (isMoving)
            {
                bobTimer += Time.deltaTime * bobFrequency;
                float bobOffset = Mathf.Sin(bobTimer) * bobAmplitude;

                Vector3 targetPos = camStartLocalPos + new Vector3(0, bobOffset, 0);
                cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, targetPos, bobLerpSpeed * Time.deltaTime);
            }
            else
            {
                // Return to original position
                cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, camStartLocalPos, bobLerpSpeed * Time.deltaTime);
                bobTimer = 0f;
            }

            // Apply the tilt rotation (combined with any existing rotation)
            Vector3 currentRotation = cameraHolder.localEulerAngles;
            cameraHolder.localRotation = Quaternion.Euler(currentRotation.x, currentRotation.y, currentTilt);
        }

        // Landing bounce update
        if (landingBounce != null)
        {
            landingBounce.SetGrounded(grounded);
        }
    }
}