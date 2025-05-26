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
    public List<string> foodStatNames = new List<string>
    {
        "LowContamPt",
        "MedContamPt",
        "HighContamPt",
        "LowHunger",
        "MedHunger",
        "HighHunger"
    };
    public List<float> foodStatMinValues = new List<float> { 1f, 1f, 1f, 1f, 1f, 1f };
    public List<float> foodStatMaxValues = new List<float> { 15f, 15f, 15f, 15f, 15f, 15f };

    [Header("Prefabs")]
    public Toggle togglePrefab;
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
            /*
            case "Hunger":
                // Add hunger logic here
                break;
            case "Stamina":
                // Add stamina logic here
                break; 
            */
            default:
                Debug.LogWarning($"No handler for stat '{statName}'");
                break;
        }
    }

    void PopulateFoodStatSliders()
    {
        for (int i = 0; i < foodStatNames.Count; i++)
        {
            Slider newSlider = Instantiate(wholePrefab, environmentPanel.transform);

            float minValue = foodStatMinValues.Count > i ? foodStatMinValues[i] : newSlider.minValue;
            float maxValue = foodStatMaxValues.Count > i ? foodStatMaxValues[i] : newSlider.maxValue;

            newSlider.minValue = minValue;
            newSlider.maxValue = maxValue;

            float statValue = minValue;
            newSlider.value = statValue;

            var texts = newSlider.GetComponentsInChildren<TextMeshProUGUI>();
            if (texts.Length >= 2)
            {
                texts[0].text = foodStatNames[i];
                texts[1].text = statValue.ToString("0");
            }

            string foodName = foodStatNames[i];

            /*
            switch (foodName)
            {
                case "LowContaminationPt":
                    // Add "LowContaminationPt" setup logic here
                    break;
                case "MediumContaminationPt":
                    // Add "MediumContaminationPt" setup logic here
                    break;
                case "HighContaminationPt":
                    // Add "HighContaminationPt" setup logic here
                    break;
                case "LowHungerRefill":
                    // Add "LowHungerRefill" setup logic here
                    break;
                case "MediumHungerRefill":
                    // Add "MediumHungerRefill" setup logic here
                    break;
                case "HighHungerRefill":
                    // Add "HighHungerRefill" setup logic here
                    break;
                default:
                    // No handler for food stat setup
                    break;
            } */

            newSlider.onValueChanged.AddListener((val) => HandleFoodStatSlider(foodName, val, texts));
        }

        ForceLayoutRebuild(environmentPanel);
    }

    void HandleFoodStatSlider(string foodName, float val, TextMeshProUGUI[] texts)
    {
        if (texts.Length >= 2)
            texts[1].text = val.ToString("0");

        switch (foodName)
        {
            case "LowContaminationPt":
                // Add "LowContaminationPt" logic here
                break;
            case "MediumContaminationPt":
                // Add "MediumContaminationPt" logic here
                break;
            case "HighContaminationPt":
                // Add "HighContaminationPt" logic here
                break;
            case "LowHungerRefill":
                // Add "LowHungerRefill" logic here
                break;
            case "MediumHungerRefill":
                // Add "MediumHungerRefill" logic here
                break;
            case "HighHungerRefill":
                // Add "HighHungerRefill" logic here
                break;
            default:
                Debug.LogWarning($"No handler for food stat '{foodName}'");
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

