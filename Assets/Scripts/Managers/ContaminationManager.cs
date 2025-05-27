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

    [SerializeField]
    [Tooltip("Leave contaminable potency up to level design or randomness")]
    private bool useRandomPotency = true;
    

    //Each contaminable object will be registered with an individual contamination level
    private Dictionary<Contaminable, float> contaminables = new Dictionary<Contaminable, float>();
    private float flatLevel = 0;
    private float totalFlatLevel = 0;
    private int mutationPoints = 0;
    [SerializeField]
    [Tooltip("Baseline point requirement to gain a mutation (scales)")]
    private int mutationRequirements = 3;

    public event System.Action<float, bool> thresholdPassed;
    //Subscribe the human agent to this so they go toward a designated infected item to trigger a win
    public event System.Action<Contaminable> winConActive;
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

    public void SelfDestroy()
    {
        Instance = null;
        Destroy(gameObject);
    }

    
    public void AddContaminable (Contaminable c, float v)
    {
        if (!contaminables.ContainsKey(c))
        {
            contaminables.Add(c, v);
            //Every contaminable object adds another 100 capacity of true contamination to the total
            totalFlatLevel += 100f;
            //Reassign a random potency value to the contaminable if toggled on
            if (useRandomPotency)
            {
                int rn = Random.Range(1, 4);
                c.potency = (Contaminable.potencyLevel)rn;
            }
        }
    }

    public void CalculateContaminationLevel(Contaminable c, float v)
    {
        //Add the change in value to the flat contamination level before updating the object's entry
        flatLevel += v - contaminables[c];
        if (flatLevel > totalFlatLevel) flatLevel = Mathf.Clamp(flatLevel, 0f, totalFlatLevel);
        contaminables[c] = v;
        level = (flatLevel / totalFlatLevel) * 7.5f;
        UIManager.Instance.changeContaminationBar(level);
        CheckThresholds();
    }

    //Deprecated for now
    //Ensures that checkpoints can be passed simultaneously but only ever once
    //Gameplay effects of passing these thresholds is defined elsewhere
    public void CheckThresholds()
    {
        Queue<float> passedThresholds = new Queue<float>();
        foreach(float checkpoint in thresholds)
        {
            if (level >= (checkpoint / 100f))
            {
                passedThresholds.Enqueue(checkpoint);
            }
            else
            {
                break;
            }
        }
        while (passedThresholds.Count > 0)
        {
            float passed = passedThresholds.Dequeue();
            thresholds.Remove(passed);
            bool winConPassed = passedThresholds.Count == 0;
            thresholdPassed?.Invoke(passed, winConPassed);
        }
    }

    public void AddMutationPoints(int points)
    {
        mutationPoints += points;
        if (mutationPoints >= mutationRequirements)
        {
            flatLevel -= mutationRequirements;
            GameManager.Instance.RandomMutate();
            MutationLevelUp();
        }
    }

    public void ActivateWinCondition(Contaminable c)
    {
        winConActive?.Invoke(c);
    }

    /*Scale the mutation reqs based on level tiers
     * Level 1-3: 3, 6, 9
     * Level 4-6: 15, 21, 27
     * Level 7-9: 37, 47, 57
     * Level 10: 70
     * Level 11+: 70 + ((Level - 10) * 15) //15 for each level beyond 10
     */
    public void MutationLevelUp()
    {
        int mutationLevel = GameManager.Instance.ratStats.mutationLevel;
        mutationLevel++;
        if (mutationLevel >= 1 && mutationLevel <= 3)
        {
            mutationRequirements += 3;
        }
        else if (mutationLevel >= 4 && mutationLevel <= 6)
        {
            mutationRequirements += 6;
        }
        else if (mutationLevel >= 7 && mutationLevel <= 8)
        {
            mutationRequirements += 10;
        }
        else if (mutationLevel == 9)
        {
            mutationRequirements += 13;
        }
        else
        {
            mutationRequirements += (mutationLevel - 10) * 15;
        }

    }
}
