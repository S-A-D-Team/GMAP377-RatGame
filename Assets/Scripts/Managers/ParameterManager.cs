using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ParameterManager : MonoBehaviour
{
    [Header("Mutation Toggles")]
    public List<string> mutations = new List<string> { "Bite", "Climb", "Jump Boost", "Speed Boost" };

    [Header("Stat Sliders")]
    public List<string> statNames = new List<string> { "Jump", "Speed", "AirSpeed" };
    public List<float> statMinValues = new List<float> { 5, 5, 1 };
    public List<float> statMaxValues = new List<float> { 20, 20, 10 };

    [Header("Prefabs")]
    public Toggle togglePrefab;
    public Slider sliderPrefab;

    [Header("UI Parents")]
    public GameObject mutationPanel;
    public GameObject statsPanel;

    [Header("UI Buttons")]
    public Button mutationButton;
    public Button statsButton;

    private RatStats ratStats;

    private void Start()
    {
        // Attach panel toggle listeners to buttons
        if (mutationButton != null)
            mutationButton.onClick.AddListener(ToggleMutationPanel);
        if (statsButton != null)
            statsButton.onClick.AddListener(ToggleStatsPanel);

        // Hide panels at start
        mutationPanel.SetActive(false);
        statsPanel.SetActive(false);

        // Cache reference to RatStats on player
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
            newToggle.onValueChanged.RemoveAllListeners(); // Prevent duplicate listeners from prefab
            newToggle.GetComponentInChildren<Text>().text = mutation;

            // Set toggle state based on current player RatStats or GameManager
            bool isOn = false;
            if (ratStats != null)
            {
                switch (mutation)
                {
                    case "Bite":
                        isOn = ratStats.canBite;
                        break;
                    case "Climb":
                        GameManager.Instance.onClimbToggle(isOn);
                        break;
                    // Add more cases if you want to reflect other mutations
                    /*
                    case "Speed Boost":
                        Add Speed Boost logic here (Just a way to apply a mutation)
                        break;
                    case "Jump Boost":
                        Add Jump Boost logic here (Just a way to apply a mutation)
                        break;
                    */ 
                }
            }
            newToggle.isOn = isOn;

            // Immediately apply the state to the rat
            HandleMutationToggle(mutation, isOn);

            newToggle.onValueChanged.AddListener((toggleState) => {
                Debug.Log($"Toggle for {mutation} set to {toggleState}");
                HandleMutationToggle(mutation, toggleState);
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

            // Always use current RatStats value if available, otherwise use min
            float statValue = minValue;
            if (ratStats != null)
            {
                switch (statNames[i])
                {
                    case "Jump":
                        statValue = Mathf.Clamp(ratStats.maxJumpForce, minValue, maxValue);
                        break;
                    case "Speed":
                        statValue = Mathf.Clamp(ratStats.runSpeed, minValue, maxValue);
                        break;
                    case "AirSpeed":
                        statValue = Mathf.Clamp(ratStats.airSpeed, minValue, maxValue);
                        break;
                }
            }
            newSlider.value = statValue;

            var texts = newSlider.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                texts[0].text = statNames[i];
                texts[1].text = statValue.ToString("0");
            }

            string statName = statNames[i]; // Capture for closure
            newSlider.onValueChanged.AddListener((val) => HandleStatSlider(statName, val, texts));
        }

        ForceLayoutRebuild(statsPanel);
    }

    void HandleStatSlider(string statName, float val, TextMeshProUGUI[] texts)
    {
        // Update value label
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
                //GameManager.Instance.onJumpStackChange((int)val);
                ratStats.maxJumpForce = val;
                break;
            case "Speed":
                //GameManager.Instance.onSpeedStackChange((int)val);
                ratStats.runSpeed = val;
                ratStats.walkSpeed = val * 0.6f; // Walk speed is always 60% of run speed
                break;
            case "AirSpeed":
                ratStats.airSpeed = val;
                break;
            /*
            case "Hunger":
                // Add hunger logic here if needed
                break;
            case "Stamina":
                // Add stamina logic here if needed
                break; 
            */
            default:
                Debug.LogWarning($"No handler for stat '{statName}'");
                break;
        }
    }

    // Forces a layout rebuild so UI updates immediately
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

