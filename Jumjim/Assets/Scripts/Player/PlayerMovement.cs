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

    void Start()
    {
        dash = GetComponent<PlayerDash>();
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
            else
            {
                yVelocity = -1f;
            }
        }
        else
        {
            yVelocity += gravity * Time.deltaTime;
        }

        Vector3 move = velocity + Vector3.up * yVelocity;
        controller.Move(move * Time.deltaTime);

        // Camera tilt
        if (cameraHolder)
        {
            float horizontalInput = Input.GetAxisRaw("Horizontal");
            float targetTilt = horizontalInput * tiltAmount;
            if (invertTilt) targetTilt = -targetTilt;
            currentTilt = Mathf.Lerp(currentTilt, targetTilt, tiltLerpSpeed * Time.deltaTime);
        }

        // Head bobbing + footstep
        if (cameraHolder)
        {
            bool isMoving = input.magnitude > 0.1f && grounded;

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

            Vector3 currentRotation = cameraHolder.localEulerAngles;
            cameraHolder.localRotation = Quaternion.Euler(currentRotation.x, currentRotation.y, currentTilt);
        }

        if (landingBounce)
            landingBounce.SetGrounded(grounded);
    }
}
