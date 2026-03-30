using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointMover : MonoBehaviour
{
    public Transform waypointParent;
    public float moveSpeed = 2f;
    public float waitTimeAtWaypoint = 1f;
    public bool loop = true;

    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private bool isWaiting;
    void Start()
    {
        waypoints = new Transform[waypointParent.childCount];
        for(int i=0; i< waypointParent.childCount; i++)
        {
            waypoints[i] = waypointParent.GetChild(i);
        };
    }

    // Update is called once per frame
    void Update()
    {
        if(Time.timeScale == 0f || isWaiting)
        {
            return; // Don't move if the game is paused or there are no waypoints
        }
        //Move to pointway
        MoveToWaypoint();
    }
    void MoveToWaypoint()
    {
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        transform.position = Vector2.MoveTowards(transform.position, targetWaypoint.position, moveSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetWaypoint.position) < 0.1f)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }
    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTimeAtWaypoint);
        currentWaypointIndex = loop ? (currentWaypointIndex + 1) % waypoints.Length : Mathf.Min(currentWaypointIndex + 1, waypoints.Length - 1);// Move to the next waypoint, looping back to the start if necessary
        isWaiting = false;
    }
}
