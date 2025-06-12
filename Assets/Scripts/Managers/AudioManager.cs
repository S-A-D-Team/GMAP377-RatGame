using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; } //creates the instance 
    public AudioClip deATHsoUND; //audio clip of death sound

    private void Awake()
    {
        //makes sure there is only one instance of AudioManager running
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    //kills the Inatance
    public void SelfDestroy()
    {
        Instance = null;
        Destroy(gameObject);
    }
    public void playDeath()
    {

    }
}
