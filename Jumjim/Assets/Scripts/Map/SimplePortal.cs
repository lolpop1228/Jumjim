using UnityEngine;

public class SimplePortal : MonoBehaviour
{
    public Transform destinationPoint;
    public string targetTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(targetTag)) return;

        // Disable CharacterController before teleporting
        CharacterController cc = other.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;

        // Teleport the player
        other.transform.position = destinationPoint.position;

        // Re-enable CharacterController
        if (cc != null) cc.enabled = true;

        // Reset falling velocity (if using your PlayerMovement script)
        PlayerMovement pm = other.GetComponent<PlayerMovement>();
        if (pm != null) pm.SetYVelocity(0f);
    }
}
