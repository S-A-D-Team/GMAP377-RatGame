using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class Parameter
{
    private static int nextId = 0;

    public int id;
    public string label;
    public object value;
    public GameObject go;
    public Parameter(string label, bool value, GameObject _go)
    {
        this.id = nextId++;
        this.label = label;
        this.value = value;
        this.go = _go;
    }
    public Parameter(string label, string value, GameObject _go)
    {
        this.id = nextId++;
        this.label = label;
        this.value = value;
        this.go = _go;
    }

    public int getID() 
    {
        return id;
    }
    public string GetDisplayValue()
    {
        if (value is bool boolVal)
            return boolVal ? "true":"false";

        return value?.ToString() ?? string.Empty;
    }
    public bool GetBoolValue()
    {
        if (value is bool boolVal)
            return boolVal;

        return false;
    }
}
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private TextMeshProUGUI gameTime;
    [SerializeField] private TextMeshProUGUI gameDay;

    [Space]

    [SerializeField] private GameObject deathSet;
    [SerializeField] private Image deathNoise;
    [SerializeField] private Image deathRed;

    [SerializeField] private GameObject Parameters_UI_Object;
    [SerializeField] private GameObject template_InputField;
    [SerializeField] private GameObject template_Dropdown;
    [SerializeField] private GameObject template_ToggleBox;
    private List<Parameter> parameters;

    [Space]
    [Header("Pause")]
    [SerializeField] private List<GameObject>  pauseScreens;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        parameters = new List<Parameter>();
    }
    //Thanks ChatG
    private void Start()
    {
        //disable them just in case
        template_InputField.SetActive(false);
        template_Dropdown.SetActive(false);
        template_ToggleBox.SetActive(false);

        // Add a TextField parameter
        int textParamID = AddParameterOptionTextField("Username", "PlayerOne");
        setParamValue(textParamID, "NewPlayerName");
        Debug.Log("Text Param Value: " + getParamValue(textParamID));

        // Add a Dropdown parameter
        List<string> dropdownOptions = new List<string> { "Easy", "Medium", "Hard" };
        int dropdownParamID = AddParameterOptionDropMenu("Difficulty", dropdownOptions, 0);
        setParamValue(dropdownParamID, "Hard");
        Debug.Log("Dropdown Param Value: " + getParamValue(dropdownParamID));

        // Add a Toggle parameter
        int toggleParamID = AddParameterOptionToggleBox("Enable Music", true);
        setParamValue(toggleParamID, false);
        Debug.Log("Toggle Param Value: " + getParamValue(toggleParamID));

        //disable all pause ui elements
        foreach (GameObject _uis in pauseScreens)
        {
            _uis.SetActive(false);
        }
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

    public void onResume()
    {
        foreach (GameObject _uis in pauseScreens)
        {
            _uis.SetActive(false);
        }
        //Invoke
    }

    public void pnPause()
    {
        foreach(GameObject _uis in pauseScreens)
        {
            _uis.SetActive(true);
        }
    }

    public int AddParameterOptionTextField(string label, string _initialValue = "Enter Here...")
    {
        GameObject _temp = Instantiate(template_InputField, Parameters_UI_Object.transform);
        _temp.SetActive(true);

        Parameter newParam = new Parameter(label, _initialValue, _temp);

        _temp.GetComponentInChildren<TextMeshProUGUI>().text = label;
        TMP_InputField inputField = _temp.GetComponentInChildren<TMP_InputField>();
        inputField.text = _initialValue;
        // Sync UI to Parameter
        inputField.onValueChanged.AddListener((string val) =>
        {
            newParam.value = val;
        });

        parameters.Add(newParam);


        return newParam.getID();
    }
    public int AddParameterOptionToggleBox(string label, bool _initialToggle = false)
    {
        GameObject _temp = Instantiate(template_ToggleBox, Parameters_UI_Object.transform);
        _temp.SetActive(true);

        Parameter newParam = new Parameter(label, _initialToggle, _temp);

        _temp.GetComponentInChildren<TextMeshProUGUI>().text = label;
        ToggleBox toggleBox = _temp.GetComponentInChildren<ToggleBox>();
        toggleBox.paramID = newParam.getID();
        toggleBox.Init(_initialToggle);
        // Sync ToggleBox
        toggleBox.onValueChanged += (bool val) =>
        {
            newParam.value = val;
        };

        parameters.Add(newParam);
        return newParam.getID();
    }
    public int AddParameterOptionDropMenu(string label, List<string> options, int initialIndex)
    {
        GameObject _temp = Instantiate(template_Dropdown, Parameters_UI_Object.transform);
        _temp.SetActive(true);

        //Thanks ChatG
        _temp.GetComponentInChildren<TextMeshProUGUI>().text = label;
        TMP_Dropdown dropdown = _temp.GetComponentInChildren<TMP_Dropdown>();
        dropdown.ClearOptions();
        dropdown.AddOptions(options);
        dropdown.value = initialIndex;
        dropdown.RefreshShownValue();

        Parameter newParam = new Parameter(label, options[initialIndex], _temp);
        // Sync UI to Parameter
        dropdown.onValueChanged.AddListener((int index) =>
        {
            newParam.value = options[index];
        });
        parameters.Add(newParam);

        return newParam.getID();
    }

    public object getParamValue(int id)
    {
        foreach (Parameter param in parameters)
        {
            if(param.getID() == id) return param.value;
        }
        return -1;
    }

    public bool setParamValue(int id, object value)
    {
        foreach (Parameter param in parameters)
        {
            if (param.getID() == id)
            {
                param.value = value;

                TMP_InputField inputField = param.go.GetComponentInChildren<TMP_InputField>();
                if (inputField != null)
                {
                    inputField.text = value?.ToString() ?? "";
                    return true;
                }

                TMP_Dropdown dropdown = param.go.GetComponentInChildren<TMP_Dropdown>();
                if (dropdown != null)
                {
                    if (value is string valStr)
                    {
                        int index = dropdown.options.FindIndex(opt => opt.text == valStr);
                        if (index >= 0)
                        {
                            dropdown.value = index;
                            dropdown.RefreshShownValue();
                            return true;
                        }
                    }
                    else if (value is int indexVal && indexVal >= 0 && indexVal < dropdown.options.Count)
                    {
                        dropdown.value = indexVal;
                        dropdown.RefreshShownValue();
                        return true;
                    }
                }

                ToggleBox toggleBox = param.go.GetComponentInChildren<ToggleBox>();
                if (toggleBox != null && value is bool boolVal)
                {
                    toggleBox.value = boolVal;
                    toggleBox.childObject.SetActive(boolVal);
                    return true;
                }

                return false;
            }
        }
        return false;
    }



}
