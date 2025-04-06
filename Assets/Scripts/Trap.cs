using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trap : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		//maybe we will need multiplayer?
		if(other.tag.ToLower().Contains("player"))
		{
			Destroy(other.gameObject);
		}
	}
}
