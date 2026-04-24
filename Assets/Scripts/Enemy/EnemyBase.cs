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

    [Header("Quest")]
    [Tooltip("ID này phải khớp với objectiveID trong Quest ScriptableObject loại DefeatEnemy. Để 0 nếu con quái này không thuộc quest nào.")]
    public int enemyID = 0;

    [Header("Health")]
    public int maxHealth = 10;
    public int currentHealth;

    [Header("Health Regen")]
    public float regenDelay = 10f; // Thời gian kể từ khi mất máu đến lúc bắt đầu tự hồi phục
    public int regenPerSecond = 2; // Số máu hồi trên mỗi giây
    protected float lastDamageTime;
    protected float regenTimer;

    [Header("Obstacle Avoidance")]
    public bool enableObstacleAvoidance = true;
    public float avoidanceRadius = 0.3f;
    public float avoidanceDistance = 0.5f;

    [Header("Auto Roam (If no Waypoint)")]
    public bool enableAutoRoam = true;
    public float roamRadius = 3f;
    public float roamSpeed = 1f;
    public float roamWaitTime = 2f;
    
    protected Vector2 spawnPosition;
    protected Vector2 currentRoamTarget;
    protected bool isWaitingToRoam;
    protected float roamTimer;

    [Header("UI Head HealthBar Generator")]
    public GameObject healthBarPrefab; 
    public Vector3 healthBarOffset = new Vector3(0, 1.2f, 0); 
    protected EnemyHealthBarWorld spawnedHealthBar;
    protected GameObject spawnedHealthBarObj; // Lưu lại GameObject gốc của thanh máu

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

        spawnPosition = transform.position;
        SetNewRoamTarget();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        waypointMover = GetComponent<WaypointMover>();
        
        // Sinh ra thanh máu UI tự động & KO GẮN LAM CON để tránh lỗi toạ độ và lật lộn xộn
        if (healthBarPrefab != null)
        {
            spawnedHealthBarObj = Instantiate(healthBarPrefab); // Đẻ độc lập trên Map
            
            // Ép Scale nhỏ tẹo nếu bạn quên reset scale
            spawnedHealthBarObj.transform.localScale = new Vector3(0.015f, 0.015f, 0.015f);
            
            spawnedHealthBar = spawnedHealthBarObj.GetComponentInChildren<EnemyHealthBarWorld>();
            if (spawnedHealthBar != null)
            {
                spawnedHealthBar.UpdateHealth(currentHealth, maxHealth);
            }
        }
    }

    protected virtual void Update()
    {
        // Cho thanh máu đi theo đầu quái đều đặn
        if (spawnedHealthBarObj != null)
        {
            spawnedHealthBarObj.transform.position = transform.position + healthBarOffset;
        }

        if (player == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // Logic hồi phục máu (Regen Health) theo thời gian nếu chưa đầy
        if (currentHealth < maxHealth && currentHealth > 0)
        {
            if (Time.time - lastDamageTime >= regenDelay)
            {
                regenTimer += Time.deltaTime;
                if (regenTimer >= 1f) // Cứ trôi qua 1s thì bơm máu 1 lần
                {
                    ChangeHealth(regenPerSecond, DameType.TypeDamage.Heal);
                    regenTimer -= 1f; // Trừ đi thay vì gán = 0 để giữ mượt timing
                }
            }
        }

        if (player == null) return;

        attackTimer -= Time.deltaTime;

        float distance = Vector2.Distance(transform.position, player.position);
        isChasing = distance <= detectRange;
    }

    protected virtual void FixedUpdate()
    {
        if (player == null) return;
        
        if (isChasing)
        {
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
        else
        {
            if (waypointMover == null && enableAutoRoam)
            {
                AutoRoam();
            }
            else if (waypointMover == null)
            {
                // Đứng im nếu không bật tính năng roam
                rb.velocity = Vector2.zero;
                UpdateAnimator(lastLookDir, false);
            }
        }
    }

    protected virtual void AutoRoam()
    {
        if (isWaitingToRoam)
        {
            rb.velocity = Vector2.zero;
            UpdateAnimator(lastLookDir, false);
            
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
                lastLookDir = GetCardinalDirection(moveDir);
                Vector2 newPos = rb.position + moveDir * roamSpeed * Time.fixedDeltaTime;
                rb.MovePosition(newPos);
                UpdateAnimator(lastLookDir, true);
            }
            else
            {
                // Bị kẹt khi đang đi dạo: tìm đường khác lập tức
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

    protected void SetNewRoamTarget()
    {
        Vector2 randomDir = Random.insideUnitCircle * roamRadius;
        currentRoamTarget = spawnPosition + randomDir;
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
        Vector2 moveDirection = GetAvoidanceDirection(rawDirection);

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            lastLookDir = GetCardinalDirection(moveDirection);
            Vector2 newPos = rb.position + moveDirection * speed * Time.fixedDeltaTime;
            rb.MovePosition(newPos);
            UpdateAnimator(lastLookDir, true);
        }
        else
        {
            // Bị kẹt hoàn toàn thì đứng im nhưng vẫn nhìn vào player
            lastLookDir = GetCardinalDirection(rawDirection);
            UpdateAnimator(lastLookDir, false);
        }
    }

    protected Vector2 GetAvoidanceDirection(Vector2 targetDir)
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
        return Vector2.zero; // Bị kẹt hoàn toàn thì không cố đi thẳng vào tường
    }

    protected bool IsBlocked(Vector2 dir)
    {
        RaycastHit2D[] hits = new RaycastHit2D[5];
        ContactFilter2D filter = new ContactFilter2D();
        filter.useTriggers = false; // Bỏ qua các trigger
        
        int hitCount = Physics2D.CircleCast(rb.position, avoidanceRadius, dir, filter, hits, avoidanceDistance);
        
        for (int i = 0; i < hitCount; i++)
        {
            if (hits[i].collider == null) continue;
            // Bỏ qua chính bản thân
            if (hits[i].collider.gameObject == this.gameObject) continue;
            // Bỏ qua Player (theo yêu cầu "ngoài player")
            if (hits[i].collider.CompareTag("Player")) continue;
            
            if (!hits[i].collider.isTrigger) 
            {
                return true;
            }
        }
        return false;
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
        if (amount < 0 && type != DameType.TypeDamage.Heal)
        {
            lastDamageTime = Time.time; // Cập nhật lại thời gian nhận sát thương để reset Regen
            regenTimer = 0f;
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        // UpdateUI khi lượng máu thay đổi
        if (spawnedHealthBar != null)
        {
            spawnedHealthBar.UpdateHealth(currentHealth, maxHealth);
        }

        if (currentHealth <= 0)
        {
            animator.SetBool("isDead", true);
            Die();
        }
    }

    protected virtual void Die()
    {
        if (spawnedHealthBarObj != null)
        {
            Destroy(spawnedHealthBarObj); // Phá huỷ thanh máu khi chết
        }

        // Báo cáo quest diệt quái (chỉ khi enemyID hợp lệ)
        if (enemyID != 0)
        {
            QuestController.instance?.ReportEnemyKilled(enemyID);
        }

        Destroy(gameObject, 1f);
    }

    protected virtual void OnPlayerDetected() { }
    protected virtual void OnPlayerLost() { }
}