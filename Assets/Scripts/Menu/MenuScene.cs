using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuScene : MonoBehaviour
{
    public List<GameObject> rats = new List<GameObject>();
    public GameObject cat;
    public GameObject groundPlane;
    public float ratSpeed = 1f;
    public float catSpeed = 0.5f;
    public float turnSpeed = 2f;

    public float avoidanceRadius = 0.5f;
    public float avoidanceStrength = 2f;

    private Vector3 planeMin;
    private Vector3 planeMax;

    private List<Vector3> ratTargetDirections = new List<Vector3>();
    private List<float> ratDirectionChangeTimers = new List<float>();
    private float ratDirectionChangeInterval = 2f;

    public float edgeBuffer = 0.5f; // How far from the edge the rat starts to turn back

    void Start()
    {
        if (groundPlane != null)
        {
            Renderer rend = groundPlane.GetComponent<Renderer>();
            if (rend != null)
            {
                Vector3 size = rend.bounds.size;
                Vector3 center = rend.bounds.center;
                planeMin = center - size / 2f;
                planeMax = center + size / 2f;
            }
        }

        // Initialize direction and timer for each rat
        ratTargetDirections.Clear();
        ratDirectionChangeTimers.Clear();
        foreach (var rat in rats)
        {
            ratTargetDirections.Add(RandomDirection());
            ratDirectionChangeTimers.Add(0f);
        }
    }

    void Update()
    {
        if (rats.Count > 0 && cat != null && groundPlane != null)
        {
            Vector3 catTarget = Vector3.zero;
            float closestDist = float.MaxValue;

            // Move each rat
            for (int i = 0; i < rats.Count; i++)
            {
                var rat = rats[i];
                if (rat == null) continue;

                Vector3 ratPos = rat.transform.position;

                // Check if rat is near the edge
                bool nearEdge =
                    ratPos.x < planeMin.x + edgeBuffer || ratPos.x > planeMax.x - edgeBuffer ||
                    ratPos.z < planeMin.z + edgeBuffer || ratPos.z > planeMax.z - edgeBuffer;

                // RAT: Pick a new random direction at intervals or if near edge
                ratDirectionChangeTimers[i] += Time.deltaTime;
                if (ratDirectionChangeTimers[i] >= ratDirectionChangeInterval || nearEdge)
                {
                    if (nearEdge)
                    {
                        // Turn toward center if near edge
                        Vector3 toCenter = (planeMin + planeMax) * 0.5f - ratPos;
                        toCenter.y = 0;
                        ratTargetDirections[i] = toCenter.normalized;
                    }
                    else
                    {
                        ratTargetDirections[i] = RandomDirection();
                    }
                    ratDirectionChangeTimers[i] = 0f;
                }

                // RAT: Turn smoothly toward the target direction
                Quaternion ratTargetRotation = Quaternion.LookRotation(ratTargetDirections[i]);
                rat.transform.rotation = Quaternion.Slerp(
                    rat.transform.rotation,
                    ratTargetRotation,
                    turnSpeed * Time.deltaTime
                );
                rat.transform.position += rat.transform.forward * ratSpeed * Time.deltaTime;

                Vector3 clampedPos = ClampToPlane(rat.transform.position);
                clampedPos.y = -2.0685f;
                rat.transform.position = clampedPos;

                Vector3 euler = rat.transform.eulerAngles;
                rat.transform.rotation = Quaternion.Euler(0, euler.y, -90);

                // Find the closest rat for the cat to chase
                float dist = Vector3.Distance(cat.transform.position, rat.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    catTarget = rat.transform.position;
                }

                //repulse rat when very close
                Vector3 avoidance = Vector3.zero;
                int neighbors = 0;
                for (int j = 0; j < rats.Count; j++)
                {
                    if (i == j || rats[j] == null) continue;
                    Vector3 toOther = ratPos - rats[j].transform.position;
                    float otherDist = toOther.magnitude;
                    if (otherDist < avoidanceRadius && otherDist > 0.01f)
                    {
                        float strength = Mathf.Lerp(avoidanceStrength, avoidanceStrength * 4f, 1f - (otherDist / avoidanceRadius));
                        avoidance += toOther.normalized * strength / (otherDist + 0.01f);
                        neighbors++;
                    }
                }
                if (neighbors > 0)
                {
                    avoidance /= neighbors;
                    // Blend avoidance with the current direction
                    ratTargetDirections[i] = Vector3.Slerp(ratTargetDirections[i], (ratTargetDirections[i] + avoidance).normalized, 0.7f).normalized;
                }
            }

            // CAT: Chase the closest rat
            Vector3 direction = (catTarget - cat.transform.position);
            direction.y = 0;
            if (direction.magnitude > 0.1f)
            {
                Quaternion lookRot = Quaternion.LookRotation(direction);
                cat.transform.rotation = Quaternion.Slerp(cat.transform.rotation, lookRot, turnSpeed * Time.deltaTime);
                cat.transform.position += cat.transform.forward * catSpeed * Time.deltaTime;
            }

            cat.transform.position = ClampToPlane(cat.transform.position);
        }
    }

    Vector3 ClampToPlane(Vector3 pos)
    {
        return new Vector3(
            Mathf.Clamp(pos.x, planeMin.x, planeMax.x),
            planeMin.y,
            Mathf.Clamp(pos.z, planeMin.z, planeMax.z)
        );
    }

    Vector3 RandomDirection()
    {
        // Random direction on the XZ plane
        float angle = Random.Range(0f, 360f);
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad)).normalized;
    }
}
