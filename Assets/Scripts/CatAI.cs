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
    private float lastJumpTime = -10f;
    private bool isJumping = false;
    private Vector3? lastLinkEndPos = null;

    private float playerLostTimer = 0f;
    private float playerLoseDelay = 1.0f;

    private RatStats ratStats;

    private bool audioPlayed = false; // Added: ensures audio plays only once per cat

    void Start()
    {
        prevTask = transform.position;
        baseSpeed = agent.speed;
        eyeLocation = eyes.transform;
        ratStats = player.GetComponent<RatStats>();
        StartCoroutine(HandleOffMeshLink());
    }

    void Update()
    {
        switch (currentState)
        {
            case CatState.Patrolling:
                HandlePatrolling();
                break;

            case CatState.Reacting:
                Reaction();
                break;

            case CatState.Chasing:
                if (isPlayerSighted(eyeLocation))
                {
                    playerLostTimer = 0f;
                    Chase();
                }
                else
                {
                    playerLostTimer += Time.deltaTime;
                    if (playerLostTimer >= playerLoseDelay)
                    {
                        StartCoroutine(StopChase());
                    }
                }
                break;
        }
    }

    private void HandlePatrolling()
    {
        if (isPlayerSighted(eyeLocation))
        {
            currentState = CatState.Reacting;
        }
        else
        {
            if (ReachedDestination())
            {
                (Vector3, float) task = changeLocation(CatTasks, prevTask);
                prevTask = task.Item1;
                locationTime = Random.Range(task.Item2 - 1, task.Item2 + 1);
                agent.SetDestination(prevTask);
                taskTimer = 0;
            }

            taskTimer += Time.deltaTime;
        }
    }

    private IEnumerator HandleOffMeshLink()
    {
        while (true)
        {
            if (agent.isOnOffMeshLink && !isJumping)
            {
                OffMeshLinkData linkData = agent.currentOffMeshLinkData;

                if ((lastLinkEndPos.HasValue && Vector3.Distance(linkData.endPos, lastLinkEndPos.Value) < 0.1f) || !IsLinkInPath(linkData))
                {
                    yield return null;
                    continue;
                }

                CatState savedState = currentState;
                isJumping = true;

                Vector3 startPos = transform.position;
                Vector3 endPos = linkData.endPos;

                Vector3 savedDestination = agent.hasPath ? agent.destination : prevTask;

                agent.enabled = false;
                if (anim != null) anim.SetBool("isJumping", true);

                yield return new WaitForSeconds(0.5f);

                float verticalDiff = endPos.y - startPos.y;
                float peakHeight = verticalDiff < 0
                    ? Mathf.Clamp(-verticalDiff * 0.2f, minDropArcHeight, maxDropArcHeight)
                    : Mathf.Clamp(Mathf.Abs(verticalDiff) * jumpHeight, 0.05f, 0.3f);

                float arcLength = EstimateArcLength(startPos, endPos, peakHeight);
                float jumpDuration = Mathf.Clamp(arcLength / jumpSpeed, 0.08f, 1.8f);

                yield return StartCoroutine(ParabolicJump(startPos, endPos, peakHeight, jumpDuration));

                lastJumpTime = Time.time;

                if (anim != null) anim.SetBool("isJumping", false);

                agent.enabled = true;
                agent.Warp(transform.position);

                currentState = savedState;

                if (currentState == CatState.Chasing && player != null)
                {
                    Debug.Log("Chasing Player");
                    agent.stoppingDistance = 0f;
                    agent.SetDestination(player.position);
                    anim.SetFloat("WalkSpeed", 3f);
                }
                else
                {
                    if (agent.isOnNavMesh)
                    {
                        agent.stoppingDistance = 2f;
                        agent.SetDestination(savedDestination);
                        anim.SetFloat("WalkSpeed", 1f);
                    }
                }

                if (!agent.isOnNavMesh)
                {
                    Debug.LogWarning("Agent not on NavMesh after landing!");
                }

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
        float elapsed = 0f;
        float verticalDifference = end.y - start.y;
        bool jumpingDown = verticalDifference < -0.1f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            Vector3 pos = Vector3.Lerp(start, end, t);

            if (jumpingDown)
            {
                float fallCurve = Mathf.Sin(t * Mathf.PI * 0.5f);
                pos.y = Mathf.Lerp(start.y, end.y, fallCurve);
            }
            else
            {
                pos.y += 4 * peakHeight * Mathf.Sin(Mathf.PI * t);
            }

            transform.position = pos + Vector3.up * agent.baseOffset;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = end + Vector3.up * agent.baseOffset;
    }

    private float EstimateArcLength(Vector3 start, Vector3 end, float peakHeight, int resolution = 10)
    {
        float totalLength = 0f;
        Vector3 previous = start;

        for (int i = 1; i <= resolution; i++)
        {
            float t = i / (float)resolution;
            Vector3 current = Vector3.Lerp(start, end, t);
            current.y += peakHeight * t * (1 - t);

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

        // Play audio only once per cat
        if (!audioPlayed && audioData != null)
        {
            audioData.Play();
            audioPlayed = true;
        }

        if (!playerSpottedFirstTime)
        {
            playerSpottedFirstTime = true;

            if (PopupManager.Instance != null)
            {
                PopupManager.Instance.ShowCatPopup();
            }
        }
    }


    public void Chase()
    {
        if (player == null) return;
        agent.stoppingDistance = 0f;
        agent.SetDestination(player.position);
        anim.SetFloat("WalkSpeed", 3f);
    }

    private IEnumerator StopChase()
    {
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
            if(ratStats.hasStinky){
                Debug.Log("Stinky Activated");
                StartCoroutine(Stun());
                return;
            }
            Destroy(other.gameObject);
            GameManager.Instance.onPlayerDead();
            StartCoroutine(GameObject.Find("RELOADQUIT").GetComponent<UIManagerTWOOOOO>().startReload(3f));
            UIManager.Instance.cueDeathUI(1);
        }
        else if (other.gameObject.layer == LayerMask.NameToLayer("whatIsGround"))
        {
            Crouch();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        StandUp();
    }

    private void Crouch()
    {
        eyeLocation = crouchedEyes.transform;
        anim.SetBool("isCrouching", true);
    }

    private void StandUp()
    {
        eyeLocation = eyes.transform;
        anim.SetBool("isCrouching", false);
    }

    private IEnumerator Stun(){
        agent.enabled = false;
        yield return new WaitForSeconds(3f);
        ratStats.hasStinky = false;
        agent.enabled = true;
        currentState = CatState.Patrolling;
    }
}
