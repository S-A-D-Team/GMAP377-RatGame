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
    [SerializeField] private float minDropArcHeight;
    [SerializeField] private float maxDropArcHeight;


    [SerializeField] private bool playerSpottedFirstTime = false;

    private Transform eyeLocation;
    private Vector3 prevTask;
    private float taskTimer;
    private float locationTime;
    private float baseSpeed;
    private float jumpCooldownTime = 10.0f;
    private float lastJumpTime = -10f;
    private bool isJumping = false;
    private bool midJump = false;
    private Vector3? lastLinkEndPos = null;

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

    private IEnumerator HandleOffMeshLink()
    {
        while (true)
        {
            if (agent.isOnOffMeshLink && !isJumping)
            {
                OffMeshLinkData linkData = agent.currentOffMeshLinkData;

                if (lastLinkEndPos.HasValue && Vector3.Distance(linkData.endPos, lastLinkEndPos.Value) < 0.1f)
                {
                    yield return null;
                    continue;
                }

                isJumping = true;

                Vector3 startPos = transform.position;
                Vector3 endPos = linkData.endPos + Vector3.up * agent.baseOffset;

                agent.enabled = false;

                if (anim != null) anim.SetBool("isJumping", true);

                yield return new WaitForSeconds(0.5f);

                float verticalDifference = endPos.y - startPos.y;
                float peakHeight = verticalDifference < 0
                    ? Mathf.Clamp(-verticalDifference * 0.2f, minDropArcHeight, maxDropArcHeight)
                    : Mathf.Clamp(Mathf.Abs(verticalDifference) * jumpHeight, 0.05f, 0.3f);   

                float arcLength = EstimateArcLength(startPos, endPos, peakHeight);
                float jumpDuration = Mathf.Clamp(arcLength / jumpSpeed, 0.08f, 1.8f);

                yield return StartCoroutine(ParabolicJump(startPos, endPos, peakHeight, jumpDuration));

                lastJumpTime = Time.time;

                if (anim != null) anim.SetBool("isJumping", false);

                agent.enabled = true;
                lastLinkEndPos = linkData.startPos; 
                agent.CompleteOffMeshLink();
                isJumping = false;
            }

            yield return null;
        }
    }

    private bool IsLinkInPath(OffMeshLinkData linkData)
    {
        if (!agent.hasPath) return false;

        Vector3 endPosFlat = new Vector3(linkData.endPos.x, 0, linkData.endPos.z);

        foreach (Vector3 corner in agent.path.corners)
        {
            Vector3 cornerFlat = new Vector3(corner.x, 0, corner.z);
            if (Vector3.Distance(cornerFlat, endPosFlat) < 0.5f)
            {
                return true;
            }
        }

        return false;
    }

    private IEnumerator ParabolicJump(Vector3 start, Vector3 end, float peakHeight, float duration)
    {
        midJump = true;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 pos = Vector3.Lerp(start, end, t);
            pos.y +=  4 * peakHeight * Mathf.Sin(Mathf.PI * t);
            transform.position = pos;

            elapsed += Time.deltaTime;
            yield return null;
        }

        /*Vector3 endPos = new Vector3(end.x, end.y - 2, end.z);*/
        transform.position = end;
        midJump = false;
    }

    private float EstimateArcLength(Vector3 start, Vector3 end, float peakHeight, int resolution = 10)
    {
        float totalLength = 0f;
        Vector3 previous = start;

        for (int i = 1; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector3 current = Vector3.Lerp(start, end, t);
            float height = peakHeight * t * (1 - t);
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
