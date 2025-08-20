using UnityEngine;
using TMPro; // <-- Needed for TextMeshPro

[RequireComponent(typeof(CharacterController))]
public class PlayerDash : MonoBehaviour
{
    [Header("Dash Settings")]
    public float dashSpeed = 30f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 0.5f;
    public KeyCode dashKey = KeyCode.LeftShift;

    [Header("Audio")]
    private AudioSource audioSource;
    public AudioClip dashClip;

    [Header("UI")]
    public TextMeshProUGUI dashCooldownText; // Assign in Inspector

    private CharacterController controller;
    private float lastDashTime = -999f;
    private float dashTimeLeft = 0f;
    private bool isDashing = false;
    private Vector3 dashDirection = Vector3.zero;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();

        if (dashCooldownText != null)
            dashCooldownText.text = ""; // Clear text at start
    }

    void Update()
    {
        // Dash input
        if (Input.GetKeyDown(dashKey) && Time.time >= lastDashTime + dashCooldown)
        {
            Vector3 inputDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
            dashDirection = transform.TransformDirection(inputDir != Vector3.zero ? inputDir : transform.forward);

            if (audioSource != null && dashClip != null)
                audioSource.PlayOneShot(dashClip);

            isDashing = true;
            dashTimeLeft = dashDuration;
            lastDashTime = Time.time;
        }

        // Dash movement
        if (isDashing)
        {
            controller.Move(dashDirection * dashSpeed * Time.deltaTime);
            dashTimeLeft -= Time.deltaTime;
            if (dashTimeLeft <= 0f)
                isDashing = false;
        }

        // Update cooldown text
        UpdateCooldownUI();
    }

    void UpdateCooldownUI()
    {
        if (dashCooldownText == null) return;

        float timeSinceLastDash = Time.time - lastDashTime;
        float cooldownRemaining = Mathf.Clamp(dashCooldown - timeSinceLastDash, 0f, dashCooldown);

        if (cooldownRemaining > 0f)
            dashCooldownText.text = cooldownRemaining.ToString("F1") + "s"; // shows like "0.3s"
        else
            dashCooldownText.text = ""; // Clear when ready
    }

    public bool IsDashing()
    {
        return isDashing;
    }
}
