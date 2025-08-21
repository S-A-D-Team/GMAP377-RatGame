using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    /*
    [SerializeField] private TextMeshProUGUI gameTime;
    [SerializeField] private TextMeshProUGUI gameDay;
    */

    [Space]
    [Header("Tutorial")]
    public bool isTutorialActive = false;
    [SerializeField] private TutorialManager tutorialManager;
    [SerializeField] private GameObject tutorialParent;
    [SerializeField] private TextMeshProUGUI tutorialCaption;
    public Image hungerHighlight;
    public Image staminaHighlight;
    public Image mutationHighlight;
    [SerializeField] private GameObject tutorialContinueButton;

    [Space]
    [Header("Indicators")]
    [SerializeField] private Image biteIncicator;
    [SerializeField] private Image climbIndicator; 
    [SerializeField] private Image contaminationIndicator;

    private Animator climbAnimator;

    [Space]
    [Header("Win")]
    [SerializeField] private GameObject WinSet;
    [SerializeField] private List<GameObject> everythingElseToHide;

    [Space]
    [Header("Death")]
    [SerializeField] private GameObject deathSet;
    [SerializeField] private Image deathNoise;
    [SerializeField] private Image deathRed;
    [Space]
    [SerializeField] private GameObject deathCat;
    [SerializeField] private GameObject deathHunger;
    [SerializeField] private GameObject deathTrap;
    [SerializeField] private TextMeshProUGUI deathText;
    [SerializeField] private string deathCaption_Cat;
    [SerializeField] private string deathCaption_Hunger;
    [SerializeField] private string deathCaption_Trap;

    //[SerializeField] private GameObject template_InputField;
    //[SerializeField] private GameObject template_Dropdown;
    //[SerializeField] private GameObject template_Toggle;

    [Space]  
    [SerializeField] private List<GameObject> pauseScreens;

    [Space]
    [SerializeField] private Image hungerBar;
    [SerializeField] private Image StaminaBar;
    [SerializeField] private Image ContaminationBar;
    [SerializeField] private GameObject InteractUI;
    [SerializeField] private GameObject InfectUI;

    [Space]
    public TextMeshProUGUI mutationPointsGainCueText;
    public GameObject mutationPointsGainCue;
    private Queue<string> pendingMutationCues;
    private bool cueRunning = false;
    
    //public event System.Action<List<int>> updateParams;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        pendingMutationCues = new Queue<string>();
        climbAnimator = climbIndicator.gameObject.GetComponent<Animator>();
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

    public void showBiteBuildUp(bool _active, float _completion)
    {
        biteIncicator.gameObject.SetActive(_active);
        biteIncicator.transform.GetChild(0).gameObject.SetActive(_active);
        biteIncicator.transform.GetChild(0).gameObject.GetComponent<Image>().fillAmount = _completion;
    }

    public void showClimb(bool _active)
    {
        climbIndicator.gameObject.SetActive(_active);
    }

    public void playClimb()
    {
        if (climbAnimator.speed == 0f)
        {
            climbAnimator.speed = 1f;
        }
    }

    public void pauseClimb()
    {
        if (climbAnimator.speed != 0f)
        {
            climbAnimator.speed = 0f;
        }
    }
    public void showContainationBuildUp(bool _active, float _completion)
    {
        contaminationIndicator.gameObject.SetActive(_active);
        contaminationIndicator.transform.GetChild(0).gameObject.SetActive(_active);
        contaminationIndicator.transform.GetChild(0).gameObject.GetComponent<Image>().fillAmount = _completion;
    }

    public void cueWinUI()
    {
        WinSet.SetActive(true);
        foreach (GameObject _uiItem in everythingElseToHide)
        {
            _uiItem.SetActive(false);
        }
    }

    public void TriggerDeathEffect()
    {
        deathSet.SetActive(true);
        //indefinately pulse the red
        StartCoroutine(PulseDeathRed());
    }

    /// <summary>
    /// 1 - Cat,
    /// 2 - Hunger,
    /// 3 - Trap
    /// </summary>
    /// <param name="_deathIndex"></param>
    public void cueDeathUI(int _deathIndex)
    {
        deathCat.SetActive(false);
        deathHunger.SetActive(false);
        deathTrap.SetActive(false);
        switch (_deathIndex)
        {
            case 1:
                deathCat.SetActive(true);
                deathText.text = deathCaption_Cat;
                break;
            case 2:
                deathHunger.SetActive(true);
                deathText.text = deathCaption_Hunger;
                break;
            case 3:
                deathTrap.SetActive(true);
                deathText.text = deathCaption_Trap;
                break;
            default:
                break;
        }
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
        /*
        string _formattedTime = $"{_gameHour}:{_gameMinute:D2}";
        _formattedTime += _isPM ? " PM" : " AM";

        gameDay.text = "Day: " + _gameDay.ToString();
        gameTime.text = _formattedTime;
        */
    }
    

    public void onResume()
    {
        foreach (GameObject _uis in pauseScreens)
        {
            _uis.SetActive(false);
        }

        if (!isTutorialActive)
        {
            Time.timeScale = 1.0f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

    }

    public void onPause()
    {
        foreach(GameObject _uis in pauseScreens)
        {
            _uis.SetActive(true);
        }
        
    }

    public void showInteractCue(bool _state)
    {
        InteractUI.SetActive(_state);
    }

    public void showInfectionCue(bool _state)
    {
        InfectUI.SetActive(_state);
    }

    public void changeStaminaBar(float _newvalue)
    {
        StaminaBar.fillAmount = _newvalue;
    }
    public void changeContaminationBar(float _newvalue)
    {
        ContaminationBar.fillAmount = _newvalue;
    }

    public void changeHungerBar(float _change)
    {
        float _value = hungerBar.fillAmount += _change;
        _value = Mathf.Clamp(_value, 0f, 1f);
        hungerBar.fillAmount = _value;
    }

    public void SetTutorialCaption(string _narration)
    {
        tutorialCaption.text = _narration;
    }

    public void cueMutation(string cueText)
    {
        pendingMutationCues.Enqueue(cueText);
        if (!cueRunning)
        {
            StartCoroutine(_cueMutationProcessor());
        }
    }

    private IEnumerator _cueMutationProcessor()
    {
        cueRunning = true; 
        while (pendingMutationCues.Count > 0)
        {
            yield return _cueMutation();
        }
        cueRunning = false;
        
    }

    private IEnumerator _cueMutation()
    {
        mutationPointsGainCueText.text = pendingMutationCues.Dequeue();
        mutationPointsGainCue.SetActive(true);
        yield return new WaitForSeconds(2f);
        mutationPointsGainCue.SetActive(false);
    }

    public void beginTutorial(int _tutorialIndex)
    {
        tutorialParent.SetActive(true);
        tutorialManager.tutorialStage = _tutorialIndex;
        isTutorialActive = true;
        tutorialManager.SetTutorial();
        //enable the cursor and stop
        Time.timeScale = 0.0f;
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }
    public void endTutorial()
    {
        tutorialParent.SetActive(false);
        isTutorialActive = false;
        //continue the game
        Time.timeScale = 1.0f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }
}
