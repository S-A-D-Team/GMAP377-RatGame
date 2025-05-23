using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ParameterManager : MonoBehaviour
{
    [Header("Mutation Toggles")]
    public List<string> mutations = new List<string> { "Bite", "Climb", "Jump Boost", "Speed Boost" };

    [Header("Stat Sliders")]
    public List<string> statNames = new List<string> { "Jump", "Speed", "Hunger", "Stamina" };
    public List<float> statMinValues = new List<float> { 1, 1, 50, 50 };
    public List<float> statMaxValues = new List<float> { 20, 20, 150, 150 };

    [Header("Prefabs")]
    public Toggle togglePrefab;
    public Slider sliderPrefab;

    [Header("UI Parents")]
    public GameObject mutationPanel;  // Parent for mutation toggles
    public GameObject statsPanel;     // Parent for stat sliders

    [Header("UI Buttons")]
    public Button mutationButton;
    public Button statsButton;

    private RatStats ratStats;

    private void Start()
    {
        // Wire up buttons to toggle panels
        if (mutationButton != null)
            mutationButton.onClick.AddListener(ToggleMutationPanel);
        if (statsButton != null)
            statsButton.onClick.AddListener(ToggleStatsPanel);

        // Optionally start with panels hidden
        mutationPanel.SetActive(false);
        statsPanel.SetActive(false);

        // Get ratStats reference from player
        var player = GameObject.FindWithTag("player");
        if (player != null)
            ratStats = player.GetComponent<RatStats>();

        PopulateMutationToggles();
        PopulateStatSliders();
    }

    public void ToggleMutationPanel()
    {
        mutationPanel.SetActive(!mutationPanel.activeSelf);
    }

    public void ToggleStatsPanel()
    {
        statsPanel.SetActive(!statsPanel.activeSelf);
    }

    void PopulateMutationToggles()
    {
        foreach (string mutation in mutations)
        {
            Toggle newToggle = Instantiate(togglePrefab, mutationPanel.transform);
            newToggle.onValueChanged.RemoveAllListeners(); // Avoid prefab listener bugs
            newToggle.GetComponentInChildren<Text>().text = mutation;
            newToggle.onValueChanged.AddListener((isOn) => {
                Debug.Log($"Toggle for {mutation} set to {isOn}");
                HandleMutationToggle(mutation, isOn);
            });
        }

        ForceLayoutRebuild(mutationPanel);
    }


    void HandleMutationToggle(string mutation, bool isOn)
    {
        switch (mutation)
        {
            case "Bite":
                ratStats.canBite = isOn;
            break;
            
            case "Climb":
                GameManager.Instance.onClimbToggle(isOn);
                break;
            /* 
            case "Jump Boost":
                GameManager.Instance.onJumpBoostToggle(isOn);
                break;
            case "Speed Boost":
                GameManager.Instance.onSpeedBoostToggle(isOn);
                break;
            */
            default:
                Debug.LogWarning($"No handler for mutation '{mutation}'");
                break;
        }
    }

    void PopulateStatSliders()
    {
        for (int i = 0; i < statNames.Count; i++)
        {
            Slider newSlider = Instantiate(sliderPrefab, statsPanel.transform);

            float minValue = statMinValues.Count > i ? statMinValues[i] : newSlider.minValue;
            float maxValue = statMaxValues.Count > i ? statMaxValues[i] : newSlider.maxValue;

            newSlider.minValue = minValue;
            newSlider.maxValue = maxValue;
            newSlider.value = minValue;

            var texts = newSlider.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                texts[0].text = statNames[i];
                texts[1].text = minValue.ToString("0");
            }

            string statName = statNames[i]; // capture variable for closure
            newSlider.onValueChanged.AddListener((val) => HandleStatSlider(statName, val, texts));
        }

        ForceLayoutRebuild(statsPanel);
    }

    void HandleStatSlider(string statName, float val, TextMeshProUGUI[] texts)
    {
        if (texts.Length >= 2)
            texts[1].text = val.ToString("0");

        if (ratStats == null)
        {
            Debug.LogWarning("ratStats not found on player!");
            return;
        }

        switch (statName)
        {
            case "Jump":
                GameManager.Instance.onJumpStackChange((int)val);
                break;
            case "Speed":
                GameManager.Instance.onSpeedStackChange((int)val);
                break;
            case "Hunger":
                // hunger logic 
                break;
            case "Stamina":
                // stamina logic 
                break;
            default:
                Debug.LogWarning($"No handler for stat '{statName}'");
                break;
        }
    }


    private void ForceLayoutRebuild(GameObject target)
    {
        bool wasActive = target.activeSelf;
        if (!wasActive) target.SetActive(true);

        var parent = target.transform.parent?.gameObject;
        bool parentWasActive = parent != null && parent.activeSelf;
        if (parent != null && !parentWasActive) parent.SetActive(true);

        Canvas.ForceUpdateCanvases();
        if (parent != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(parent.GetComponent<RectTransform>());
        else
            LayoutRebuilder.ForceRebuildLayoutImmediate(target.GetComponent<RectTransform>());

        if (!wasActive) target.SetActive(false);
        if (parent != null && !parentWasActive) parent.SetActive(false);
    }
}

