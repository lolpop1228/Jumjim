using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndMenu : MonoBehaviour
{
    private void Start()
    {
        Time.timeScale = 1.0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    public void BackToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void Continue()
    {
        SceneManager.LoadScene("TestMap");
    }
}
