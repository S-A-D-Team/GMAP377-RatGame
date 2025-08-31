using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log("HUSHGLKHBDKS " + other.tag);
		//maybe we will need multiplayer?
		if(other.tag.ToLower().Contains("player"))
		{
			Destroy(other.gameObject);
			GameManager.Instance.onPlayerDead();
			StartCoroutine(GameObject.Find("RELOADQUIT").GetComponent<UIManagerTWOOOOO>().startReload(3f));
			MainAudioManager.Instance.PlaySFX("Rat Trap");
            UIManager.Instance.cueDeathUI(3);
        }
	}
}
