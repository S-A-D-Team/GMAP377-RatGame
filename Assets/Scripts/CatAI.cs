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

    private bool playerFound;
    private bool isInTask;
    private bool isReacting;

    private float taskTimer;
    private float locationTime;

    private Vector3 prevTask;

    [SerializeField] private Animator anim;

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
        playerFound = base.isPlayerSighted(player) && base.isSightClear(player);
        if (!isReacting)
        {
            if (playerFound)
            {
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
                Chase(player.position);
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
        agent.SetDestination(playerPosition);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("player"))
        {
            Destroy(collision.gameObject);
            GameManager.Instance.onPlayerTrapped();
            StartCoroutine(GameObject.Find("RELOADQUIT").GetComponent<UIManagerTWOOOOO>().startReload(3f));
            print("Cat Kill");
        }
    }
}
