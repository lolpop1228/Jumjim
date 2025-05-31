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
    public float airControlFactor = 0.1f;

    private Vector3 velocity = Vector3.zero;
    private float yVelocity = 0f;
    private CharacterController controller;

    private bool isDashing = false;
    private float dashTimeLeft = 0f;
    private float lastDashTime = -999f;
    private Vector3 dashDirection;

    public float normalFOV = 60f;
    public float dashFOV = 80f;
    public float fovLerpSpeed = 8f;

    [Header("Head Bobbing")]
    public Transform cameraHolder;
    public float bobFrequency = 10f;
    public float bobAmplitude = 0.05f;
    public float bobLerpSpeed = 10f;

    [Header("Camera Tilt")]
    public float maxTiltAngle = 10f;
    public float tiltLerpSpeed = 8f;

    private float bobTimer = 0f;
    private Vector3 camStartLocalPos;
    private Quaternion camStartLocalRot;

    [Header("Landing Effect")]
    public LandingBounce landingBounce;

    void Start()
    {
        controller = GetComponent<CharacterController>();

        if (cameraHolder != null)
        {
            camStartLocalPos = cameraHolder.localPosition;
            camStartLocalRot = cameraHolder.localRotation;
        }
    }

    void Update()
    {
        bool grounded = controller.isGrounded;

        // Input
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        Vector3 inputDir = transform.TransformDirection(input);

        // Dash start
        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= lastDashTime + dashCooldown)
        {
            isDashing = true;
            dashTimeLeft = dashDuration;
            lastDashTime = Time.time;
            dashDirection = inputDir != Vector3.zero ? inputDir : transform.forward;
        }

        // Movement
        if (isDashing)
        {
            velocity = dashDirection * dashSpeed;
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0)
                isDashing = false;
        }
        else
        {
            if (grounded)
            {
                Vector3 targetVelocity = inputDir * moveSpeed;
                velocity = Vector3.MoveTowards(velocity, targetVelocity, moveSnapSpeed * Time.deltaTime);
            }
            else
            {
                Vector3 targetVelocity = inputDir * moveSpeed;
                velocity = Vector3.MoveTowards(velocity, targetVelocity, moveSnapSpeed * airControlFactor * Time.deltaTime);
            }
        }

        // Jumping & Gravity
        if (grounded)
        {
            if (Input.GetButtonDown("Jump"))
                yVelocity = jumpForce;
            else
                yVelocity = -1f;
        }
        else
        {
            yVelocity += gravity * Time.deltaTime;
        }

        Vector3 move = velocity + Vector3.up * yVelocity;
        controller.Move(move * Time.deltaTime);

        // FOV kick during dash
        if (Camera.main != null)
        {
            float targetFOV = isDashing ? dashFOV : normalFOV;
            Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFOV, fovLerpSpeed * Time.deltaTime);
        }

        // Head bobbing
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
                cameraHolder.localPosition = Vector3.Lerp(cameraHolder.localPosition, camStartLocalPos, bobLerpSpeed * Time.deltaTime);
                bobTimer = 0f;
            }

            // DUSK-style camera tilt based on sideways input only
            float sidewaysInput = Input.GetAxisRaw("Horizontal");
            float targetTilt = -sidewaysInput * maxTiltAngle;

            Quaternion targetRotation = Quaternion.Euler(
                camStartLocalRot.eulerAngles.x,
                camStartLocalRot.eulerAngles.y,
                targetTilt
            );

            cameraHolder.localRotation = Quaternion.Lerp(
                cameraHolder.localRotation,
                targetRotation,
                tiltLerpSpeed * Time.deltaTime
            );
        }

        // Landing camera bounce
        if (landingBounce != null)
        {
            landingBounce.SetGrounded(grounded);
        }
    }
}
