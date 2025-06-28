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
    private bool shouldDoTask = false;
    protected bool isDesperate = false;
    private LayerMask rayCastBlock;
    protected int aiLevel;
    private GameManager gameManager;
    private UnityEvent aiUpdate;
    protected NavMeshAgent agent;

    void Awake(){
        rayCastBlock = LayerMask.GetMask("Default", "whatIsGround", "Walls");
        aiLevel = 1;

        gameManager = GameObject.Find("Managers").GetComponent<GameManager>();
        aiUpdate = gameManager.aiUpdate;
        aiUpdate.AddListener(updateAi);

        agent = GetComponent<NavMeshAgent>();
    }
    
    // Start is called before the first frame update
    protected Transform findPlayer()
    {
        return GameObject.FindWithTag("player").transform;
    }

    protected bool isPlayerSighted(Transform player, Transform enemyHeight)
    {
        Vector3 playerDirection = transform.position - player.position;
        float playerAngle = Vector3.Angle(transform.forward, playerDirection);

        if(Mathf.Abs(playerAngle) > 135 && Mathf.Abs(playerAngle) < 225)
        {
            return isSightClear(player, enemyHeight.position);
        }
        return false;
    }

    protected bool isSightClear(Transform player, Vector3 enemyHeight)
    {
        RaycastHit _hit;
        Vector3 playerDirection = player.position - enemyHeight;
        if(Physics.Raycast(enemyHeight, playerDirection, out _hit, 10f, rayCastBlock))
        {
            if (_hit.transform.CompareTag("player"))
            {
                return true;
            }
        }
        return false;
    }

    protected (Vector3, float) changeLocation(List<TaskInfo> tasks, Vector3 prevTask)
    {
        int _index = 0;
        Vector3 task = prevTask;

        //at the end of previous task, do the task action if we need
        if(shouldDoTask)
        {
            shouldDoTask = false;
            taskEndAction();
        }

        while (task == prevTask && tasks.Count > 1)
        {
            float totalWeight = 0f;

            // Calculate total weight excluding the previous task
            foreach (var t in tasks)
            {
                if (t.location.position != prevTask)
                    totalWeight += t.weight;
            }

            float randomValue = Random.Range(0, totalWeight);
            float currentSum = 0f;

            for (int i = 0; i < tasks.Count; i++)
            {
                if (tasks[i].location.position == prevTask)
                    continue;

                currentSum += tasks[i].weight;
                if (randomValue <= currentSum)
                {
                    _index = i;
                    task = tasks[_index].location.position;
                    break;
                }
            }
        }
        shouldDoTask = tasks[_index].endTask;
        agent.SetDestination(task);
        return (task, tasks[_index].time );
    }

    public void updateAi(){
        isDesperate = true;
        aiLevel++;
        agent.speed += (1f * (aiLevel - 1));
    }

    //will get overridden by child classes
    protected virtual void taskEndAction(){}
}
