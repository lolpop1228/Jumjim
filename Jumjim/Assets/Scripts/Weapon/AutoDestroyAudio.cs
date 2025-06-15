using UnityEngine;

public class AutoDestroyAudio : MonoBehaviour
{
    void Start()
    {
        AudioSource audio = GetComponent<AudioSource>();
        if (audio != null && audio.clip != null)
        {
            Destroy(gameObject, audio.clip.length);
        }
        else
        {
            Destroy(gameObject, 2f); // fallback destroy time
        }
    }
}
