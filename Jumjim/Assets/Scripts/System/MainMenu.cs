using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string sceneToStart;
    public GameObject controlsUI;
    public GameObject creditsUI;

    public void StartGame()
    {
        SceneManager.LoadScene(sceneToStart);
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void OpenControls()
    {
        controlsUI.SetActive(true);
    }

    public void CloseControls()
    {
        controlsUI.SetActive(false);
    }

    public void OpenCredits()
    {
        creditsUI.SetActive(true);
    }

    public void CloseCredits()
    {
        creditsUI.SetActive(false);
    }

}
