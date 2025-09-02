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

    private float realSecondsElapsed;
    private float hungerTimer;

    [Space]
    [Header("In-Game Time")]
    [SerializeField] private int gameDay = 1;
    [SerializeField] private int gameHour = 9;
    [SerializeField] private int gameMinute = 30;
    [SerializeField] private bool isPM = true;

    [Tooltip("Hunger decrease per real-life second")]
    public float hungerPerSecond = 0.0133f; // adjust for desired rate

    public event Action minutePassed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void SelfDestroy()
    {
        Instance = null;
        Destroy(gameObject);
    }

    void Start()
    {
        UIManager.Instance.UpdateTimeUI(gameDay, gameHour, gameMinute, isPM);
    }

    void Update()
    {
        realSecondsElapsed += Time.deltaTime;
        hungerTimer += Time.deltaTime;

        // Advance in-game minute as before
        if (realSecondsElapsed >= RealSecondsToGameMinutes)
        {
            realSecondsElapsed -= RealSecondsToGameMinutes;
            AdvanceMinute();
            UIManager.Instance.UpdateTimeUI(gameDay, gameHour, gameMinute, isPM);
        }

        // Reduce hunger every real-life second
        if (hungerTimer >= 1f)
        {
            hungerTimer -= 1f;
            if (GameManager.Instance != null)
            {
                GameManager.Instance.changeHunger(-hungerPerSecond);
            }
        }
    }

    private void AdvanceMinute()
    {
        gameMinute++;

        if (gameMinute >= 60)
        {
            gameMinute = 0;
            gameHour++;

            if (gameHour > 12) gameHour = 1;

            if (gameHour == 12) isPM = !isPM;

            if (gameHour == 1 && !isPM) gameDay++;
        }

        minutePassed?.Invoke();
    }

    public void skipDay()
    {
        if (!GameObject.FindWithTag("player").GetComponent<PlayerSafeZone>().isTouchingWallBack)
            return;

        UIManager.Instance.onResume();
        Time.timeScale = 1.0f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        for (int i = 0; i < 1440; i++)
        {
            AdvanceMinute();
        }
    }
}
