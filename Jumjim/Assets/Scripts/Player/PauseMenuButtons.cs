using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenuButtons : MonoBehaviour
{
    public void Resume()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked; // lock
        Cursor.visible = false; // hide
    }
}
