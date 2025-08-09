
// Controls the menu scene, including rat and cat movement and avoidance logic
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class MenuScene : MonoBehaviour
{
    public List<GameObject> rats = new List<GameObject>();
    public GameObject cat;
    public GameObject groundPlane;
    public float ratSpeed = 2f;
    public float catSpeed = 2f;
    public float turnSpeed = 1f;

    // How close rats must be to start avoiding each other
    public float avoidanceRadius = 1f;
    // How strongly rats steer away from each other
    public float avoidanceStrength = 2f;

    // Corners of the ground plane (used to keep rats/cat inside bounds)
    private Vector3 planeMin;
    private Vector3 planeMax;

    // Each rat's current target direction
    private List<Vector3> ratTargetDirections = new List<Vector3>();
    // Timers for when each rat should pick a new direction
    private List<float> ratDirectionChangeTimers = new List<float>();
    // How often rats pick a new random direction
    private float ratDirectionChangeInterval = 2f;

    // How close to the edge before rats turn back
    public float edgeBuffer = 0.5f;

    // Initialize plane bounds and rat directions
    void Start()
    {
        if (groundPlane != null)
        {
            Renderer rend = groundPlane.GetComponent<Renderer>();
            if (rend != null)
            {
                // Calculate min/max corners of the ground plane
                Vector3 size = rend.bounds.size;
                Vector3 center = rend.bounds.center;
                planeMin = center - size / 2f;
                planeMax = center + size / 2f;
            }
        }

        // Initialize each rat's direction and timer
        ratTargetDirections.Clear();
        ratDirectionChangeTimers.Clear();
        foreach (var rat in rats)
        {
            ratTargetDirections.Add(RandomDirection()); // Start with a random direction
            ratDirectionChangeTimers.Add(0f);
        }
    }

    // Main update loop: handles rat and cat movement and avoidance
    void Update()
    {
        // Only run if all required objects are present
        if (rats.Count > 0 && cat != null && groundPlane != null)
        {
            Vector3 catTarget = Vector3.zero; // Closest rat position for the cat to chase
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

                // Pick a new random direction at intervals or if near edge
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

                // Turn smoothly toward the target direction
                Quaternion ratTargetRotation = Quaternion.LookRotation(ratTargetDirections[i]);
                rat.transform.rotation = Quaternion.Slerp(
                    rat.transform.rotation,
                    ratTargetRotation,
                    turnSpeed * Time.deltaTime
                );
                rat.transform.position += rat.transform.forward * ratSpeed * Time.deltaTime;

                // Clamp rat to the ground plane and set y to fixed value
                Vector3 clampedPos = ClampToPlane(rat.transform.position);
                clampedPos.y = -2.0685f;
                rat.transform.position = clampedPos;

                // Fix rat's rotation so it stays upright
                Vector3 euler = rat.transform.eulerAngles;
                rat.transform.rotation = Quaternion.Euler(0, euler.y, -90);

                // Find the closest rat for the cat to chase
                float dist = Vector3.Distance(cat.transform.position, rat.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    catTarget = rat.transform.position;
                }

                // Smoother avoidance: steer away from nearby rats with stronger influence
                Vector3 avoidance = Vector3.zero;
                int neighbors = 0;
                float smoothAvoidanceRadius = avoidanceRadius * 1.5f; // look further ahead
                float smoothAvoidanceStrength = avoidanceStrength * 1.5f;
                for (int j = 0; j < rats.Count; j++)
                {
                    if (i == j || rats[j] == null) continue;
                    Vector3 toOther = ratPos - rats[j].transform.position;
                    float otherDist = toOther.magnitude;
                    if (otherDist < smoothAvoidanceRadius && otherDist > 0.01f)
                    {
                        // Stronger repulsion the closer the rats are
                        float strength = Mathf.Lerp(smoothAvoidanceStrength, smoothAvoidanceStrength * 2.5f, 1f - (otherDist / smoothAvoidanceRadius));
                        avoidance += toOther.normalized * strength / (otherDist + 0.01f);
                        neighbors++;
                    }
                }
                if (neighbors > 0)
                {
                    avoidance /= neighbors;
                    // Blend avoidance more strongly for smoother steering
                    ratTargetDirections[i] = Vector3.Slerp(ratTargetDirections[i], (ratTargetDirections[i] + avoidance).normalized, 0.85f).normalized;
                }
            }

            // (No post-move overlap push; avoidance is handled in steering above)

            // CAT: Chase the closest rat
            Vector3 direction = (catTarget - cat.transform.position);
            direction.y = 0;
            if (direction.magnitude > 0.1f)
            {
                // Turn smoothly toward the closest rat
                Quaternion lookRot = Quaternion.LookRotation(direction);
                cat.transform.rotation = Quaternion.Slerp(cat.transform.rotation, lookRot, turnSpeed * Time.deltaTime);
                cat.transform.position += cat.transform.forward * catSpeed * Time.deltaTime;
            }

            // Clamp cat to the ground plane
            cat.transform.position = ClampToPlane(cat.transform.position);
        }
    }


    // Clamp a position to the ground plane bounds
    Vector3 ClampToPlane(Vector3 pos)
    {
        return new Vector3(
            Mathf.Clamp(pos.x, planeMin.x, planeMax.x),
            planeMin.y,
            Mathf.Clamp(pos.z, planeMin.z, planeMax.z)
        );
    }

    // Returns a random direction on the XZ plane
    Vector3 RandomDirection()
    {
        float angle = Random.Range(0f, 360f);
        return new Vector3(Mathf.Sin(angle * Mathf.Deg2Rad), 0, Mathf.Cos(angle * Mathf.Deg2Rad)).normalized;
    }
}
