using System.Collections;
using UnityEngine;

public class EnemyExploder : MonoBehaviour
{
    [Header("Detect")]
    public float detectRange = 5f;
    public float explodeRange = 1.2f;
    public float moveSpeed = 2.5f;

    [Header("Obstacle Avoidance")]
    public bool enableObstacleAvoidance = true;
    public float avoidanceRadius = 0.3f;
    public float avoidanceDistance = 0.5f;

    [Header("Auto Roam (If no Waypoint)")]
    public bool enableAutoRoam = true;
    public float roamRadius = 3f;
    public float roamSpeed = 1f;
    public float roamWaitTime = 2f;
    
    private Vector2 spawnPosition;
    private Vector2 currentRoamTarget;
    private bool isWaitingToRoam;
    private float roamTimer;

    [Header("Explosion")]
    public float explodeDelay = 0.6f;
    public int damage = 3;
    public GameObject explosionPrefab;

    [Header("Flash Effect")]
    public Color flashColor = Color.red;
    public float minFlashSpeed = 0.03f;
    public float maxFlashSpeed = 0.15f;

    private Transform player;
    private Animator animator;
    private Rigidbody2D rb;
    private WaypointMover waypointMover;
    private SpriteRenderer spriteRenderer;

    private bool isExploding = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        waypointMover = GetComponent<WaypointMover>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (rb != null)
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        spawnPosition = transform.position;
        SetNewRoamTarget();
    }

    void Update()
    {
        if (player == null || isExploding) return;

        float dist = Vector2.Distance(transform.position, player.position);

        // bật/tắt waypoint
        if (waypointMover != null)
            waypointMover.enabled = dist > detectRange;
    }

    void FixedUpdate()
    {
        if (player == null || isExploding) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= detectRange)
        {
            Vector2 dir = (player.position - transform.position).normalized;

            // Nếu đủ gần → nổ
            if (dist <= explodeRange)
            {
                rb.velocity = Vector2.zero;
                animator?.SetBool("isMoving", false);

                StartCoroutine(Explode());
            }
            else
            {
                // đuổi player, kết hợp tránh vật cản
                Vector2 moveDir = GetAvoidanceDirection(dir);
                
                if (moveDir.sqrMagnitude > 0.001f)
                {
                    rb.velocity = moveDir * moveSpeed * 2;
                    animator?.SetBool("isMoving", true);
                    animator?.SetFloat("lastInputX", moveDir.x);
                    animator?.SetFloat("lastInputY", moveDir.y);
                }
                else
                {
                    rb.velocity = Vector2.zero;
                    animator?.SetBool("isMoving", false);
                    animator?.SetFloat("lastInputX", dir.x);
                    animator?.SetFloat("lastInputY", dir.y);
                }
            }
        }
        else
        {
            if (waypointMover == null && enableAutoRoam)
            {
                AutoRoam();
            }
            else if (waypointMover == null)
            {
                rb.velocity = Vector2.zero;
                animator?.SetBool("isMoving", false);
            }
        }
    }

    private void AutoRoam()
    {
        if (isWaitingToRoam)
        {
            rb.velocity = Vector2.zero;
            animator?.SetBool("isMoving", false);
            
            roamTimer -= Time.fixedDeltaTime;
            if (roamTimer <= 0)
            {
                isWaitingToRoam = false;
                SetNewRoamTarget();
            }
        }
        else
        {
            Vector2 dir = (currentRoamTarget - rb.position).normalized;
            Vector2 moveDir = GetAvoidanceDirection(dir);
            
            if (moveDir.sqrMagnitude > 0.001f)
            {
                rb.velocity = moveDir * roamSpeed;
                animator?.SetBool("isMoving", true);
                animator?.SetFloat("lastInputX", moveDir.x);
                animator?.SetFloat("lastInputY", moveDir.y);
            }
            else
            {
                isWaitingToRoam = true;
                roamTimer = roamWaitTime;
            }

            if (Vector2.Distance(rb.position, currentRoamTarget) < 0.2f)
            {
                isWaitingToRoam = true;
                roamTimer = roamWaitTime;
            }
        }
    }

    private void SetNewRoamTarget()
    {
        Vector2 randomDir = Random.insideUnitCircle * roamRadius;
        currentRoamTarget = spawnPosition + randomDir;
    }

    private Vector2 GetAvoidanceDirection(Vector2 targetDir)
    {
        if (!enableObstacleAvoidance) return targetDir;

        if (!IsBlocked(targetDir)) return targetDir;

        float[] tryAngles = new float[] { 30f, -30f, 60f, -60f, 90f, -90f };
        foreach (float angle in tryAngles)
        {
            Vector2 currentDir = Quaternion.Euler(0, 0, angle) * targetDir;
            if (!IsBlocked(currentDir))
            {
                return currentDir;
            }
        }
        return Vector2.zero;
    }

    private bool IsBlocked(Vector2 dir)
    {
        RaycastHit2D[] hits = new RaycastHit2D[5];
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false; // Bỏ qua trigger Collider
        
        int hitCount = Physics2D.CircleCast(rb.position, avoidanceRadius, dir, filter, hits, avoidanceDistance);
        
        for (int i = 0; i < hitCount; i++)
        {
            if (hits[i].collider == null) continue;
            if (hits[i].collider.gameObject == this.gameObject) continue;
            if (hits[i].collider.CompareTag("Player")) continue;
            
            if (!hits[i].collider.isTrigger) 
            {
                return true;
            }
        }
        return false;
    }

    IEnumerator Explode()
    {
        isExploding = true;

        // animation
        animator?.SetTrigger("Attack");

        // nhấp nháy đỏ
        StartCoroutine(FlashRed());

        yield return new WaitForSeconds(explodeDelay);

        // hiệu ứng nổ
        if (explosionPrefab != null)
        {
            GameObject fx = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 1f);
        }

        // gây damage
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explodeRange);

        foreach (var hit in hits)
        {
            PlayerController playerCtrl = hit.GetComponent<PlayerController>();
            if (playerCtrl != null)
            {
                playerCtrl.ChangeHealth(-damage, DameType.TypeDamage.Monster);
            }
        }

        Destroy(gameObject);
    }

    IEnumerator FlashRed()
    {
        if (spriteRenderer == null) yield break;

        Color originalColor = spriteRenderer.color;
        float timer = 0f;

        while (timer < explodeDelay)
        {
            float t = timer / explodeDelay;
            float speed = Mathf.Lerp(maxFlashSpeed, minFlashSpeed, t);

            spriteRenderer.color = flashColor;
            yield return new WaitForSeconds(speed);

            spriteRenderer.color = originalColor;
            yield return new WaitForSeconds(speed);

            timer += speed * 2;
        }

        spriteRenderer.color = originalColor;
    }

    // debug vùng nổ
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explodeRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}