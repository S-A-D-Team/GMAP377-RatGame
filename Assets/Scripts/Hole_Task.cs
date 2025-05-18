using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using UnityEngine;

public class Hole_Task : MonoBehaviour
{
    public Transform taskLocation;

    void Start()
    {
        TaskInfo newTask = new TaskInfo(taskLocation, 20f, 20f);
        GameObject.Find("Human").GetComponent<HumanAI>().HumanTasks.Add(newTask);
        GameObject.Find("Cat").GetComponent<CatAI>().CatTasks.Add(newTask);
    }
}
