using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTimer : MonoBehaviour
{

    public static GameTimer Instance { get; private set; }

    [Tooltip("How many real life seconds equal to 1 in-gameMinute")]
    public int RealSecondsToGameMinutes = 10;
    //timing
    private float realSecondsElapsed;
    //in-game time
    [SerializeField]
    private int gameMinute = 0;
    private int gameHour = 0;
    private int gameDay = 1;

    //action for other objects to listen to when a in-game time passes 
    public event Action minutePassed;

    private void Awake()
    {
        //Singleton
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("Another instance already exists! Destroying this one.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        //persistent across scenes
        DontDestroyOnLoad(gameObject);
    }
    //Clear on destroy
    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        realSecondsElapsed += Time.deltaTime;

        //once we cross that threshold in irl seconds
        if (realSecondsElapsed >= RealSecondsToGameMinutes)
        {
            //remove the int part and move a minute
            realSecondsElapsed -= RealSecondsToGameMinutes;
            AdvanceMinute();
        }
    }

    public float getgameMinutesElapsed() { return (gameDay * 24 * 60) + (gameHour * 60) + gameMinute; }
    public float getHour() { return gameHour; }
    public float getDay() { return gameDay; }

    private void AdvanceMinute()
    {
        gameMinute++;

        if (gameMinute >= 60)
        {
            gameMinute = 0;
            gameHour++;

            if (gameHour >= 24)
            {
                gameHour = 0;
                gameDay++;
            }
        }

        //Let all subsscribers know of minuteIncrease
        minutePassed?.Invoke();
    }
}
