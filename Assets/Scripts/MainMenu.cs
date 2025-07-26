using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject controlsScreen;
    public GameObject loadingScreen;
    public GameObject SettingObject;
    public GameObject LevelSelectScreen;
    public Image loadingBarFill;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    IEnumerator LoadSceneAsyncly(string _scenename)
    {
        AsyncOperation operation = SceneManager.LoadSceneAsync(_scenename);

        loadingScreen.SetActive(true);

        while (!operation.isDone)
        {
            float _progressValue = Mathf.Clamp01(operation.progress / 0.99f);
            loadingBarFill.fillAmount = _progressValue;
            yield return null;
        }
    }

    public void PlayGame()
    {
        //StartCoroutine(LoadSceneAsyncly("Main"));
        LevelSelectScreen.SetActive(true);
    }

    public void LoadLevel(string sceneName)
    {
        StartCoroutine(LoadSceneAsyncly(sceneName));
    }

    public void PlayDemo()
    {
        StartCoroutine(LoadSceneAsyncly("SpringFinal_Demo"));
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

    public void Controls()
    {
        controlsScreen.SetActive(true);
    }
    public void ControlsBack()
    {
        controlsScreen.SetActive(false);
    }
    

    public static void QuitGame()
    {
        Application.Quit();
    }
}
