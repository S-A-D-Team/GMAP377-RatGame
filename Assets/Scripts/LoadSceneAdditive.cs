using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneAdditive : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadScene("Pre-Alpha_LivingRoom", LoadSceneMode.Additive);
        SceneManager.LoadScene("Pre-Alpha_Kitchen", LoadSceneMode.Additive);
    }

}
