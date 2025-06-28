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

    private Transform player;

    private bool playerFound;
    private bool isInTask;
    private bool isReacting;
    private bool playerMoving;
    private bool placingTraps;

    private float taskTimer;
    private float locationTime;
    private float baseSpeed;
    private float startTurnAngle;
    public float rotationSpeed = 1f;
    public float minAngle = -45f;
    public float maxAngle = 45f;

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
        isInTask = false;
        isReacting = false;
        prevTask = new Vector3(0,0,0);
        reactionCanvas.SetActive(false);
        playerMoving = false;
        humanDeath = new UnityEvent();
        placingTraps = false;
        baseSpeed = agent.speed;
    }

    // Update is called once per frame
    void Update()
    {
        Debug.Log(agent.speed);
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
                    agent.speed /= 2;
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
                (Vector3, float) _holderVar = base.changeLocation(HumanTasks, prevTask);
                prevTask = _holderVar.Item1;
                minTaskTime = (int)_holderVar.Item2 - 1;
                maxTaskTime = (int)_holderVar.Item2 + 1;
                locationTime = Random.Range(minTaskTime, maxTaskTime);
                taskTimer = 0;
                isInTask = true;
                startTurnAngle = transform.eulerAngles.y;
            }
            else
            {
                taskTimer += Time.deltaTime;
                if (taskTimer >= locationTime)
                {
                    isInTask = false;
                }
                if(ReachedDestination() && isDesperate){
                    float pingPongValue = Mathf.PingPong(Time.time * rotationSpeed, 0.5f);
                    float targetYRotation = Mathf.Lerp(startTurnAngle + minAngle, startTurnAngle + maxAngle, pingPongValue);

                    transform.rotation = Quaternion.Euler(0, targetYRotation, 0);
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
                Stalk();
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

    private void Stalk()
    {
        Vector3 playerDirection = player.position - transform.position;
        agent.SetDestination(playerDirection);
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
        agent.speed = (float)(baseSpeed + (0.5 * (aiLevel - 1)));
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
