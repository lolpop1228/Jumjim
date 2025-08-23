using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;
    public GameObject settingsMenu;
    public GameObject weaponHolder;
    public GameObject controlsMenu;
    bool isPause = false;

    void Start()
    {
        if (pauseMenu != null)
        {
            pauseMenu.SetActive(false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1f;
        AudioListener.pause = false; // NEW
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            isPause = !isPause; // flip the state
            pauseMenu.SetActive(isPause);
            weaponHolder.SetActive(!isPause);
            if (isPause)
            {
                Time.timeScale = 0f;
                Cursor.lockState = CursorLockMode.None; // unlock
                Cursor.visible = true; // show
                AudioListener.pause = true; // NEW - mute audio
            }
            else
            {
                Time.timeScale = 1f;
                Cursor.lockState = CursorLockMode.Locked; // lock
                Cursor.visible = false; // hide
                AudioListener.pause = false; // NEW - unmute audio
            }
        }
    }

    public void Resume()
    {
        pauseMenu.SetActive(false);
        weaponHolder.SetActive(true);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        isPause = false;
        AudioListener.pause = false; // NEW
    }

    public void Settings()
    {
        settingsMenu.SetActive(true);
    }

    public void Controls()
    {
        controlsMenu.SetActive(true);
    }

    public void CloseControls()
    {
        controlsMenu.SetActive(false);
    }

    public void Back()
    {
        settingsMenu.SetActive(false);
    }

    public void Quit()
    {
        Debug.Log("Quitting game...");
        Application.Quit();
    }

    public void GoMenu()
    {
        AudioListener.pause = false; // reset audio before switching
        SceneManager.LoadScene("MainMenu");
    }

    public void Restart()
    {
        AudioListener.pause = false; // reset audio before switching
        SceneManager.LoadScene("TestMap");
    }
}
