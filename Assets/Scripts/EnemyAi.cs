using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[System.Serializable]
public struct TaskInfo
{
    public Transform location;
    public float time;
    public float weight;
    public bool endTask;

    public TaskInfo(Transform _location, float _time, float _weight, bool _endTask = false)
    {
        this.location = _location;
        this.time = _time;
        this.weight = _weight;
        this.endTask = _endTask;
    }

}

public class EnemyAi : MonoBehaviour
{
    public static EnemyAi Instance { get; private set; }

    protected Transform player;
    protected NavMeshAgent agent;
    protected bool shouldDoTask = false;
    protected bool isDesperate = false;
    protected LayerMask rayCastBlock;
    protected int aiLevel = 1;

    private GameManager gameManager;
    private UnityEvent aiUpdate;

    void Awake(){
        rayCastBlock = LayerMask.GetMask("Default", "whatIsGround", "Walls");
        gameManager = GameObject.Find("Managers").GetComponent<GameManager>();
        aiUpdate = gameManager.aiUpdate;
        aiUpdate.AddListener(updateAi);
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindWithTag("player").transform;
    }

    protected virtual void updateAi(){
        isDesperate = true;
        aiLevel++;
        agent.speed += (0.5f * (aiLevel - 1));
    }

    protected bool isPlayerSighted(Transform enemyEyes, float fieldOfView = 120f, float viewDistance = 20f)
    {
        Vector3 playerDirection = player.position - enemyEyes.position;
        float playerAngle = Vector3.Angle(enemyEyes.forward, playerDirection);

        if(playerAngle < fieldOfView / 2f)
        {
            if(Physics.Raycast(enemyEyes.position, playerDirection, out RaycastHit hit, viewDistance, rayCastBlock)){
                return hit.transform.CompareTag("player");
            }
        }
        return false;
    }

    protected (Vector3, float) changeLocation(List<TaskInfo> tasks, Vector3 prevTask)
    {
        Vector3 task = prevTask;
        int index = 0;

        if(shouldDoTask){
            shouldDoTask = false;
            taskEndAction();
        }

        while(task == prevTask && tasks.Count > 1){
            float totalWeight = 0f;
            foreach (var t in tasks)
                if (t.location.position != prevTask)
                    totalWeight += t.weight;
            
            float rand = Random.Range(0, totalWeight);
            float current = 0f;

            for(int i = 0; i < tasks.Count; i++){
                if(tasks[i].location.position == prevTask) continue;
                current += tasks[i].weight;

                if(rand <= current){
                    index = i;
                    task = tasks[i].location.position;
                    break;
                }
            }
        }

        shouldDoTask = tasks[index].endTask;
        agent.SetDestination(task);
        return (task, tasks[index].time);
    }

    protected bool ReachedDestination(){
        return !agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && (!agent.hasPath || agent.velocity.sqrMagnitude < 0.01f);
    }

    protected bool TryGetValidNavMeshPosition(Vector3 position){
        return NavMesh.SamplePosition(position, out _, 0.1f, NavMesh.AllAreas);
    }

    //will get overridden by child classes
    protected virtual void taskEndAction(){}
}
