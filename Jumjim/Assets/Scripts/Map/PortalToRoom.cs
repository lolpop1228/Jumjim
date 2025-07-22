using UnityEngine;

public class PortalToRoom : MonoBehaviour
{
    [Header("Room Settings")]
    public GameObject[] roomPrefabs;        // Array of room prefabs
    public Transform spawnOffset;           // Where to spawn the new room
    public bool destroyCurrentRoom = true;  // Whether to destroy the room this portal is in
    public bool teleportPlayerToEntrance = true;

    private int lastRoomIndex = -1; // Track last room to avoid repeats

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            SpawnRoom(other.gameObject);
        }
    }

    void SpawnRoom(GameObject player)
    {
        if (roomPrefabs == null || roomPrefabs.Length == 0) return;

        // Pick a random room index, different from last one
        int randomIndex = GetRandomRoomIndex();
        GameObject randomRoomPrefab = roomPrefabs[randomIndex];
        lastRoomIndex = randomIndex; // Update last room index

        // Calculate spawn position
        Vector3 spawnPosition = spawnOffset != null ? spawnOffset.position : transform.position + Vector3.forward * 50f;

        // Instantiate the new room
        GameObject newRoom = Instantiate(randomRoomPrefab, spawnPosition, Quaternion.identity);

        // Find entrance point in new room
        Transform entrance = null;
        Room roomComponent = newRoom.GetComponent<Room>();
        if (roomComponent != null)
        {
            entrance = roomComponent.roomEntrancePoint;
        }

        // Teleport player and rotate to face entrance
        if (teleportPlayerToEntrance && player != null && entrance != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = entrance.position;
            player.transform.rotation = Quaternion.LookRotation(entrance.forward);

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

    int GetRandomRoomIndex()
    {
        if (roomPrefabs.Length == 1) return 0; // Only one room available

        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, roomPrefabs.Length);
        }
        while (randomIndex == lastRoomIndex);

        return randomIndex;
    }
}
