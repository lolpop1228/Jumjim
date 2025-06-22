using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public GameObject[] roomPrefabs;
    public GameObject portalPrefab;
    public GameObject player;

    private Room currentRoom;
    private Vector3 lastRoomPosition = Vector3.zero;
    private float roomSpacing = 50f;

    void Start()
    {
        SpawnNextRoom(); // spawn the first room at game start
    }

    public void SpawnNextRoom()
    {
        // 1. Choose a random room
        GameObject roomPrefab = roomPrefabs[Random.Range(0, roomPrefabs.Length)];
        Vector3 spawnPosition = lastRoomPosition + Vector3.forward * roomSpacing;

        // 2. Spawn the room
        GameObject roomObj = Instantiate(roomPrefab, spawnPosition, Quaternion.identity);
        Room newRoom = roomObj.GetComponent<Room>();

        // 3. Move player to new room's entrance point
        if (newRoom.roomEntrancePoint != null && player != null)
        {
            CharacterController cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;

            player.transform.position = newRoom.roomEntrancePoint.position;

            if (cc != null) cc.enabled = true;
        }

        // 4. Spawn portal at the exit
        if (newRoom.portalSpawnPoint != null)
        {
            GameObject portal = Instantiate(portalPrefab, newRoom.portalSpawnPoint.position, Quaternion.identity);
            portal.GetComponent<Portal>().SetGenerator(this);
        }

        // 5. Update current room & last position
        currentRoom = newRoom;
        lastRoomPosition = spawnPosition;
    }
}
