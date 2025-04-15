using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Singleton for evaluating contamination levels within game environment
public class ContaminationManager : MonoBehaviour
{
    [SerializeField]
    private ContaminationSettings settings;
    //Remove the serialization of this when we have an actual gameplay indicator for contamination levels
    [SerializeField]
    [Tooltip("Contamination level as a total percentage")]
    private float level;
    [SerializeField]
    [Tooltip("Contamination thresholds to determine gameplay changes")]
    //Defined as a sorted set to enforce unique thresholds and proper behavior order from passing them
    //In cases where there would need to be a repeat threshold value, that could be replaced by just triggering multiple behaviors elsewhere upon hitting the threshold
    //Defensive measure in the event that designers do not enter (n1, n2, n3 ... , nk) values in the settings where n_(i+1) > n_i
    private SortedSet<float> thresholds;
    

    //Each contaminable object will be registered with an individual contamination level
    private Dictionary<Contaminatable, float> contaminables = new Dictionary<Contaminatable, float>();
    private float flatLevel = 0;
    private float totalFlatLevel = 0;

    public event System.Action<float> thresholdPassed;
    public static ContaminationManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("ContaminationManager duplicate instance found and removed.");
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (settings == null)
        {
            Debug.LogError("No contamination settings found.");
        }
        //Set the initial contamination and corresponding thresholds according to preset (initial value will likely only ever be 0 in actual gameplay, can vary for testing)
        else
        {
            level = settings.initialContaminationLevel;
            thresholds = new SortedSet<float>(settings.contaminationThresholds);
        }
        //Defensive measure for the dict contents
        contaminables.Clear();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //"Contaminable" class is spelled wrong, should replace all instances of this later
    public void AddContaminable (Contaminatable c, float v)
    {
        if (!contaminables.ContainsKey(c))
        {
            contaminables.Add(c, v);
            //Every contaminable object adds another 100 capacity of true contamination to the total
            totalFlatLevel += 100f;
        }
    }

    public void CalculateContaminationLevel(Contaminatable c, float v)
    {
        //Add the change in value to the flat contamination level before updating the object's entry
        flatLevel += v - contaminables[c];
        if (flatLevel > totalFlatLevel) flatLevel = Mathf.Clamp(flatLevel, 0f, totalFlatLevel);
        contaminables[c] = v;
        level = flatLevel / totalFlatLevel;
        CheckThresholds();
    }
    //Ensures that checkpoints can be passed simultaneously but only ever once
    //Gameplay effects of passing these thresholds is defined elsewhere
    public void CheckThresholds()
    {
        Queue<float> passedThresholds = new Queue<float>();
        foreach(float checkpoint in thresholds)
        {
            if (level >= checkpoint)
            {
                passedThresholds.Enqueue(checkpoint);
            }
            else
            {
                break;
            }

            while (passedThresholds.Count > 0)
            {
                float passed = passedThresholds.Dequeue();
                thresholds.Remove(passed);
                thresholdPassed?.Invoke(passed);
            }
        }
    }
}
