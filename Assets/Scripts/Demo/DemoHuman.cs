using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class DemoHuman : DemoEnemyAi
{

    private int minTaskTime = 10;
    private int maxTaskTime = 10;

    [SerializeField]
    private int runawayTime;

    [SerializeField]
    private DemoCat cat;

    [SerializeField]
    private GameObject ratTrap;

    [SerializeField]
    private GameObject reactionCanvas;

    protected NavMeshAgent agent;

    private Transform player;

    private bool playerFound;
    private bool isInTask;
    private bool isReacting;
    private bool playerMoving;
    private bool placingTraps;

    private float taskTimer;
    private float locationTime;

    private Vector3 prevTask;
    private Vector3 trapPos;
    private Vector3 secondaryTrapPos;

    private KeyCode killKey = KeyCode.Z;

    public UnityEvent humanDeath;

    [SerializeField] private bool playerSpottedFirstTime = false;

    [SerializeField]
    private int trapsToPlace;

    [Space]
    public List<TaskInfo> HumanTasks;

    [SerializeField]
    private GameObject eyes;

    // Start is called before the first frame update
    void Start()
    { 
        player = base.findPlayer();
        agent = GetComponent<NavMeshAgent>();
        isInTask = false;
        isReacting = false;
        prevTask = new Vector3(0,0,0);
        reactionCanvas.SetActive(false);
        playerMoving = false;
        humanDeath = new UnityEvent();
        placingTraps = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(killKey))
        {
            //StartCoroutine(EatInfected());
        }
        if (!isReacting)
        {
            playerFound = base.isPlayerSighted(player, eyes.transform);
            if (playerFound)
            {
                StartCoroutine(checkMoving());
                if (!playerMoving && !placingTraps)
                {
                    StartCoroutine(timeReaction());
                    Reaction();
                    isReacting = true;
                    StartCoroutine(SecondaryTrapCheck());

                    //if the player has been spotted for the first time, 
                    if (!playerSpottedFirstTime)
                    {
                        playerSpottedFirstTime = true;
                        UIManager.Instance.beginTutorial(5);
                    }

                }
            }
            else if (!isInTask)
            {
                (Vector3, float) _holderVar = base.changeLocation(HumanTasks, agent, prevTask);
                prevTask = _holderVar.Item1;
                minTaskTime = (int)_holderVar.Item2 - 1;
                maxTaskTime = (int)_holderVar.Item2 + 1;
                locationTime = Random.Range(minTaskTime, maxTaskTime);
                taskTimer = 0;
                isInTask = true;
            }
            else
            {
                taskTimer += Time.deltaTime;
                if (taskTimer >= locationTime)
                {
                    isInTask = false;
                }
            }
        }
        else
        {
            if (taskTimer >= runawayTime)
            {
                isReacting = false;
                taskTimer = 0;
                StartCoroutine(placeTraps(trapsToPlace));
                trapPos = secondaryTrapPos;
                StartCoroutine(placeTraps(trapsToPlace));
            }
            else
            {
                Runaway();
                taskTimer += Time.deltaTime;
            }
        }
    }
    
    private void Reaction()
    {
        trapPos = new Vector3 (player.transform.position.x, 0.05f, player.transform.position.z);
        isInTask = false;
        taskTimer = 0;
        cat.Reaction();
        cat.Chase();
    }

    IEnumerator timeReaction()
    {
        reactionCanvas.SetActive(true);
        yield return new WaitForSeconds(3);
        reactionCanvas.SetActive(false);
    }

    private void Runaway()
    {
        Vector3 playerDirection = player.position - transform.position;
        Vector3 oppositeDirection = transform.position - playerDirection;

        agent.SetDestination(oppositeDirection);
    }

    IEnumerator placeTraps(int numToPlace){
        placingTraps = true;
        isInTask = true;
        agent.SetDestination(trapPos);
        yield return new WaitUntil(ReachedDestination);
        for(int i = 0; i < (numToPlace * aiLevel); i++){
            Vector3 pos = new Vector3(trapPos.x + Random.Range(-1.5f, 1.5f), 0.05f, trapPos.z + Random.Range(-1.5f, 1.5f));
            GameObject trapPlaced = Instantiate(ratTrap, pos, Quaternion.Euler(-90, 0, 0));
        }
        isInTask = false;
        placingTraps = false;
    }

    IEnumerator checkMoving()
    {
        Vector3 currentPos = player.position;
        yield return new WaitForSeconds(0.1f);
        if(player.position != currentPos)
        {
            playerMoving = true;
        }
        playerMoving = false;
    }

    IEnumerator SecondaryTrapCheck(){
        yield return new WaitUntil(() => !(base.isPlayerSighted(player, eyes.transform)));
        secondaryTrapPos = new Vector3 (player.transform.position.x, 0.05f, player.transform.position.z);
    }

    /*IEnumerator EatInfected()
    {
        GameObject infectedItem = GameObject.FindWithTag("Kill Food");
        Vector3 infectedPosition = infectedItem.transform.position;
        agent.SetDestination(infectedPosition);
        isReacting = true;

        yield return new WaitUntil(ReachedDestination);

        Destroy(infectedItem);
        humanDeath.Invoke();
    }*/

    private bool ReachedDestination()
    {
        if (!agent.pathPending)
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                if(!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    protected override void taskEndAction()
    {
        base.taskEndAction();
        Debug.Log("taking end action");
        Vector3 _temp = trapPos;
        //trap pos becomes the current position
        trapPos = transform.position + Vector3.up;
        //just place 1 trap
        StartCoroutine(placeTraps(1));

        //change it back so that it stays clean (not affected by this)
        trapPos = _temp;
    }
}
