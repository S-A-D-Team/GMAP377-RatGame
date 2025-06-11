using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManagerTWOOOOO : MonoBehaviour
{
    public GameObject controlsScreen;
    public GameObject SettingObject;
    public IEnumerator startReload(float time)
    {
        yield return new WaitForSeconds(time);
        ReloadScene();
    }

    public IEnumerator winCaseQuitToMenu(float time)
    {
        yield return new WaitForSeconds(time);
        quitGame();
    }

    public static void ReloadScene()
    {
        Time.timeScale = 1f;

        // Destroy singletons if needed
        GameManager.Instance?.SelfDestroy();
        UIManager.Instance?.SelfDestroy();
        GameTimer.Instance?.SelfDestroy();
        ContaminationManager.Instance?.SelfDestroy();



        // Or use GameObject.Find and destroy specific persistent objects

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void quitGame()
    {
        // Destroy singletons if needed
        GameManager.Instance?.SelfDestroy();
        UIManager.Instance?.SelfDestroy();
        GameTimer.Instance?.SelfDestroy();
        ContaminationManager.Instance?.SelfDestroy();
        SceneManager.LoadScene("MainMenu");
    }

    //Controls UI
    public void Controls()
    {
        controlsScreen.SetActive(true);
    }
    public void ControlsBack()
    {
        controlsScreen.SetActive(false);
    }
    
    //SettingsUI
    public void SettingOpen()
    {
        SettingObject.SetActive(true);
    }
    public void SettingClose()
    {
        SettingObject.SetActive(false);
    }
}
