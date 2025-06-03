using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class LoadSceneAdditive : MonoBehaviour
{
    public GameObject loadingScreen;
    public Image loadingBarFill;
    IEnumerator LoadAdditiveScenesAsync(string[] sceneNames)
    {
        loadingScreen.SetActive(true);

        List<AsyncOperation> operations = new List<AsyncOperation>();

        // Start loading all scenes asynchronously
        foreach (string sceneName in sceneNames)
        {
            AsyncOperation op = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            op.allowSceneActivation = false; //wait until everything is loaded
            operations.Add(op);
        }

        bool allDone = false;
        while (!allDone)
        {
            float totalProgress = 0f;
            allDone = true;

            foreach (var op in operations)
            {
                float progress = Mathf.Clamp01(op.progress / 0.99f); 
                totalProgress += progress;
                if (op.progress < 0.9f)
                    allDone = false;
            }

            float normalizedProgress = 0.5f + (totalProgress / operations.Count) * 0.5f;
            loadingBarFill.fillAmount = normalizedProgress;
            yield return null;
        }

        // Activate scenes if needed
        foreach (var op in operations)
            op.allowSceneActivation = true;

        //disable the screen
        loadingScreen.SetActive(false);
    }


    // Start is called before the first frame update
    void Start()
    {
        loadingScreen.SetActive(true);
        string[] scenesToLoad = { "Pre-Alpha_LivingRoom", "Pre-Alpha_Kitchen", "Foods" };
        StartCoroutine(LoadAdditiveScenesAsync(scenesToLoad));
    }


}
