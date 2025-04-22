using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

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
            return true;
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

    protected Vector3 changeLocation(Transform[] tasks, NavMeshAgent agent, Vector3 prevTask)
    {
        Vector3 task = prevTask;
        while (task == prevTask)
        {
            task = tasks[Random.Range(0, tasks.Length)].position;
        }
        agent.SetDestination(task);
        return task;
    }      
}
