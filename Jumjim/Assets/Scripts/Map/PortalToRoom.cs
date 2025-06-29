using UnityEngine;

public class PortalToRoom : MonoBehaviour
{
    public GameObject roomPrefab;          // The room to spawn
    public Transform spawnOffset;          // Where to spawn the new room
    public bool destroyCurrentRoom = true; // Whether to destroy the room this portal is in
    public bool teleportPlayerToEntrance = true;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SpawnRoom(other.gameObject);
        }
    }

    void SpawnRoom(GameObject player)
    {
        if (roomPrefab == null) return;

        Vector3 spawnPosition = spawnOffset != null ? spawnOffset.position : transform.position + Vector3.forward * 50f;

        // Instantiate the new room
        GameObject newRoom = Instantiate(roomPrefab, spawnPosition, Quaternion.identity);

        // Find entrance point in new room
        Transform entrance = null;
        Room roomComponent = newRoom.GetComponent<Room>();
        if (roomComponent != null)
        {
            entrance = roomComponent.roomEntrancePoint;
        }

        if (teleportPlayerToEntrance && player != null && entrance != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = entrance.position;

            if (cc != null) cc.enabled = true;

            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null) pm.SetYVelocity(0f);
        }

        // Destroy current room (assumes portal is inside the current room)
        if (destroyCurrentRoom)
        {
            Transform parentRoom = transform.root;
            if (parentRoom != null && parentRoom != newRoom.transform)
            {
                Destroy(parentRoom.gameObject);
            }
        }
    }
}
