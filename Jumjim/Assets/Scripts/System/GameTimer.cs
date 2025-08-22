using UnityEngine;
using TMPro; // Needed for UI text

public class GameTimer : MonoBehaviour
{
    [Header("Timer Settings")]
    public bool countDown = true;       // true = countdown, false = count up
    public float startMinutes = 1f;     // Starting time in minutes

    private float currentTime;          // Timer value in seconds
    private bool isRunning = true;      // Control if timer is active
    private bool hasEnded = false;      // Ensure EndTimer runs only once

    [Header("UI")]
    public TextMeshProUGUI timerText;   // Optional UI text (assign in inspector)

    // 🔹 Public variable to store final time (for GameOver to read)
    public string finalTime { get; private set; }

    void Start()
    {
        currentTime = startMinutes * 60f; // Convert minutes to seconds
    }

    void Update()
    {
        if (!isRunning) return; // If timer stopped, do nothing

        if (countDown)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0 && !hasEnded)
            {
                currentTime = 0;
                EndTimer();
            }
        }
        else
        {
            currentTime += Time.deltaTime;
        }

        DisplayTime(currentTime);
    }

    void DisplayTime(float timeToDisplay)
    {
        int minutes = Mathf.FloorToInt(timeToDisplay / 60f);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60f);
        int milliseconds = Mathf.FloorToInt((timeToDisplay * 1000f) % 1000f);

        string formattedTime = string.Format("{0:00}:{1:00}:{2:000}", minutes, seconds, milliseconds);

        if (timerText != null)
            timerText.text = formattedTime;

        if (!hasEnded) // only update live while running
            finalTime = formattedTime;
    }

    // 🔹 Called when the timer ends
    public void EndTimer()
    {
        isRunning = false;
        hasEnded = true;

        // Capture the final formatted time once
        finalTime = timerText != null ? timerText.text : finalTime;

        Debug.Log("⏱ Timer ended at: " + finalTime);
    }

    public void StartTimer()
    {
        isRunning = true;
        hasEnded = false;
    }

    public float GetTimeInSeconds()
    {
        return currentTime;
    }
}
