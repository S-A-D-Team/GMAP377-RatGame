using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class HumanAI : EnemyAi
{

    private int minTaskTime = 10;
    private int maxTaskTime = 20;

    [SerializeField]
    private int runawayTime;

    [SerializeField]
    private CatAI cat;

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

    private float taskTimer;
    private float locationTime;

    private Vector3 prevTask;

    private KeyCode killKey = KeyCode.Z;

    public UnityEvent humanDeath;

    [SerializeField] private bool playerSpottedFirstTime = false;

    [Space]
    public List<TaskInfo> HumanTasks;

    AudioSource audioData;

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
        audioData = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(killKey))
        {
            EatInfected();
        }
        if (!isReacting)
        {
            playerFound = base.isPlayerSighted(player);
            if (playerFound)
            {
                audioData.Play(0);
                checkMoving();
                if (!playerMoving)
                {
                    StartCoroutine(timeReaction());
                    Reaction();
                    isReacting = true;

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
                minTaskTime = (int)_holderVar.Item2 + 1;
                minTaskTime = (int)_holderVar.Item2 + 1;
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
        isInTask = false;
        taskTimer = 0;
        //cat.FirstReaction(player.position);
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

    private void EatInfected()
    {
        GameObject infectedItem = GameObject.FindWithTag("Kill Food");
        Vector3 infectedPosition = infectedItem.transform.position;
        agent.SetDestination(infectedPosition);
        isInTask = true;

        while (!ReachedDestination())
        {
            continue;
        }

        Destroy(infectedItem);
        humanDeath.Invoke();
    }

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
}
