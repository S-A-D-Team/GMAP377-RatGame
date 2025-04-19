using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
	public AudioSource snapAudio;

	private void OnTriggerEnter(Collider other)
	{
		Debug.Log("HUSHGLKHBDKS " + other.tag);
		//maybe we will need multiplayer?
		if(other.tag.ToLower().Contains("player"))
		{
			Destroy(other.gameObject);
			GameManager.Instance.onPlayerTrapped();
			snapAudio.Play();
		}
	}
}
