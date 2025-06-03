using UnityEngine;

public class Hole_Task : MonoBehaviour
{
    public WallsHole attachedWall;

    public Transform taskLocation;

    public GameObject trap;

    void Start()
    {
        TaskInfo newTask = new TaskInfo(taskLocation, 10f, 2f, true);
        GameObject.FindGameObjectWithTag("Human").GetComponent<HumanAI>().HumanTasks.Add(newTask);
        GameObject.FindGameObjectWithTag("Cat").GetComponent<CatAI>().CatTasks.Add(newTask);
    }

    public void changeHoleToTrap(bool _destroy = false)
    {
        //spawn a lil bit up to let the gravity put it to ground
        GameObject _trap = Instantiate(trap, taskLocation.position + (Vector3.up * 0.5f), Quaternion.identity);
        if(_destroy) DestroyImmediate(gameObject);
    }
}
