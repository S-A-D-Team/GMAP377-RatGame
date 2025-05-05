using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutToMain : MonoBehaviour
{
    public string sceneToTransitionTo;

	public void SwitchScene()
    {
        SceneManager.LoadScene(sceneToTransitionTo);
    }
}
