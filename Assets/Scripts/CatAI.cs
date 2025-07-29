using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CatAI : EnemyAi
{
    private enum CatState { Patrolling, Reacting, Chasing }
    private CatState currentState = CatState.Patrolling;

    [SerializeField] public List<TaskInfo> CatTasks;
    [SerializeField] private GameObject eyes;
    [SerializeField] private GameObject crouchedEyes;
    [SerializeField] private Animator anim;
    [SerializeField] private AudioSource audioData;
    [SerializeField] private float chaseSpeedMultiplier;
    [SerializeField] private float crouchSpeedMultiplier;
    [SerializeField] private float jumpSpeed;
    [SerializeField] private float jumpHeight;


    [SerializeField] private bool playerSpottedFirstTime = false;

    private Transform eyeLocation;
    private Vector3 prevTask;
    private float taskTimer;
    private float locationTime;
    private float baseSpeed;

    void Start()
    {
        prevTask = transform.position;
        baseSpeed = agent.speed;
        eyeLocation = eyes.transform;
        StartCoroutine(HandleOffMeshLink());
    }

    void Update()
    {
        switch(currentState){
            case CatState.Patrolling:
                HandlePatrolling();
                break;
            case CatState.Reacting:
                Reaction();
                break;
            case CatState.Chasing:
                if(isPlayerSighted(eyeLocation)) Chase();
                else StartCoroutine(StopChase());
                break;
        }
    }

    private void HandlePatrolling(){
        if(isPlayerSighted(eyeLocation)){
            currentState = CatState.Reacting;
        } else{
            if(ReachedDestination()){
                (Vector3, float) task = changeLocation(CatTasks, prevTask);
                prevTask = task.Item1;
                locationTime = Random.Range(task.Item2 - 1, task.Item2 + 1);
                taskTimer = 0;
            }

            taskTimer += Time.deltaTime;
            if(taskTimer >= locationTime){
                taskTimer = 0;
            }
        }
    }

    private IEnumerator HandleOffMeshLink(){
        while(true){
            if(agent.isOnOffMeshLink){
                OffMeshLinkData linkData = agent.currentOffMeshLinkData;
                Vector3 start = agent.transform.position;
                Vector3 end = linkData.endPos + Vector3.up * agent.baseOffset;

                float verticalDifference = end.y - start.y;
                float peakHeight = Mathf.Abs(verticalDifference) * jumpHeight;

                float arcLength = EstimateArcLength(start, end, peakHeight);
                float jumpDuration = arcLength / jumpSpeed;

                if(anim != null){
                    anim.SetBool("isJumping", true);
                }

                yield return new WaitForSeconds(0.2f);

                yield return StartCoroutine(ParabolicJump(start, end, peakHeight, jumpDuration));

                if(anim != null){
                    anim.SetBool("isJumping", false);
                }

                agent.CompleteOffMeshLink();
            }

            yield return null;
        }
    }

    private IEnumerator ParabolicJump(Vector3 start, Vector3 end, float peakHeight, float duration){
        float elapsed = 0f;

        agent.updatePosition = false;
        agent.updateRotation = false;

        while (elapsed < duration){
            float t = elapsed/duration;

            Vector3 horizontal = Vector3.Lerp(start, end, t);

            float heightOffset = peakHeight * t * (1-t);
            horizontal.y += heightOffset;

            agent.transform.position = horizontal;

            elapsed += Time.deltaTime;
            yield return null;
        }

        agent.transform.position = end;

        agent.updatePosition = true;
        agent.updateRotation = true;
    }

    private float EstimateArcLength(Vector3 start, Vector3 end, float peakHeight, int resolution = 10)
    {
        float totalLength = 0f;
        Vector3 previous = start;

        for (int i = 1; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector3 current = Vector3.Lerp(start, end, t);
            float height = 4 * peakHeight * t * (1 - t);
            current.y += height;

            totalLength += Vector3.Distance(previous, current);
            previous = current;
        }

        return totalLength;
    }

    public void Reaction()
    {
        agent.speed *= chaseSpeedMultiplier;
        currentState = CatState.Chasing;
        anim.SetFloat("WalkSpeed", 3f);
        if(audioData != null){
            audioData.Play();
        }

        if(!playerSpottedFirstTime){
            playerSpottedFirstTime = true;
            UIManager.Instance.beginTutorial(6);
        }
    }

    public void Chase(){
        agent.stoppingDistance = 0;
        agent.SetDestination(player.position);
    }

    private IEnumerator StopChase(){
        currentState = CatState.Patrolling;
        agent.speed = baseSpeed + (0.5f * (aiLevel - 1));
        yield return new WaitUntil(ReachedDestination);
        anim.SetFloat("WalkSpeed", 0f);
        agent.stoppingDistance = 2f;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("player"))
        {
            Destroy(other.gameObject);
            GameManager.Instance.onPlayerDead();
            StartCoroutine(GameObject.Find("RELOADQUIT").GetComponent<UIManagerTWOOOOO>().startReload(3f));
            UIManager.Instance.cueDeathUI(1);
        } else if(other.gameObject.layer == LayerMask.NameToLayer("whatIsGround")){
            Crouch();
        }
    }

    private void OnTriggerExit(Collider other){
        StandUp();
    }

    private void Crouch(){
        eyeLocation = crouchedEyes.transform;
        anim.SetBool("isCrouching", true);
    }

    private void StandUp(){
        eyeLocation = eyes.transform;
        anim.SetBool("isCrouching", false);
    }
}
