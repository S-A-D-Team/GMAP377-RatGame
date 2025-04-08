using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameTimer : MonoBehaviour
{

    public static GameTimer Instance { get; private set; }

    [Tooltip("How many real life seconds equal to 1 in-gameMinute")]
    public int RealSecondsToGameMinutes = 10;
    //timing
    private float realSecondsElapsed;
    //in-game time
    [Space]
    [Header("In-Game Time")]
    [SerializeField] private int gameDay = 1;
    [SerializeField] private int gameHour = 9;
    [SerializeField] private int gameMinute = 30;
    [SerializeField] private bool isPM = true;


    //action for other objects to listen to when a in-game time passes 
    public event Action minutePassed;

    void OnValidate()
    {
        gameHour = Mathf.Clamp(gameHour, 1, 12);
        gameMinute = Mathf.Clamp(gameMinute, 0, 59);
    }

    private void Awake()
    {
        //Singleton
        if (Instance != null && Instance != this)
        {
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
        UIManager.Instance.UpdateTimeUI(gameDay, gameHour, gameMinute, isPM);
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

            UIManager.Instance.UpdateTimeUI(gameDay, gameHour, gameMinute, isPM);
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

            if (gameHour > 12) //rolloverr
            {
                gameHour = 1;
            }
            //switch AM and PM
            if (gameHour == 12) 
            {
                isPM = !isPM; 
            }
            //increment day when flipping from 11:59 PM to 12:00 AM
            if (gameHour == 1 && !isPM) 
            {
                gameDay++;
            }
        }

        //Let all subsscribers know of minuteIncrease
        minutePassed?.Invoke();
    }

}
