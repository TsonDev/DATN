using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaypointMover : MonoBehaviour
{
    public Transform waypointParent;
    public float moveSpeed = 2f;
    public float waitTimeAtWaypoint = 1f;
    public bool loop = true;

    [Header("Auto Generate (For Spawned Enemies)")]
    public bool autoGenerateWaypoints = false;
    public int generateCount = 3;
    public float generateRadius = 5f;

    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private bool isWaiting;
    private GameObject generatedParent; // Lưu lại để xóa khi enemy chết

    // Animation
    [Header("Animation")]
    public Animator animator; // assign in Inspector or leave null to auto-get
    private Vector2 lastInput = Vector2.right;
    private const float arriveThreshold = 0.1f;

    void Start()
    {
        if (waypointParent != null && waypointParent.childCount > 0)
        {
            waypoints = new Transform[waypointParent.childCount];
            for (int i = 0; i < waypointParent.childCount; i++)
            {
                waypoints[i] = waypointParent.GetChild(i);
            }
        }
        else if (autoGenerateWaypoints)
        {
            // Tự động tạo waypoints ngẫu nhiên xung quanh vị trí spawn
            generatedParent = new GameObject(gameObject.name + "_AutoWaypoints");
            waypoints = new Transform[generateCount];
            
            for (int i = 0; i < generateCount; i++)
            {
                GameObject wp = new GameObject("WP_" + i);
                // Vị trí ngẫu nhiên trong vòng tròn bán kính generateRadius
                Vector2 randomPos = (Vector2)transform.position + Random.insideUnitCircle * generateRadius;
                wp.transform.position = randomPos;
                wp.transform.SetParent(generatedParent.transform);
                waypoints[i] = wp.transform;
            }
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

    private void OnDestroy()
    {
        // Dọn dẹp waypoints tự tạo khi enemy bị tiêu diệt
        if (generatedParent != null)
        {
            Destroy(generatedParent);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.timeScale == 0f || isWaiting || waypoints == null || waypoints.Length == 0) return; // Don't move if the game is paused, waiting, or no waypoints exist

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
