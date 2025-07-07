using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class HumanAI : EnemyAi
{
    private enum HumanState { Patrolling, Reacting, PlacingTraps}
    private HumanState currentState = HumanState.Patrolling;

    [SerializeField] private int runawayTime;
    [SerializeField] private int trapsToPlace;
    [SerializeField] private CatAI cat;
    [SerializeField] private GameObject ratTrap;
    [SerializeField] private GameObject reactionCanvas;
    [SerializeField] private GameObject eyes;

    private float taskTimer;
    private float locationTime;
    private float baseSpeed;
    private Vector3 prevTask;
    private Vector3 trapPos, secondaryTrapPos;
    private float startTurnAngle;

    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float minAngle = -45f, maxAngle = 45f;
    [SerializeField] private bool playerSpottedFirstTime = false;

    private bool playerMoving;
    private bool coroutineRunning;

   public UnityEvent humanDeath = new UnityEvent();
   public List<TaskInfo> HumanTasks;

    void Start()
    { 
        prevTask = transform.position;
        reactionCanvas.SetActive(false);
        baseSpeed = agent.speed;
    }

    void Update()
    {
        switch (currentState){
            case HumanState.Patrolling:
                HandlePatrolling();
                break;
            case HumanState.Reacting:
                HandleReacting();
                break;
            case HumanState.PlacingTraps:
                break;
        }
    }

    private void HandlePatrolling(){
        bool playerSeen = isPlayerSighted(eyes.transform);

        if(playerSeen && !coroutineRunning){
            StartCoroutine(checkIfPlayerIsMoving());
        }
        else if (!playerSeen && shouldDoTask){
            (Vector3, float) task = changeLocation(HumanTasks, prevTask);
            prevTask = task.Item1;
            locationTime = Random.Range(task.Item2 - 1, task.Item2 + 1);
            taskTimer = 0;
            startTurnAngle = transform.eulerAngles.y;
        } else{
            taskTimer += Time.deltaTime;
            if(taskTimer >= locationTime) shouldDoTask = true;

            if(ReachedDestination() && isDesperate){
                float pingPong = Mathf.PingPong(Time.time * rotationSpeed, 0.5f);
                float angle = Mathf.Lerp(startTurnAngle + minAngle, startTurnAngle + maxAngle, pingPong);
                transform.rotation = Quaternion.Euler(0, angle, 0);
            }
        }
    }

    private IEnumerator checkIfPlayerIsMoving(){
        coroutineRunning = true;
        Vector3 start = player.position;
        yield return new WaitForSeconds(0.1f);
        playerMoving = player.position != start;
        coroutineRunning = false;

        if(playerMoving){
            StartCoroutine(reactToPlayer());
        }
    }

    private IEnumerator reactToPlayer(){
        currentState = HumanState.Reacting;
        trapPos = new Vector3(player.position.x, 0.05f, player.position.z);
        reactionCanvas.SetActive(true);
        cat.Reaction();
        cat.Chase();
        agent.speed /= 2;

        if(!playerSpottedFirstTime){
            playerSpottedFirstTime = true;
            UIManager.Instance.beginTutorial(5);
        }

        yield return new WaitForSeconds(3);
        reactionCanvas.SetActive(false);
    }

    private void HandleReacting(){
        taskTimer += Time.deltaTime;
        if(taskTimer >= runawayTime){
            taskTimer = 0;
            StartCoroutine(SecondaryTrapRoutine());
        } else{
            agent.SetDestination(player.position);
        }
    }

    private IEnumerator SecondaryTrapRoutine(){
        currentState = HumanState.PlacingTraps;

        yield return new WaitUntil(() => !isPlayerSighted(eyes.transform));
        secondaryTrapPos = new Vector3(player.position.x, 0.05f, player.position.z);
        agent.speed = baseSpeed + (0.5f * (aiLevel - 1));

        Vector3[] trapPoints = { trapPos, secondaryTrapPos };
        foreach (var pos in trapPoints){
            yield return StartCoroutine(placeTraps(pos));
        }

        currentState = HumanState.Patrolling;
    }

    private IEnumerator placeTraps(Vector3 pos){
        agent.SetDestination(pos);
        yield return new WaitUntil(ReachedDestination);
        for(int i = 0; i < trapsToPlace * aiLevel; i++){
            Vector3 trapPosition = pos + new Vector3(Random.Range(-1.5f, 1.5f), 0.05f, Random.Range(-1.5f, 1.5f));
            Instantiate(ratTrap, trapPosition, Quaternion.Euler(-90, 0, 0));
        }
    }

    protected override void taskEndAction(){
        StartCoroutine(placeTraps(transform.position + Vector3.up));
    }
}