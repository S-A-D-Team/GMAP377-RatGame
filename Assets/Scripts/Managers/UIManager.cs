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
    public string getLabel()
    {
        return label;
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
    [SerializeField] private PlayerMovement RatMov;

    [SerializeField] private GameObject Parameters_UI_Object;
    //[SerializeField] private GameObject template_InputField;
    //[SerializeField] private GameObject template_Dropdown;
    //[SerializeField] private GameObject template_Toggle;
    private List<Parameter> parameters;
    private List<int> paramIDs;
    private bool paramUIChanged = false;

    [Space]
    [Header("Pause")]
    [SerializeField] private List<GameObject> pauseScreens;

    public event System.Action<List<int>> updateParams;


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
        //template_InputField.SetActive(false);
        //template_Dropdown.SetActive(false);
        //template_Toggle.SetActive(false);

        // Add a TextField parameter
        //int textParamID = AddParameterOptionTextField("Username", "PlayerOne");
        //int jumpMutationTextParamID = AddParameterOptionTextField("Jump");
        //setParamValue(jumpMutationTextParamID, "Stack #");
        //int speedMutationTextParamID = AddParameterOptionTextField("Speed");
        //setParamValue(speedMutationTextParamID, "Stack #");
        //setParamValue(textParamID, "NewPlayerName");
        //Debug.Log("Text Param Value: " + getParamValue(textParamID));

        ///*Add a Dropdown parameter
        //List<string> dropdownOptions = new List<string> { "Easy", "Medium", "Hard" };
        //int dropdownParamID = AddParameterOptionDropMenu("Difficulty", dropdownOptions, 0);
        //setParamValue(dropdownParamID, "Hard");
        //Debug.Log("Dropdown Param Value: " + getParamValue(dropdownParamID));
        //*/

        //// Add a Toggle parameter
        //int toggleClimbID = AddParameterOptionToggle("Climb", false);
        //int toggleBiteID = AddParameterOptionToggle("Bite", false);
        //setParamValue(toggleClimbID, false);
        //setParamValue(toggleBiteID, false);

        //paramIDs = new List<int> { jumpMutationTextParamID, speedMutationTextParamID, toggleClimbID, toggleBiteID };

        //disable all pause ui elements
        foreach (GameObject _uis in pauseScreens)
        {
            _uis.SetActive(false);
        }
    }
    public void SelfDestroy()
    {
        Instance = null;
        Destroy(gameObject);
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
        Time.timeScale = 1.0f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (paramUIChanged)
        {
            updateParams?.Invoke(paramIDs);
        }
        paramUIChanged = false;
        ClearParamUIListeners();
    }

    public void onPause()
    {
        foreach(GameObject _uis in pauseScreens)
        {
            _uis.SetActive(true);
        }
        SetupParamUIListeners();
    }

    private void SetupParamUIListeners()
    {
        paramUIChanged = false;

        foreach (var inputField in Parameters_UI_Object.GetComponentsInChildren<TMP_InputField>())
        {
            inputField.onValueChanged.AddListener((_) => paramUIChanged = true);
        }

        foreach (var toggle in Parameters_UI_Object.GetComponentsInChildren<Toggle>())
        {
            toggle.onValueChanged.AddListener((_) => paramUIChanged = true);
        }

    }

    private void ClearParamUIListeners()
    {
        foreach (var inputField in Parameters_UI_Object.GetComponentsInChildren<TMP_InputField>())
        {
            inputField.onValueChanged.RemoveAllListeners();
        }

        foreach (var toggle in Parameters_UI_Object.GetComponentsInChildren<Toggle>())
        {
            toggle.onValueChanged.RemoveAllListeners();
        }
    }

    /*
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
    public int AddParameterOptionToggle(string label, bool _initialToggle = false)
    {
        GameObject _temp = Instantiate(template_Toggle, Parameters_UI_Object.transform);
        _temp.SetActive(true);

        Parameter newParam = new Parameter(label, _initialToggle, _temp);

        _temp.GetComponentInChildren<TextMeshProUGUI>().text = label;
        Toggle Toggle = _temp.GetComponentInChildren<Toggle>();
        Toggle.onValueChanged.AddListener((bool val) =>
        {
            newParam.value = val;
        });

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
    }*/

    public string getParamName(int id)
    {
        foreach (Parameter param in parameters)
        {
            if (param.getID() == id) return param.label;
        }
        return "null";
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

                Toggle Toggle = param.go.GetComponentInChildren<Toggle>();
                if (Toggle != null && value is bool boolVal)
                {
                    Toggle.isOn = boolVal;
                    return true;
                }

                return false;
            }
        }
        return false;
    }
    [Space]
    [Header("Parameters")]

    public Toggle biteToggle;

    public void onBiteToggleChange()
    {
        GameObject.FindWithTag("player").GetComponent<Hole>().isEnabled = biteToggle.isOn;
        //_ = biteToggle.isOn;
    }

    /*
    public Toggle ClimbToggle;
    public void onClimbToggleChange()
    {
        _ = ClimbToggle.isOn;
    }*/

    public TMP_InputField jumpStack;
    public void onJumpStackChange()
    {
        try
        {
            int newVal = int.Parse(jumpStack.text);
            GameManager.Instance.onJumpStackChange(newVal);
        }
        catch (System.Exception)
        {

            throw;
        }

    }

    public TMP_InputField speedStack;
    public void onspeedStackChange()
    {
        try
        {
            int newVal = int.Parse(speedStack.text);
            GameManager.Instance.onSpeedStackChange(newVal);
        }
        catch (System.Exception)
        {

            throw;
        }

    }



}
