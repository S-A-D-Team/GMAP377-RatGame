using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CatAI : EnemyAi
{
    [SerializeField]
    protected Transform[] tasks;

    [SerializeField]
    private int minTaskTime;

    [SerializeField]
    private int maxTaskTime;

    protected NavMeshAgent agent;

    private Transform player;

    [SerializeField] private bool playerFound;
    [SerializeField] private bool isInTask;
    [SerializeField] private bool isReacting;

    private float taskTimer;
    private float locationTime;

    private Vector3 prevTask;

    [SerializeField] private Animator anim;

    //Chase Mechanic
    [SerializeField] private bool playerLockedOn = false;
    private Vector3 lastValidPlayerPosition;
    [SerializeField]  private List<Vector3> offNavMeshTrail = new List<Vector3>();
    private int frameCounter = 0;
    private bool followingTrail = false, playerInZone = true;
    private int trailIndex = 0;


    // Start is called before the first frame update
    void Start()
    {
        player = base.findPlayer();
        agent = GetComponent<NavMeshAgent>();
        isInTask = false;
        isReacting = false;
        prevTask = new Vector3(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        if(playerLockedOn)
        {
            anim.SetFloat("WalkSpeed", 1.0f);
            Vector3 direction = (player.position - transform.position).normalized;
            transform.position = Vector3.MoveTowards(transform.position, player.position, 4f * Time.deltaTime);
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, 5f * Time.deltaTime);
            //Chase(player.position);

            if(player.gameObject.GetComponent<PlayerSafeZone>().isTouchingWallBack)
            {
                playerLockedOn = false;
                playerFound = false;
            }
            return;
        }

        playerFound = base.isPlayerSighted(player) && base.isSightClear(player);
        if (!isReacting)
        {
            if (playerFound)
            {
                Reaction();
                isReacting = true;
                playerLockedOn = true;
            }
            else if (!isInTask)
            {
                if(!playerLockedOn) prevTask = base.changeLocation(tasks, agent, prevTask);
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
            if (!playerFound)
            {
                agent.speed = 1;
                anim.SetFloat("WalkSpeed", 0f);
                isReacting = false;
            }
            else
            {
                agent.speed = 3;
                anim.SetFloat("WalkSpeed", 1.0f);
                Debug.Log("TRUIHIHasfnlausdhfuilahfuklhfuklsghafliuaghef");

                agent.SetDestination(player.position);
                //Chase(player.position);
            }
        }
    }

    private void Reaction()
    {
        isInTask = false;
        taskTimer = 0;
    }

    public void Chase(Vector3 playerPosition)
    {
        Debug.Log("CHASING");
        if (TryGetValidNavMeshPosition(player.position))
        {
            // player is back on NavMesh
            lastValidPlayerPosition = player.position;
            offNavMeshTrail.Clear();
            followingTrail = false;
            trailIndex = 0;
            frameCounter++;
            if (frameCounter >= 15)
            {
                agent.SetDestination(lastValidPlayerPosition);
                frameCounter = 0;
            }
        }
        else
        {
            // Player is off the NavMesh
            frameCounter++;
            if (frameCounter >= 5)
            {
                offNavMeshTrail.Add(player.position);
                frameCounter = 0;
            }

            if (!followingTrail)
            {
                // Head to last known good NavMesh position
                agent.SetDestination(lastValidPlayerPosition);
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    followingTrail = true;
                    trailIndex = 0;
                }
            }
            else
            {
                // Follow breadcrumb trail
                if (trailIndex < offNavMeshTrail.Count)
                {
                    agent.SetDestination(offNavMeshTrail[trailIndex]);
                    if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                    {
                        trailIndex++;
                    }
                }
            }
        }
    }

    //Thanks chatG
    private bool TryGetValidNavMeshPosition(Vector3 targetPosition)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(targetPosition, out hit, 0.1f, NavMesh.AllAreas))
        {
            
            return true;
        }

        return false;
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("player"))
        {
            Destroy(other.gameObject);
            GameManager.Instance.onPlayerTrapped();
            StartCoroutine(GameObject.Find("RELOADQUIT").GetComponent<UIManagerTWOOOOO>().startReload(3f));
            print("Cat Kill");
        }
    }
    /*
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("player"))
        {
            Destroy(collision.gameObject);
            GameManager.Instance.onPlayerTrapped();
            StartCoroutine(GameObject.Find("RELOADQUIT").GetComponent<UIManagerTWOOOOO>().startReload(3f));
            print("Cat Kill");
        }
    }*/
}
