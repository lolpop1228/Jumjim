using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public GameObject startRoomPrefab;     // First room to always spawn
    public GameObject[] roomPrefabs;       // Pool of random rooms for the rest
    public GameObject portalPrefab;        // The exit portal prefab
    public GameObject player;              // Reference to the player

    private Room currentRoom;
    private Vector3 lastRoomPosition = Vector3.zero;
    private float roomSpacing = 50f;

    private bool hasSpawnedStartRoom = false;

    void Start()
    {
        SpawnNextRoom(); // Spawn start room first
    }

    public void SpawnNextRoom()
    {
        GameObject roomPrefab;

        // 1. Choose the start room for the first room, then random after that
        if (!hasSpawnedStartRoom && startRoomPrefab != null)
        {
            roomPrefab = startRoomPrefab;
            hasSpawnedStartRoom = true;
        }
        else
        {
            roomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Length)];
        }

        // 2. Calculate spawn position
        Vector3 spawnPosition = lastRoomPosition + Vector3.forward * roomSpacing;

        // 3. Instantiate room
        GameObject roomObj = Instantiate(roomPrefab, spawnPosition, Quaternion.identity);
        Room newRoom = roomObj.GetComponent<Room>();

        // 4. Teleport player to room's entrance
        if (newRoom.roomEntrancePoint != null && player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = newRoom.roomEntrancePoint.position;

            if (cc != null) cc.enabled = true;

            PlayerMovement pm = player.GetComponent<PlayerMovement>();
            if (pm != null) pm.SetYVelocity(0f);
        }

        // 5. Spawn portal at exit
        if (newRoom.portalSpawnPoint != null)
        {
            GameObject portal = Instantiate(portalPrefab, newRoom.portalSpawnPoint.position, Quaternion.identity);
            portal.GetComponent<Portal>().SetGenerator(this);
        }

        // 6. Update last position
        currentRoom = newRoom;
        lastRoomPosition = spawnPosition;
    }
}
