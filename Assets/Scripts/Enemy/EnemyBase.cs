using UnityEngine;

public class EnemyBase : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;
    public float changeTime = 3f;
    private Vector2 lastLookDir;
    private bool isLockedDirection = false;

    [Header("Combat")]
    public int dame;
    [Header("Chase")]
    public float stopDistance = 0.5f;

    [Header("Detect")]
    public float detectRange = 5f;

    [Header("Patrol Area")]
    public BoxCollider2D patrolArea;

    [Header("Health")]
    public int maxHealth = 10;
    public int currentHealth;

    private Rigidbody2D rb;
    private Animator animator;
    private Transform player;

    private Vector2 moveDir;
    private float timer;
    private bool isChasing = false;

    private Bounds bounds;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        currentHealth = maxHealth;
        timer = changeTime;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        if (patrolArea != null)
            bounds = patrolArea.bounds;

        ChooseRandomDirection();
    }

    void Update()
    {
        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);
            isChasing = distance <= detectRange;
        }

        if (!isChasing)
        {
            timer -= Time.deltaTime;
            if (timer <= 0)
            {
                ChooseRandomDirection();
                timer = changeTime;
            }
        }
    }

    void FixedUpdate()
    {
        if (isChasing)
            ChasePlayer();
        else
            Patrol();
    }

    void Patrol()
    {
        if (IsBlocked(moveDir))
        {
            ChooseRandomDirection();
            return;
        }

        Move(moveDir);
    }

    void ChasePlayer()
    {
        Vector2 direction = player.position - transform.position;
        float distance = direction.magnitude;

        // Nếu đủ gần → đứng yên + KHÓA hướng
        if (distance <= stopDistance)
        {
            if (!isLockedDirection)
            {
                direction.Normalize();

                // ❌ Không đi chéo
                if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                    lastLookDir = new Vector2(Mathf.Sign(direction.x), 0);
                else
                    lastLookDir = new Vector2(0, Mathf.Sign(direction.y));

                isLockedDirection = true;
            }

            // 👉 chỉ giữ hướng cũ, không update liên tục nữa
            animator.SetFloat("MoveX", lastLookDir.x);
            animator.SetFloat("MoveY", lastLookDir.y);

            return;
        }

        // Nếu ra xa → mở khóa
        isLockedDirection = false;

        direction.Normalize();

        if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
            direction = new Vector2(Mathf.Sign(direction.x), 0);
        else
            direction = new Vector2(0, Mathf.Sign(direction.y));

        if (IsBlocked(direction))
        {
            ChooseRandomDirection();
            return;
        }

        Move(direction);
    }

    void Move(Vector2 dir)
    {
        Vector2 newPos = rb.position + dir * speed * Time.fixedDeltaTime;

        // Giới hạn trong vùng BoxCollider
        if (patrolArea != null)
        {
            newPos.x = Mathf.Clamp(newPos.x, bounds.min.x, bounds.max.x);
            newPos.y = Mathf.Clamp(newPos.y, bounds.min.y, bounds.max.y);
        }

        rb.MovePosition(newPos);

        animator.SetFloat("MoveX", dir.x);
        animator.SetFloat("MoveY", dir.y);
    }

    void ChooseRandomDirection()
    {
        int rand = Random.Range(0, 4);

        switch (rand)
        {
            case 0: moveDir = Vector2.up; break;
            case 1: moveDir = Vector2.down; break;
            case 2: moveDir = Vector2.left; break;
            case 3: moveDir = Vector2.right; break;
        }
    }

    bool IsBlocked(Vector2 dir)
    {
        float distance = 0.5f;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dir, distance);

        if (hit.collider != null && !hit.collider.isTrigger)
        {
            return true;
        }

        return false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null)
        {
            player.ChangeHealth(-dame, DameType.TypeDamage.Monster);
        }

        ProjectTile projectile = other.GetComponent<ProjectTile>();
        if (projectile != null)
        {
            //animator.SetTrigger("Hit");
            Destroy(gameObject, 1f);
        }
    }
    public void ChangeHealth(int amount, DameType.TypeDamage type)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        if (currentHealth <= 0)
        {
                Destroy(gameObject);
        }
    }
}