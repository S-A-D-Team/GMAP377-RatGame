using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vaccum : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("player"))
        {
            Destroy(other.gameObject);
            GameManager.Instance.onPlayerDead();
            StartCoroutine(GameObject.Find("RELOADQUIT").GetComponent<UIManagerTWOOOOO>().startReload(3f));
            UIManager.Instance.cueDeathUI(1);
        }
    }
}
