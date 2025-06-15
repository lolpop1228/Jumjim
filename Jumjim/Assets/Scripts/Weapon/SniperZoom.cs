using UnityEngine;

public class SniperZoom : MonoBehaviour
{
    [Header("Zoom Settings")]
    public Camera playerCamera;
    public float normalFOV = 60f;
    public float zoomedFOV = 20f;
    public float zoomSpeed = 10f;

    [Header("Input")]
    public KeyCode zoomKey = KeyCode.Mouse1; // Right click by default

    [Header("Visuals (Optional)")]
    public GameObject weaponModel;
    public GameObject scopeOverlayUI;

    private bool isZoomed = false;

    // ─────────────────────────────────────────────────────────────
    void Start()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        playerCamera.fieldOfView = normalFOV;

        if (scopeOverlayUI)
            scopeOverlayUI.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────
    void Update()
    {
        if (Input.GetKeyDown(zoomKey))
        {
            ToggleZoom();
        }

        float targetFOV = isZoomed ? zoomedFOV : normalFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, Time.deltaTime * zoomSpeed);
    }

    void ToggleZoom()
    {
        isZoomed = !isZoomed;

        if (weaponModel)
            weaponModel.SetActive(!isZoomed); // Hide gun when zoomed

        if (scopeOverlayUI)
            scopeOverlayUI.SetActive(isZoomed); // Show scope UI
    }
}
