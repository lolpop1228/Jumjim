using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public GameObject startRoomPrefab; // First room to spawn

    void Start()
    {
        if (startRoomPrefab != null)
        {
            Instantiate(startRoomPrefab, Vector3.zero, Quaternion.identity);
        }
    }
}
