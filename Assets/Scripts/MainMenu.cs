using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{

    public GameObject loadingScreen;

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

    public void LoadLevel(string sceneName)
    {
        StartCoroutine(LoadSceneAsyncly(sceneName));
    }
    
    public void ShowUI(GameObject uiObject)
    {
        uiObject.SetActive(true);
    }

    public void HideUI(GameObject uiObject)
    {
        uiObject.SetActive(false);
    }

    public static void QuitGame()
    {
        Application.Quit();
    }
}
