using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerDash : MonoBehaviour
{
    public float dashSpeed = 30f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.5f;
    public KeyCode dashKey = KeyCode.LeftShift;
    private AudioSource audioSource;
    public AudioClip dashClip;

    private CharacterController controller;
    private float lastDashTime = -999f;
    private float dashTimeLeft = 0f;
    private bool isDashing = false;
    private Vector3 dashDirection = Vector3.zero;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(dashKey) && Time.time >= lastDashTime + dashCooldown)
        {
            Vector3 inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
            dashDirection = transform.TransformDirection(inputDir != Vector3.zero ? inputDir : transform.forward);

            if (audioSource != null)
            {
                audioSource.PlayOneShot(dashClip);
            }

            isDashing = true;
            dashTimeLeft = dashDuration;
            lastDashTime = Time.time;
        }

        if (isDashing)
        {
            controller.Move(dashDirection * dashSpeed * Time.deltaTime);
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0f)
            {
                isDashing = false;
            }
        }
    }

    public bool IsDashing()
    {
        return isDashing;
    }
}
