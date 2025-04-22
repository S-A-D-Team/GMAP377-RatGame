using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneAdditive : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        SceneManager.LoadScene("LivingRoom-Alpha1", LoadSceneMode.Additive);
        //and then the kitchen
        //SceneManager.LoadScene("LivingRoom-Alpha1", LoadSceneMode.Additive);
    }

}
