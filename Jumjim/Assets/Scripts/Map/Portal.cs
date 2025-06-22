using UnityEngine;

public class Portal : MonoBehaviour
{
    private DungeonGenerator generator;
    private bool triggered = false;

    public void SetGenerator(DungeonGenerator gen)
    {
        generator = gen;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (triggered || !other.CompareTag("Player")) return;

        triggered = true;
        generator.SpawnNextRoom();
        Destroy(gameObject); // Destroy this portal after it's used
    }
}
