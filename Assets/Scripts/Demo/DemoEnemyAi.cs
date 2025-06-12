using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[System.Serializable]
public struct DemoTaskInfo
{
    public Transform location;
    public float time;
    public float weight;
    public bool endTask;

    public DemoTaskInfo(Transform _location, float _time, float _weight, bool _endTask = false)
    {
        this.location = _location;
        this.time = _time;
        this.weight = _weight;
        this.endTask = _endTask;
    }

}

public class DemoEnemyAi : MonoBehaviour
{
    public static EnemyAi Instance { get; private set; }
    private bool shouldDoTask = false;
    private LayerMask rayCastBlock;
    protected int aiLevel;
    private GameManager gameManager;
    private UnityEvent aiUpdate;
    private int taskChoice = -1;

    void Awake(){
        rayCastBlock = LayerMask.GetMask("Default", "whatIsGround", "Walls");
        aiLevel = 1;

        gameManager = GameObject.Find("Managers").GetComponent<GameManager>();
        aiUpdate = gameManager.aiUpdate;
        aiUpdate.AddListener(updateAi);
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

    protected (Vector3, float) changeLocation(List<TaskInfo> tasks, NavMeshAgent agent, Vector3 prevTask)
    {
        int _index = 0;
        Vector3 task = prevTask;

        //at the end of previous task, do the task action if we need
        if(shouldDoTask)
        {
            shouldDoTask = false;
            taskEndAction();
        }

        taskChoice++;
        task = tasks[taskChoice].location.position;
        shouldDoTask = tasks[taskChoice].endTask;
        agent.SetDestination(task);
        return (task, tasks[taskChoice].time );
    }

    public void updateAi(){
        aiLevel++;
    }

    //will get overridden by child classes
    protected virtual void taskEndAction(){}
}
