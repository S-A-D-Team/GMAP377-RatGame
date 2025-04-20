using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class HumanAI : EnemyAi
{
    [SerializeField]
    protected Transform[] tasks;

    [SerializeField]
    private int minTaskTime;

    [SerializeField]
    private int maxTaskTime;

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

    private float taskTimer;
    private float locationTime;

    private Vector3 prevTask;

    // Start is called before the first frame update
    void Start()
    {
        player = base.findPlayer();
        agent = GetComponent<NavMeshAgent>();
        isInTask = false;
        isReacting = false;
        prevTask = new Vector3(0,0,0);
        reactionCanvas.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!isReacting)
        {
            playerFound = base.isPlayerSighted(player) && base.isSightClear(player);
            if (playerFound)
            {
                StartCoroutine(timeReaction());
                Reaction();
                isReacting = true;
            }
            else if (!isInTask)
            {
                prevTask = base.changeLocation(tasks, agent, prevTask);
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
        cat.Chase(player.position);
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
}
