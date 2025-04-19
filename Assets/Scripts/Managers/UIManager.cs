using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI gameTime;
    [SerializeField] private TextMeshProUGUI gameDay;

    [Space]

    [SerializeField] private GameObject deathSet;
    [SerializeField] private Image deathNoise;
    [SerializeField] private Image deathRed;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void TriggerDeathEffect()
    {
        deathSet.SetActive(true);
        //indefinately pulse the red
        StartCoroutine(PulseDeathRed());
    }

    //Replace with tween later
    //Thanks chatG!
    private IEnumerator PulseDeathRed()
    {
        float minAlpha = 0.3f;
        float maxAlpha = 0.5f;
        float speed = 1f;

        Color baseColor = deathRed.color;
        float t = 0f;
        bool increasing = true;

        while (true)
        {
            t += Time.deltaTime * speed * (increasing ? 1 : -1);
            float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);
            deathRed.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

            if (t >= 1f)
            {
                t = 1f;
                increasing = false;
            }
            else if (t <= 0f)
            {
                t = 0f;
                increasing = true;
            }

            yield return null;
        }
    }

    public void UpdateTimeUI(int _gameDay, int _gameHour, int _gameMinute, bool _isPM)
    {
        string _formattedTime = $"{_gameHour}:{_gameMinute:D2}";
        _formattedTime += _isPM ? " PM" : " AM";

        gameDay.text = "Day: " + _gameDay.ToString();
        gameTime.text = _formattedTime;
    }
}
