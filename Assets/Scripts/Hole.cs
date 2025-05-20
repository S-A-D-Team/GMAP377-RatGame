using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hole : MonoBehaviour
{
    public bool isEnabled = false;
    string wallLayerName = "Walls";
    public GameObject holePrefab;

    public List<GameObject> wallsInContact = new List<GameObject>();

    AudioSource audioData;

    // Start is called before the first frame update
    void Start()
    {
        audioData = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!isEnabled) return;

        if (Input.GetKeyDown(KeyCode.E)) // change later
        {
            TryHole();
        }
    }

    private void TryHole()
    {
        if (wallsInContact.Count == 0 || holePrefab == null)
            return;

        Transform wall = wallsInContact[0].transform;

        audioData.Play(0);

        //find the matching orientation reference based on name
        string _refName = wall.name.Replace("Wall", "HoleRef"); // "Wall (1)" becomes "HoleRef (1)"
        GameObject orientationRefObj = GameObject.Find(_refName);
        if (orientationRefObj == null)
        {
            Debug.LogWarning($"No matching reference object found: {_refName}");
            return;
        }

        
        Transform referenceOrientation = orientationRefObj.transform;

        //
        // 1. instantiate at player
        GameObject newObj = Instantiate(holePrefab, transform.position, referenceOrientation.rotation);

        // 2. parent to wall
        newObj.transform.SetParent(wall);

        // 3. set local Y to 0 
        Vector3 localPos = newObj.transform.localPosition;
        localPos.y = 0;
        newObj.transform.localPosition = localPos;

        // 4. hole bounds to wall
        StartCoroutine(DelayRegisterHole(newObj, wall));
    }
    private IEnumerator DelayRegisterHole(GameObject newObj, Transform wall)
    {
        // wait a 2 frames so collider bounds gets updated properly
        yield return null;
        yield return null;

        wall.GetComponent<WallsHole>().AddHole(newObj);
        wallsInContact.Clear();
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsWall(other))
        {
            GameObject _wall = getWall(other);
            if (!wallsInContact.Contains(_wall))
                wallsInContact.Add(_wall);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (IsWall(other))
        {
            GameObject _wall = getWall(other);
            if (wallsInContact.Contains(_wall))
                wallsInContact.Remove(_wall);
        }
    }

    private bool IsWall(Collider col)
    {
        return col.gameObject.layer == LayerMask.NameToLayer(wallLayerName);
    }

    private GameObject getWall(Collider col)
    {
        if(col.tag == "SpawnedWall")
        {
            return col.transform.parent.gameObject;
        }
        return col.gameObject;
    }
    
}
