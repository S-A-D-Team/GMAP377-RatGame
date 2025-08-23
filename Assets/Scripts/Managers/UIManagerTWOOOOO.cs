using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManagerTWOOOOO : MonoBehaviour
{
    public GameObject SettingObject;

    void Start()
    {
        // Play background music using MainAudioManager
        if (MainAudioManager.Instance != null)
        {
            MainAudioManager.Instance.PlayMusic("Main"); // Name must match your MusicBundle
        }
    }

    public IEnumerator startReload(float time)
    {
        yield return new WaitForSeconds(time);
        ReloadScene();
    }

    public IEnumerator winCaseQuitToMenu(float time)
    {
        yield return new WaitForSeconds(time);
        QuitGame();
    }

    public static void ReloadScene()
    {
        Time.timeScale = 1f;

        // Stop music before reloading
        if (MainAudioManager.Instance != null)
        {
            MainAudioManager.Instance.StopMusic();
        }

        // Destroy singletons if needed
        GameManager.Instance?.SelfDestroy();
        UIManager.Instance?.SelfDestroy();
        GameTimer.Instance?.SelfDestroy();
        ContaminationManager.Instance?.SelfDestroy();

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void QuitGame()
    {
        // Stop music before going to main menu
        if (MainAudioManager.Instance != null)
        {
            MainAudioManager.Instance.StopMusic();
        }

        GameManager.Instance?.SelfDestroy();
        UIManager.Instance?.SelfDestroy();
        GameTimer.Instance?.SelfDestroy();
        ContaminationManager.Instance?.SelfDestroy();

        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
    
    // Settings UI
    public void SettingOpen()
    {
        SettingObject.SetActive(true);
    }

    public void SettingClose()
    {
        SettingObject.SetActive(false);
    }
}
