using TMPro;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    public TextMeshProUGUI timeResult;
    private GameTimer timer;

    private void Start()
    {
        // Auto-find the GameTimer in the scene
        timer = FindAnyObjectByType<GameTimer>();
    }

    private void OnEnable()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (timeResult != null && timer != null)
        {
            // ✅ Show the stored final time
            timeResult.text = "Time: " + timer.finalTime;
        }
    }
}
