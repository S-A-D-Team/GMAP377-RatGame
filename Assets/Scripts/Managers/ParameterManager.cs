using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ParameterManager : MonoBehaviour
{
    [Header("Mutation Toggles")]
    public List<string> mutations = new List<string> { "Bite", "Climb", "Jump Boost", "Speed Boost" };

    [Header("RatStat Sliders")]
    public List<string> ratStatNames = new List<string> { "Jump", "Speed", "AirSpeed", "Hunger Multiplier", "Stamina Multiplier" };
    public List<float> ratStatMinValues = new List<float> { 5f, 5f, 1f, 0.5f, 0.5f };
    public List<float> ratStatMaxValues = new List<float> { 20f, 20f, 10f, 4f, 4f };

    [Header("FoodStat Sliders")]
    public List<string> potencyStatNames = new List<string>
    {
        "LowPotency",
        "MedPotency",
        "HighPotency"
    };
    public List<float> potencyStatMinValues = new List<float> { 1f, 1f, 1f };
    public List<float> potencyStatMaxValues = new List<float> { 15f, 15f, 15f };

    [Header("Prefabs")]
    public Toggle togglePrefab;
    public GameObject togglestackPrefab;
    public Slider wholePrefab;
    public Slider floatPrefab;

    [Header("UI Parents")]
    public GameObject mutationPanel;
    public GameObject statsPanel;
    public GameObject environmentPanel;

    [Header("UI Buttons")]
    public Button mutationButton;
    public Button statsButton;
    public Button environmentButton;

    private RatStats ratStats;
    private ContaminationManager contaminationManager;

    // Store the current values for potency stats (initialize with min values)
    public List<float> potencyStatCurrentValues = new List<float> { 1f, 1f, 1f };

    private void Start()
    {
        // Attach panel toggle listeners to buttons
        if (mutationButton != null)
            mutationButton.onClick.AddListener(ToggleMutationPanel);
        if (statsButton != null)
            statsButton.onClick.AddListener(ToggleStatsPanel);
        if (environmentButton != null)
            environmentButton.onClick.AddListener(ToggleEnvironmentPanel);

        // Hide panels at start
        mutationPanel.SetActive(false);
        statsPanel.SetActive(false);
        environmentPanel.SetActive(false);

        // Cache reference to RatStats on player
        var player = GameObject.FindWithTag("player");
        if (player != null)
            ratStats = player.GetComponent<RatStats>();

        contaminationManager = FindObjectOfType<ContaminationManager>();
        if (contaminationManager == null)
        Debug.LogWarning("ContaminationManager not found in scene.");


        // Populate all UI panels directly
        PopulateMutationToggles();
        PopulateRatStatSliders();
        PopulateFoodStatSliders();
    }

    public void ToggleMutationPanel()
    {
        mutationPanel.SetActive(!mutationPanel.activeSelf);
    }

    public void ToggleStatsPanel()
    {
        statsPanel.SetActive(!statsPanel.activeSelf);
    }

    public void ToggleEnvironmentPanel()
    {
        environmentPanel.SetActive(!environmentPanel.activeSelf);
    }

    void PopulateMutationToggles()
    {
        foreach (string mutation in mutations)
        {
            GameObject stackObj = null;
            Toggle newToggle = null;
            Slider stacksSlider = null;
            TextMeshProUGUI[] sliderTexts = null;

            // Use togglestackPrefab for boosts, togglePrefab for others
            if (mutation == "Jump Boost" || mutation == "Speed Boost")
            {
                stackObj = Instantiate(togglestackPrefab, mutationPanel.transform);
                newToggle = stackObj.GetComponentInChildren<Toggle>(true);
                stacksSlider = stackObj.GetComponentInChildren<Slider>(true);

                if (stacksSlider != null)
                {
                    stacksSlider.minValue = 1;
                    stacksSlider.maxValue = 10;
                    stacksSlider.value = 1;
                    stacksSlider.wholeNumbers = true;
                    stacksSlider.gameObject.SetActive(false);

                    sliderTexts = stacksSlider.GetComponentsInChildren<TextMeshProUGUI>();
                    if (sliderTexts.Length >= 2)
                    {
                        sliderTexts[0].text = "Stack";
                        sliderTexts[1].text = "1";
                    }

                    stacksSlider.onValueChanged.AddListener((val) =>
                    {
                        if (sliderTexts != null && sliderTexts.Length >= 2)
                            sliderTexts[1].text = ((int)val).ToString();

                        // Call GameManager stack change
                        switch (mutation)
                        {
                            case "Jump Boost":
                                GameManager.Instance.onJumpStackChange((int)val);
                                break;
                            case "Speed Boost":
                                GameManager.Instance.onSpeedStackChange((int)val);
                                break;
                        }

                        HandleMutationStacksSlider(mutation, (int)val);
                    });
                }
            }
            else
            {
                newToggle = Instantiate(togglePrefab, mutationPanel.transform);
            }

            // Set toggle label
            newToggle.onValueChanged.RemoveAllListeners();
            newToggle.GetComponentInChildren<Text>().text = mutation;

            // Set toggle state based on RatStats (if available)
            bool isOn = false;
            if (ratStats != null)
            {
                switch (mutation)
                {
                    case "Bite":
                        isOn = ratStats.canBite;
                        break;
                    case "Climb":
                        // GameManager.Instance.onClimbToggle(isOn); // REMOVE THIS LINE
                        break;
                    /*
                    case "Speed Boost":
                        GameManager.Instance.onSpeedStackChange((int)val);
                        break;
                    case "Jump Boost":
                        GameManager.Instance.onJumpStackChange((int)val);
                        break;
                    */ 
                }
            }
            newToggle.isOn = isOn;

            // Show/hide stack slider when toggled
            if (stacksSlider != null)
            {
                newToggle.onValueChanged.AddListener((toggleState) =>
                {
                    stacksSlider.gameObject.SetActive(toggleState);
                    // Only show/hide the slider, do NOT call GameManager here
                    HandleMutationToggle(mutation, toggleState);
                });
            }
            else
            {
                newToggle.onValueChanged.AddListener((toggleState) =>
                {
                    HandleMutationToggle(mutation, toggleState);
                });
            }
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
                GameManager.Instance.onJumpStackChange(isOn);
                break;
            case "Speed Boost":
                GameManager.Instance.onSpeedStackChange(isOn);
                break;
            */
            default:
                Debug.LogWarning($"No handler for mutation '{mutation}'");
                break;
        }
    }

    //Handles logic when a mutation stack slider is changed
    void HandleMutationStacksSlider(string mutation, int stacks)
    {
        Debug.Log($"{mutation} stacks set to {stacks}");
        // Apply stacks value to your logic as needed
    }

    void PopulateRatStatSliders()
    {
        for (int i = 0; i < ratStatNames.Count; i++)
        {
            bool isMultiplier = ratStatNames[i].Contains("Multiplier");
            Slider newSlider = isMultiplier
                ? Instantiate(floatPrefab, statsPanel.transform)
                : Instantiate(wholePrefab, statsPanel.transform);

            float minValue = ratStatMinValues.Count > i ? ratStatMinValues[i] : newSlider.minValue;
            float maxValue = ratStatMaxValues.Count > i ? ratStatMaxValues[i] : newSlider.maxValue;

            newSlider.wholeNumbers = !isMultiplier;
            newSlider.minValue = minValue;
            newSlider.maxValue = maxValue;

            float statValue = minValue;
            if (ratStats != null)
            {
                switch (ratStatNames[i])
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
                texts[0].text = ratStatNames[i];
                texts[1].text = isMultiplier ? statValue.ToString("0.0") : statValue.ToString("0");
            }

            string statName = ratStatNames[i];
            newSlider.onValueChanged.AddListener((val) =>
            {
                // Snap to 0.5 increments for multipliers
                float displayVal = val;
                if (isMultiplier)
                {
                    displayVal = Mathf.Round(val * 2f) / 2f;
                    newSlider.SetValueWithoutNotify(displayVal);
                }
                if (texts.Length >= 2)
                    texts[1].text = isMultiplier ? displayVal.ToString("0.0") : displayVal.ToString("0");
                HandleRatStatSlider(statName, displayVal, texts);
            });
        }

        ForceLayoutRebuild(statsPanel);
    }

    void HandleRatStatSlider(string statName, float val, TextMeshProUGUI[] texts)
    {
        if (texts.Length >= 2)
        {
            // Show 0.0 for multipliers, 0 for others
            bool isMultiplier = statName.Contains("Multiplier");
            texts[1].text = isMultiplier ? val.ToString("0.0") : val.ToString("0");
        }

        if (ratStats == null)
        {
            Debug.LogWarning("ratStats not found on player!");
            return;
        }

        switch (statName)
        {
            case "Jump":
                ratStats.maxJumpForce = val;
                break;
            case "Speed":
                ratStats.runSpeed = val;
                ratStats.walkSpeed = val * 0.6f;
                break;
            case "AirSpeed":
                ratStats.airSpeed = val;
                break;
            case "Hunger Multiplier":
                ratStats.hunger = ratStats.hunger + val;
                break;
            case "Stamina Multiplier":
                ratStats.stamina = ratStats.stamina + val;
                break;
            default:
                Debug.LogWarning($"No handler for stat '{statName}'");
                break;
        }
    }

    void PopulateFoodStatSliders()
    {
        for (int i = 0; i < potencyStatNames.Count; i++)
        {
            int idx = i;
            Slider newSlider = Instantiate(wholePrefab, environmentPanel.transform);

            float minValue = potencyStatMinValues.Count > i ? potencyStatMinValues[i] : newSlider.minValue;
            float maxValue = potencyStatMaxValues.Count > i ? potencyStatMaxValues[i] : newSlider.maxValue;

            newSlider.minValue = minValue;
            newSlider.maxValue = maxValue;
            newSlider.value = minValue;

            var texts = newSlider.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                texts[0].text = potencyStatNames[i];
                texts[1].text = minValue.ToString("0");
            }

            newSlider.onValueChanged.AddListener((val) => {
                if (texts.Length >= 2)
                    texts[1].text = val.ToString("0");

                potencyStatCurrentValues[idx] = val;

                // Apply to Contaminable instances through ContaminationManager
                if (contaminationManager != null)
                {
                    Contaminable.potencyLevel potencyLevel = (Contaminable.potencyLevel)(idx + 1); // LOW = 1, MEDIUM = 2, HIGH = 3
                    contaminationManager.SetPotencyForLevel(potencyLevel, val);
                }
                else
                {
                    Debug.LogWarning("ContaminationManager is missing. Cannot apply potency changes.");
                }
            });
        }

        ForceLayoutRebuild(environmentPanel);
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

    // Update this accessor to use the new potency list
    public float GetPotencyValue(Contaminable.potencyLevel potency)
    {
        // 0: LOW, 1: MEDIUM, 2: HIGH
        switch (potency)
        {
            case Contaminable.potencyLevel.LOW: return potencyStatCurrentValues[0];
            case Contaminable.potencyLevel.MEDIUM: return potencyStatCurrentValues[1];
            case Contaminable.potencyLevel.HIGH: return potencyStatCurrentValues[2];
            default: return 1f;
        }
    }
}

