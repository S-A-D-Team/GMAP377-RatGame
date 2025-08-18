using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using TMPro;

public class GrandmaAI : EnemyAi
{
    private enum HumanState { Patrolling, Reacting}
    private HumanState currentState = HumanState.Patrolling;

    [SerializeField] private int runawayTime;
    [SerializeField] private GameObject vacuum;
    [SerializeField] private CatAI cat;
    [SerializeField] private GameObject reactionCanvas;
    [SerializeField] TextMeshProUGUI lineText;
    [SerializeField] private GameObject eyes;
    [SerializeField] private Transform vacuumHolder;

    private float taskTimer;
    private float locationTime;
    private float baseSpeed;
    private Vector3 prevTask;
    private float startTurnAngle;
    private int displaySaying;

    [SerializeField] private float rotationSpeed = 1f;
    [SerializeField] private float minAngle = -45f, maxAngle = 45f;
    [SerializeField] private bool playerSpottedFirstTime = false;

    private bool playerMoving;
    private bool coroutineRunning;
    private bool hasVacuum;

    public UnityEvent humanDeath = new UnityEvent();
    public List<TaskInfo> HumanTasks;

    private string sightReaction = "Ah, a rat!";
    [SerializeField] private List<string> relaxedLines;
    [SerializeField] private List<string> nervousLines;
    [SerializeField] private List<string> concernedLines;
    [SerializeField] private List<string> desperateLines;
    private List<string> currentLines;


    void Start()
    { 
        prevTask = transform.position;
        reactionCanvas.SetActive(false);
        baseSpeed = agent.speed;
        currentLines = relaxedLines;
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

            if(ReachedDestination()){
                if(!coroutineRunning){
                    displaySaying = Random.Range(0, (currentLines.Count * 2));
                    if(displaySaying < currentLines.Count){
                        StartCoroutine(displayLine(currentLines[displaySaying]));
                    }
                }

                if( isDesperate){
                    float pingPong = Mathf.PingPong(Time.time * rotationSpeed, 0.5f);
                    float angle = Mathf.Lerp(startTurnAngle + minAngle, startTurnAngle + maxAngle, pingPong);
                    transform.rotation = Quaternion.Euler(0, angle, 0);
                } else{
                    if (hasVacuum && Random.value < 0.4f)
                    {
                        hasVacuum = false;

                        vacuum.transform.SetParent(null);
                        vacuum.transform.position = prevTask;
                        vacuum.transform.rotation = Quaternion.identity;

                        vacuum.SetActive(true);
                    }
                }
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
        StartCoroutine(displayLine(sightReaction));
        cat.Reaction();
        cat.Chase();

        yield return new WaitForSeconds(3);
    }

    private IEnumerator displayLine(string line){
        coroutineRunning = true;
        lineText.text = line;
        reactionCanvas.SetActive(true);
        yield return new WaitForSeconds(3);
        reactionCanvas.SetActive(false);
        coroutineRunning = false;
    }

    private void HandleReacting(){
        taskTimer += Time.deltaTime;
        if(taskTimer >= runawayTime){
            taskTimer = 0;
            currentState = HumanState.Patrolling;
        } else{
            if(!hasVacuum){
                Vector3 directionToPlayer = transform.position - player.position;
                Vector3 fleeTarget = transform.position + directionToPlayer.normalized * 10f;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(fleeTarget, out hit, 5f, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }
            } else{
                agent.SetDestination(player.position);
            }
        }
    }

    protected override void updateAi(){
        aiLevel++;
        agent.speed += (1f * (aiLevel - 1));
        isDesperate = true;
        switch(aiLevel){
            case 2:
                currentLines = nervousLines;
                break;
            case 3:
                currentLines = nervousLines;
                break;
            default:
                currentLines = desperateLines;
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("vacuum"))
        {
            hasVacuum = true;
            vacuum.transform.SetParent(vacuumHolder);
            vacuum.transform.localPosition = Vector3.zero;
            vacuum.transform.localRotation = Quaternion.identity;
        }
    }
}
