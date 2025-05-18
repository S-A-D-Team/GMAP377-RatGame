using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[System.Serializable]
public struct TaskInfo
{
    public Transform location;
    public float time;
    public float weight;

    public TaskInfo(Transform _location, float _time, float _weight)
    {
        this.location = _location;
        this.time = _time;
        this.weight = _weight;
    }

}

public class EnemyAi : MonoBehaviour
{

    // Start is called before the first frame update
    protected Transform findPlayer()
    {
        return GameObject.FindWithTag("player").transform;
    }

    protected bool isPlayerSighted(Transform player)
    {
        Vector3 playerDirection = transform.position - player.position;
        float playerAngle = Vector3.Angle(transform.forward, playerDirection);

        if(Mathf.Abs(playerAngle) > 90 && Mathf.Abs(playerAngle) < 270)
        {
            return isSightClear(player);
        }
        return false;
    }

    protected bool isSightClear(Transform player)
    {
        RaycastHit _hit;
        Vector3 playerDirection = player.position - transform.position;
        if(Physics.Raycast(transform.position, playerDirection, out _hit, 500000f))
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

        agent.SetDestination(task);
        return (task, tasks[_index].time );
    }  
}
