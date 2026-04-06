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

    // Animation
    [Header("Animation")]
    public Animator animator; // assign in Inspector or leave null to auto-get
    private Vector2 lastInput = Vector2.right;
    private const float arriveThreshold = 0.1f;

    void Start()
    {
        waypoints = new Transform[waypointParent.childCount];
        for (int i = 0; i < waypointParent.childCount; i++)
        {
            waypoints[i] = waypointParent.GetChild(i);
        }

        if (animator == null)
            animator = GetComponent<Animator>();
        // initialize animator last input if present
        if (animator != null)
        {
            animator.SetFloat("lastInputX", lastInput.x);
            animator.SetFloat("lastInputY", lastInput.y);
            animator.SetBool("isMoving", false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0f || isWaiting) return; // Don't move if the game is paused or waiting

        MoveToWaypoint();
    }

    void MoveToWaypoint()
    {
        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector2 currentPos = transform.position;
        Vector2 targetPos = targetWaypoint.position;
        Vector2 toTarget = targetPos - currentPos;
        float dist = toTarget.magnitude;

        if (dist > arriveThreshold)
        {
            Vector2 dir = toTarget.normalized;
            // move
            transform.position = Vector2.MoveTowards(currentPos, targetPos, moveSpeed * Time.deltaTime);

            // update animator with movement direction
            if (animator != null)
            {
                animator.SetBool("isMoving", true);
                animator.SetFloat("lastInputX", dir.x);
                animator.SetFloat("lastInputY", dir.y);
                Debug.Log("is moving");
            }

            lastInput = dir;
        }
        else
        {
            // arrived
            if (animator != null)
                animator.SetBool("isMoving", false);

            StartCoroutine(WaitAtWaypoint());
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(waitTimeAtWaypoint);

        currentWaypointIndex = loop ? (currentWaypointIndex + 1) % waypoints.Length : Mathf.Min(currentWaypointIndex + 1, waypoints.Length - 1);
        isWaiting = false;

        // keep lastInput in animator so idle pose faces last movement direction
        if (animator != null)
        {
            animator.SetFloat("lastInputX", lastInput.x);
            animator.SetFloat("lastInputY", lastInput.y);
        }
    }
}
