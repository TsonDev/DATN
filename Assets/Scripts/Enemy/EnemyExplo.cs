using System.Collections;
using UnityEngine;

public class EnemyExploder : MonoBehaviour
{
    [Header("Detect")]
    public float detectRange = 5f;
    public float explodeRange = 1.2f;
    public float moveSpeed = 2.5f;

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
                // đuổi player
                rb.velocity = dir * moveSpeed;

                animator?.SetBool("isMoving", true);
                animator?.SetFloat("lastInputX", dir.x);
                animator?.SetFloat("lastInputY", dir.y);
            }
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
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