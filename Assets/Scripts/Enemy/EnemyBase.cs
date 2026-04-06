using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;
    public float stopDistance = 0.5f;
    public float detectRange = 5f;

    [Header("Combat")]
    public int dame = 1;
    public float attackCooldown = 1f;

    [Header("Health")]
    public int maxHealth = 10;
    public int currentHealth;

    protected Rigidbody2D rb;
    protected Animator animator;
    protected Transform player;

    protected bool isChasing = false;
    protected Vector2 lastLookDir = Vector2.down;

    protected float attackTimer;
    protected WaypointMover waypointMover;

    protected virtual void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        attackTimer = 0f;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        waypointMover = GetComponent<WaypointMover>();
    }

    protected virtual void Update()
    {
        if (player == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        if (player == null) return;

        attackTimer -= Time.deltaTime;

        float distance = Vector2.Distance(transform.position, player.position);
        isChasing = distance <= detectRange;
    }

    protected virtual void FixedUpdate()
    {
        if (player == null) return;
        if (!isChasing) return;

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > stopDistance)
        {
            MoveToPlayer();
        }
        else
        {
            StopMoveAndFaceTarget();

            if (CanAttack())
                Attack();
        }
    }

    protected Vector2 GetCardinalDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude < 0.0001f)
            return lastLookDir;

        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
            return new Vector2(Mathf.Sign(dir.x), 0f);

        return new Vector2(0f, Mathf.Sign(dir.y));
    }

    protected virtual void MoveToPlayer()
    {
        Vector2 rawDirection = ((Vector2)player.position - rb.position).normalized;

        // chỉ cập nhật hướng ở lúc ĐANG DI CHUYỂN
        lastLookDir = GetCardinalDirection(rawDirection);

        Vector2 newPos = rb.position + rawDirection * speed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);

        UpdateAnimator(lastLookDir, true);
    }

    protected virtual void StopMoveAndFaceTarget()
    {
        // Dừng di chuyển
        rb.velocity = Vector2.zero;

        // Cập nhật lại hướng nhìn (lastLookDir) về phía Player hiện tại
        Vector2 rawDirection = ((Vector2)player.position - rb.position).normalized;
        lastLookDir = GetCardinalDirection(rawDirection);

        // Cập nhật Animator
        UpdateAnimator(lastLookDir, false);
    }

    protected virtual bool CanAttack()
    {
        return attackTimer <= 0f;
    }

    protected virtual void Attack()
    {
        attackTimer = attackCooldown;

        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            Debug.Log("Enemy hit dir = " + lastLookDir);
            playerController.TakeHit(dame, lastLookDir, DameType.TypeDamage.Monster);
        }
    }

    protected virtual void UpdateAnimator(Vector2 direction, bool isMoving)
    {
        if (animator == null) return;

        animator.SetFloat("lastInputX", direction.x);
        animator.SetFloat("lastInputY", direction.y);
        animator.SetBool("isMoving", isMoving);
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        ProjectTile projectile = other.GetComponent<ProjectTile>();
        if (projectile != null)
        {
            ChangeHealth(-projectile.damage, DameType.TypeDamage.Projectile);
        }
    }

    public virtual void ChangeHealth(int amount, DameType.TypeDamage type)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        if (currentHealth <= 0)
        {
            animator.SetBool("isDead", true);
            Die();
        }
    }

    protected virtual void Die()
    {
        Destroy(gameObject, 1f);
    }

    protected virtual void OnPlayerDetected() { }
    protected virtual void OnPlayerLost() { }
}