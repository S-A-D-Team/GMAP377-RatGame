using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public static void PlayGame()
    {
        SceneManager.LoadScene("main");
    }

    public static void Settings()
    {
        
    }

    public static void QuitGame()
    {
        Application.Quit();
    }
}
