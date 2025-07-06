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
    [SerializeField] private Animator anim;
    [SerializeField] private AudioSource audioData;

    [SerializeField] private bool playerSpottedFirstTime = false;

    private Vector3 prevTask;
    private float taskTimer;
    private float locationTime;
    private float baseSpeed;

    void Start()
    {
        prevTask = transform.position;
        baseSpeed = agent.speed;
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
                if(isPlayerSighted(eyes.transform)) Chase();
                else StartCoroutine(StopChase());
                break;
        }
    }

    private void HandlePatrolling(){
        if(isPlayerSighted(eyes.transform)){
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

    public void Reaction()
    {
        currentState = CatState.Chasing;
        anim.SetFloat("WalkSpeed", 1f);
        audioData.Play();

        if(!playerSpottedFirstTime){
            playerSpottedFirstTime = true;
            UIManager.Instance.beginTutorial(6);
        }
    }

    public void Chase(){
        agent.speed *= 3;
        agent.stoppingDistance = 0;
        agent.SetDestination(player.position);
    }

    private IEnumerator StopChase(){
        currentState = CatState.Patrolling;
        yield return new WaitUntil(ReachedDestination);
        anim.SetFloat("WalkSpeed", 0f);
        agent.speed = baseSpeed + (0.5f * (aiLevel - 1));
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
        }
    }
}
