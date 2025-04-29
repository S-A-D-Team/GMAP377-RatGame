using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public enum Axes { x, y, z }

public class WallsHole : MonoBehaviour
{
    [SerializeField]
    private Bounds initialBounds;
    [Tooltip("The global axis of the wall after its orientation. Will use this to align the colliders after holes")]
    [SerializeField]
    private Axes AxisToCheck;
    public List<Bounds> Holes = new List<Bounds>();
    public List<GameObject> HolesObject = new List<GameObject>();
    private void Start()
    {
        initialBounds = GetComponent<BoxCollider>().bounds;
    }
    /// <summary>
    /// Adds a new hole and recreates the collider layout.
    /// Gets box collider bounds from the Hole
    /// </summary>
    public void AddHole(GameObject hole)
    {
        Holes.Add(hole.GetComponent<BoxCollider>().bounds);
        HolesObject.Add(hole);
        CreateHoleInCollider();
    }

    /// <summary>
    /// Rebuilds the colliders based on the current list of Holes.
    /// </summary>
    public void CreateHoleInCollider()
    {
        //remove all BoxColliders colliders
        //remove all Walls if any
        foreach (Transform c in GetComponentsInChildren<Transform>().Where(child => child != transform && child.CompareTag("SpawnedWall")))
        {
            Destroy(c.gameObject);
        }
        foreach (BoxCollider _col in GetComponents<BoxCollider>())
        {
            if(!_col.isTrigger) Destroy(_col);
        }


        // Sort holes based on their axis to sort 
        switch (AxisToCheck)
        {
            case Axes.x:
                Holes.Sort((a, b) => a.min.x.CompareTo(b.min.x));
                break;
            case Axes.y:
                Debug.LogWarning("Wall Axis should be either X or Z");
                break;
            case Axes.z:
                Holes.Sort((a, b) => a.min.z.CompareTo(b.min.z));
                break;

        }

        //'cursor' to left  
        //this is accurate
        Vector3 _currentTopLeftBack = new Vector3(initialBounds.min.x, initialBounds.max.y, initialBounds.min.z);
        Vector3 _currentBottomLeftFront = AxisToCheck == Axes.x ?
                                    new Vector3(initialBounds.min.x, initialBounds.min.y, initialBounds.max.z) :
                                    new Vector3(initialBounds.max.x, initialBounds.min.y, initialBounds.min.z);

        //viewSphere(_currentTopLeftBack);
        //viewSphere(_currentBottomLeftFront);

        Vector3 _temp1, _temp2;
        //loop through each hole and..
        foreach (Bounds _hole in Holes)
        {
            //first, lets define the points of the hole
            //this is accurate
            Vector3 _topLeftBack = new Vector3(_hole.min.x, _hole.max.y, _hole.min.z);
            Vector3 _bottomRightFront = new Vector3(_hole.max.x, _hole.min.y, _hole.max.z);


            //LEFT
            //this is accurate
            //top left of that box
            _temp1 = _currentTopLeftBack;
            //bottom right of that box
            _temp2 = AxisToCheck == Axes.x ?
                                    new Vector3(_topLeftBack.x, _currentBottomLeftFront.y, _currentBottomLeftFront.z) :
                                    new Vector3(_currentBottomLeftFront.x, _currentBottomLeftFront.y, _topLeftBack.z);
            AddBoxCollider(_temp1, _temp2);

            //TOP
            //this is accurate
            //top left of that box
            _temp1 = new Vector3(_topLeftBack.x, _currentTopLeftBack.y, _topLeftBack.z);
            //bottom right of that box
            _temp2 = new Vector3(_bottomRightFront.x, _topLeftBack.y, _bottomRightFront.z);
            AddBoxCollider(_temp1, _temp2);

            //DOWN
            //this is accurate
            //top left of that box
            _temp1 = new Vector3(_topLeftBack.x, _bottomRightFront.y, _topLeftBack.z);
            //bottom right of that box
            _temp2 = new Vector3(_bottomRightFront.x, _currentBottomLeftFront.y, _bottomRightFront.z);
            AddBoxCollider(_temp1, _temp2);

            //move the cursor
            //this is accurate
            _currentTopLeftBack = AxisToCheck == Axes.x ?
                                    new Vector3(_bottomRightFront.x, initialBounds.max.y, initialBounds.min.z) :
                                    new Vector3(initialBounds.min.x, initialBounds.max.y, _bottomRightFront.z);
            _currentBottomLeftFront = new Vector3(_bottomRightFront.x, initialBounds.min.y, _bottomRightFront.z);

        }

        //finally, the last collider
        //this is accurate
        //top left of that box
        _temp1 = _currentTopLeftBack;
        //bottom right of that box
        _temp2 = AxisToCheck == Axes.x ?
                                    new Vector3(initialBounds.max.x, _currentBottomLeftFront.y, _currentBottomLeftFront.z) :
                                    new Vector3(_currentBottomLeftFront.x, _currentBottomLeftFront.y, initialBounds.max.z);
        AddBoxCollider(_temp1, _temp2);

    }

    //Thanks ChatG
    private void AddBoxCollider(Vector3 pointA, Vector3 pointB)
    {
        
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);

        // calculate center
        cube.transform.position = (pointA + pointB) / 2f;

        //calculate size
        cube.transform.localScale = new Vector3(
            Mathf.Abs(pointB.x - pointA.x),
            Mathf.Abs(pointB.y - pointA.y),
            Mathf.Abs(pointB.z - pointA.z)
        );

        cube.transform.SetParent(gameObject.transform);
        // Disable the Renderer so it's invisible
        cube.GetComponent<Renderer>().enabled = false;
        cube.tag = "SpawnedWall";
        cube.layer = LayerMask.NameToLayer("Walls");

    }
    //Thanks ChatG
    private Bounds TransformBoundsToLocal(Bounds worldBounds)
    {
        Vector3 center = transform.InverseTransformPoint(worldBounds.center);
        Vector3 extents = worldBounds.extents;
        Vector3 size = transform.InverseTransformVector(extents);
        return new Bounds(center, size);
    }

    //DEBUG
    void viewSphere(Vector3 _pos)
    {
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.position = _pos;
        sphere.transform.localScale = Vector3.one * 0.1f;
        sphere.GetComponent<Collider>().enabled = false;
    }

    public void ResetWall()
    {
        foreach (GameObject hole in HolesObject)
        {
            Destroy(hole);
        }
        Holes.Clear();

        //remove all BoxColliders colliders
        //remove all Walls if any
        foreach (Transform c in GetComponentsInChildren<Transform>().Where(child => child != transform && child.CompareTag("SpawnedWall")))
        {
            Destroy(c.gameObject);
        }
        foreach (BoxCollider _col in GetComponents<BoxCollider>())
        {
            if (!_col.isTrigger) Destroy(_col);
        }

        // Add a new BoxCollider
        BoxCollider collider = gameObject.AddComponent<BoxCollider>();

        // Convert world-space bounds to local-space center and size
        //collider.center = transform.InverseTransformPoint(initialBounds.center);
        //collider.size = transform.InverseTransformVector(initialBounds.size);

    }
}
