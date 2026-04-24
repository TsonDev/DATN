using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossEnemy : EnemyBase
{
    // =====================================================================
    // ENUMS
    // =====================================================================
    enum BossState
    {
        Idle,
        ShootAtPlayer,
        Shoot360,
        MeleeAttack,
        DashToPlayer,
        Summon
    }

    // =====================================================================
    // INSPECTOR - Shoot
    // =====================================================================
    [Header("=== BOSS: Bắn đạn ===")]
    [Tooltip("Prefab đạn (cần có EnemyProjecttile)")]
    public GameObject projectilePrefab;
    [Tooltip("Transform điểm bắn (tuỳ chọn). Nếu để trống, đạn sẽ spawn cách tâm boss 1 khoảng firePointOffset theo hướng bắn")]
    public Transform firePoint;
    public float bulletSpeed = 8f;
    public int bulletDamage = 1;

    [Tooltip("Khoảng cách spawn đạn tính từ tâm boss (dùng khi không có firePoint). Chỉnh lớn hơn nếu boss to")]
    public float firePointOffset = 1.5f;

    [Tooltip("Cooldown bắn đạn thẳng về phía Player")]
    public float shootCooldown = 2f;

    [Tooltip("Số đạn bắn mỗi lần (burst)")]
    public int burstCount = 3;
    [Tooltip("Khoảng cách giữa các viên đạn burst (giây)")]
    public float burstInterval = 0.15f;

    // =====================================================================
    // INSPECTOR - Shoot 360
    // =====================================================================
    [Header("=== BOSS: Bắn 360° ===")]
    [Tooltip("Cooldown bắn vòng tròn 360°")]
    public float shoot360Cooldown = 6f;
    [Tooltip("Số đạn trong vòng tròn")]
    public int shoot360BulletCount = 12;

    // =====================================================================
    // INSPECTOR - Melee
    // =====================================================================
    [Header("=== BOSS: Đánh gần ===")]
    public float meleeRange = 2.5f;
    public float meleeCooldown = 1.5f;
    public int meleeDamage = 3;

    // =====================================================================
    // INSPECTOR - Facing & Reposition
    // =====================================================================
    [Header("=== BOSS: Hướng mặt & Dịch vị ===")]
    [Tooltip("Boss chỉ có animation hướng trước (xuống). Khi player lên trên boss, boss sẽ dịch chuyển lên trên player")]
    public float repositionSpeed = 20f;
    [Tooltip("Khoảng cách boss sẽ đứng phía trên player sau khi dịch chuyển")]
    public float repositionAboveOffset = 2.5f;
    [Tooltip("Cooldown giữa các lần dịch vị (giây)")]
    public float repositionCooldown = 3f;

    // =====================================================================
    // INSPECTOR - Arena Bounds
    // =====================================================================
    [Header("=== BOSS: Khu vực hoạt động ===")]
    [Tooltip("Kéo thả BoxCollider2D của khu vực boss vào đây. Boss sẽ không ra ngoài vùng này")]
    public BoxCollider2D arenaArea;

    // =====================================================================
    // INSPECTOR - Dash
    // =====================================================================
    [Header("=== BOSS: Dịch chuyển (Dash) ===")]
    [Tooltip("Cooldown dịch chuyển")]
    public float dashCooldown = 5f;
    [Tooltip("Tốc độ dash")]
    public float dashSpeed = 15f;
    [Tooltip("Thời gian dash (giây)")]
    public float dashDuration = 0.3f;
    [Tooltip("Khoảng cách tối thiểu để boss muốn dash (nếu player quá gần thì không dash)")]
    public float minDashDistance = 3f;
    [Tooltip("Khoảng ngẫu nhiên thêm vào cooldown dash (0 ~ giá trị này)")]
    public float dashRandomExtra = 3f;

    // =====================================================================
    // INSPECTOR - Summon
    // =====================================================================
    [Header("=== BOSS: Triệu hồi ===")]
    [Tooltip("Prefab quái để triệu hồi")]
    public GameObject[] summonPrefabs;
    [Tooltip("Số quái tối đa tồn tại cùng lúc")]
    public int maxSummonCount = 3;
    [Tooltip("Cooldown triệu hồi")]
    public float summonCooldown = 8f;
    [Tooltip("Số quái triệu hồi mỗi lần")]
    public int summonPerWave = 2;
    [Tooltip("Bán kính spawn quanh Boss")]
    public float summonRadius = 2f;

    // =====================================================================
    // INSPECTOR - Enrage (Giai đoạn 2)
    // =====================================================================
    [Header("=== BOSS: Enrage (≤50% HP) ===")]
    [Tooltip("Màu khi enrage (đỏ hơn)")]
    public Color enrageColor = new Color(1f, 0.3f, 0.3f, 1f);
    [Tooltip("Tốc độ chuyển màu")]
    public float colorTransitionSpeed = 2f;

    // =====================================================================
    // INSPECTOR - VFX
    // =====================================================================
    [Header("=== VFX ===")]
    public GameObject dashTrailPrefab;
    public GameObject summonEffectPrefab;
    public GameObject shoot360EffectPrefab;

    // =====================================================================
    // PRIVATE STATE
    // =====================================================================
    private BossState currentState = BossState.Idle;
    private SpriteRenderer spriteRenderer;

    // Cooldown timers
    private float shootTimer;
    private float shoot360Timer;
    private float meleeTimer;
    private float dashTimer;
    private float summonTimer;

    // Dash state
    private bool isDashing = false;
    private Vector2 dashDirection;
    private float dashTimeLeft;

    // Enrage
    private bool isEnraged = false;
    private Color originalColor = Color.white;
    private float damageMultiplier = 1f;
    private float cooldownMultiplier = 1f;
    private float speedMultiplier = 1f;

    // Summon tracking
    private List<GameObject> activeSummons = new List<GameObject>();

    // State lock (đang thực hiện chiêu thì không chuyển state)
    private bool isPerformingAction = false;

    // Reposition
    private float repositionTimer;
    private bool isRepositioning = false;

    // Arena activation: boss chỉ kích hoạt khi player vào arenaArea
    private bool bossActivated = false;

    // =====================================================================
    // LIFECYCLE
    // =====================================================================
    protected override void Start()
    {
        base.Start();

        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        // Khởi tạo cooldown (sẵn sàng dùng ngay)
        shootTimer = 0f;
        shoot360Timer = shoot360Cooldown * 0.5f; // Chờ 1 chút trước khi dùng 360
        meleeTimer = 0f;
        dashTimer = dashCooldown * 0.3f;
        summonTimer = summonCooldown * 0.5f;
        repositionTimer = 0f;
    }

    protected override void Update()
    {
        if (player == null)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }
        if (player == null) return;

        // Boss chỉ kích hoạt khi player vào khu vực arenaArea
        if (arenaArea != null)
        {
            bool playerInArena = arenaArea.bounds.Contains(player.position);
            if (!bossActivated)
            {
                if (!playerInArena) return; // Player chưa vào arena → boss không làm gì
                bossActivated = true; // Player vừa bước vào → kích hoạt boss!
                Debug.Log("[BOSS] Player vào khu vực boss! Boss kích hoạt!");
            }
            // Sau khi kích hoạt, boss luôn hoạt động (không ngừng dù player ra ngoài)
            isChasing = true;
        }
        else
        {
            // Không có arenaArea → dùng detectRange mặc định
            float distance = Vector2.Distance(transform.position, player.position);
            isChasing = distance <= detectRange;
        }

        attackTimer -= Time.deltaTime;

        if (currentHealth <= 0) return;

        // Tắt waypointMover khi đang đuổi theo player
        if (waypointMover != null)
            waypointMover.enabled = !isChasing;

        // Kiểm tra enrage
        CheckEnrage();

        // Cập nhật màu enrage mượt mà
        UpdateEnrageColor();

        // Dọn dẹp summon đã chết
        CleanupDeadSummons();

        // Kiểm tra dịch vị (khi player lên trên boss)
        repositionTimer -= Time.deltaTime;
        CheckReposition();

        // Nếu đang dash, dịch vị, không chọn state mới
        if (isDashing || isRepositioning) return;

        // Nếu đang thực hiện chiêu (coroutine), không chuyển state
        if (isPerformingAction) return;

        // Giảm cooldown
        shootTimer -= Time.deltaTime;
        shoot360Timer -= Time.deltaTime;
        meleeTimer -= Time.deltaTime;
        dashTimer -= Time.deltaTime;
        summonTimer -= Time.deltaTime;

        // Chọn chiêu
        if (isChasing)
        {
            ChooseAction();
        }
    }

    protected override void FixedUpdate()
    {
        if (player == null) return;
        if (currentHealth <= 0) return;

        // Nếu đang dash hoặc dịch vị
        if (isDashing)
        {
            PerformDash();
            return;
        }
        if (isRepositioning) return;

        if (!isChasing) return;
        if (isPerformingAction) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // Luôn di chuyển về phía player khi không bận chiêu nào
        if (distance > stopDistance)
        {
            MoveToPlayer();
        }
        else
        {
            StopMoveAndFaceTarget();
        }
    }

    // =====================================================================
    // CHỌN CHIÊU (AI LOGIC)
    // =====================================================================
    private void ChooseAction()
    {
        float distance = Vector2.Distance(transform.position, player.position);

        // =================================================================
        // Ưu tiên 1: Melee CHỈ KHI boss đã đến rất gần player
        // (stopDistance + buffer nhỏ → boss phải đi sát player mới đánh)
        // =================================================================
        float meleeTriggerDist = stopDistance + 0.5f;
        if (distance <= meleeTriggerDist && meleeTimer <= 0f)
        {
            currentState = BossState.MeleeAttack;
            StartCoroutine(DoMeleeAttack());
            return;
        }

        // =================================================================
        // Ưu tiên 2: Bắn thẳng về Player (khi ở xa, ưu tiên cao hơn 360)
        // =================================================================
        if (shootTimer <= 0f && distance > meleeTriggerDist)
        {
            currentState = BossState.ShootAtPlayer;
            StartCoroutine(DoShootAtPlayer());
            return;
        }

        // =================================================================
        // Ưu tiên 3: Bắn 360° nếu đủ cooldown
        // =================================================================
        if (shoot360Timer <= 0f)
        {
            currentState = BossState.Shoot360;
            StartCoroutine(DoShoot360());
            return;
        }

        // =================================================================
        // Ưu tiên 4: Dash nếu player xa và đủ cooldown
        // =================================================================
        if (distance >= minDashDistance && dashTimer <= 0f)
        {
            currentState = BossState.DashToPlayer;
            StartDash();
            return;
        }

        // =================================================================
        // Ưu tiên 5: Triệu hồi khi enrage (không cần gần player)
        // =================================================================
        if (isEnraged && summonTimer <= 0f && GetAliveSummonCount() < maxSummonCount)
        {
            currentState = BossState.Summon;
            StartCoroutine(DoSummon());
            return;
        }

        // =================================================================
        // Mặc định: Idle → boss sẽ đi về phía Player trong FixedUpdate
        // =================================================================
        currentState = BossState.Idle;
    }

    // =====================================================================
    // CHIÊU 1: BẮN ĐẠN VỀ PHÍA PLAYER
    // =====================================================================
    private IEnumerator DoShootAtPlayer()
    {
        isPerformingAction = true;

        // Dừng di chuyển
        rb.velocity = Vector2.zero;
        StopMoveAndFaceTarget();

        // Animation bắn
        if (animator != null)
            animator.SetTrigger("Shoot");

        // Burst bắn
        for (int i = 0; i < burstCount; i++)
        {
            if (player == null) break;

            // Tính hướng bắn tại thời điểm bắn → đạn bay thẳng, không đuổi
            Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
            SpawnProjectile(dir);

            if (i < burstCount - 1)
                yield return new WaitForSeconds(burstInterval);
        }

        // Áp dụng cooldown
        shootTimer = shootCooldown * cooldownMultiplier;

        yield return new WaitForSeconds(0.3f);

        isPerformingAction = false;
        currentState = BossState.Idle;
    }

    // =====================================================================
    // CHIÊU 2: BẮN 360°
    // =====================================================================
    private IEnumerator DoShoot360()
    {
        isPerformingAction = true;

        rb.velocity = Vector2.zero;
        StopMoveAndFaceTarget();

        // Animation
        if (animator != null)
            animator.SetTrigger("Shoot");

        // VFX
        if (shoot360EffectPrefab != null)
        {
            GameObject fx = Instantiate(shoot360EffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 1.5f);
        }

        // Chờ 1 chút để báo hiệu
        yield return new WaitForSeconds(0.4f);

        // Bắn tròn 360°
        float angleStep = 360f / shoot360BulletCount;
        for (int i = 0; i < shoot360BulletCount; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            SpawnProjectile(dir);
        }

        // Cooldown
        shoot360Timer = shoot360Cooldown * cooldownMultiplier;

        yield return new WaitForSeconds(0.5f);

        isPerformingAction = false;
        currentState = BossState.Idle;
    }

    // =====================================================================
    // CHIÊU 3: ĐÁNH GẦN (MELEE)
    // =====================================================================
    private IEnumerator DoMeleeAttack()
    {
        isPerformingAction = true;

        rb.velocity = Vector2.zero;
        StopMoveAndFaceTarget();

        // Animation đánh cận chiến (boss đã có animation riêng, không cần spawn weapon prefab)
        if (animator != null)
            animator.SetTrigger("Attack");

        // Chờ animation đánh thực hiện
        yield return new WaitForSeconds(0.15f);

        // Gây damage nếu player vẫn trong tầm (dùng OverlapCircle + TakeHit giống DamageAndKnockback)
        int finalDamage = Mathf.RoundToInt(meleeDamage * damageMultiplier);
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, meleeRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerController playerCtrl = hit.GetComponent<PlayerController>();
                if (playerCtrl != null)
                {
                    // Tính hướng đẩy lùi từ boss đến player (giống DamageAndKnockback)
                    Vector2 hitDirection = (hit.transform.position - transform.position).normalized;
                    playerCtrl.TakeHit(finalDamage, hitDirection, DameType.TypeDamage.Monster);
                }
            }
        }

        // Cooldown
        meleeTimer = meleeCooldown * cooldownMultiplier;

        yield return new WaitForSeconds(0.3f);

        isPerformingAction = false;
        currentState = BossState.Idle;
    }

    // =====================================================================
    // CHIÊU 4: DỊCH CHUYỂN (DASH) VỀ PHÍA PLAYER
    // =====================================================================
    private void StartDash()
    {
        if (player == null) return;

        isDashing = true;
        isPerformingAction = true;

        dashDirection = ((Vector2)player.position - rb.position).normalized;
        dashTimeLeft = dashDuration;

        // VFX trail
        if (dashTrailPrefab != null)
        {
            GameObject trail = Instantiate(dashTrailPrefab, transform.position, Quaternion.identity);
            trail.transform.SetParent(transform);
            Destroy(trail, dashDuration + 0.5f);
        }

        // Animation (luôn giữ hướng mặt xuống dưới)
        if (animator != null)
        {
            animator.SetBool("isMoving", true);
        }

        // Cooldown (thêm random)
        dashTimer = (dashCooldown + Random.Range(0f, dashRandomExtra)) * cooldownMultiplier;
    }

    private void PerformDash()
    {
        dashTimeLeft -= Time.fixedDeltaTime;

        if (dashTimeLeft <= 0f)
        {
            // Kết thúc dash
            isDashing = false;
            isPerformingAction = false;
            rb.velocity = Vector2.zero;
            currentState = BossState.Idle;

            if (animator != null)
                animator.SetBool("isMoving", false);
            return;
        }

        // Di chuyển nhanh
        float currentDashSpeed = dashSpeed * speedMultiplier;
        Vector2 newPos = rb.position + dashDirection * currentDashSpeed * Time.fixedDeltaTime;

        // Clamp trong khu vực arena
        newPos = ClampToArena(newPos);

        rb.MovePosition(newPos);
    }

    // =====================================================================
    // CHIÊU 5: TRIỆU HỒI
    // =====================================================================
    private IEnumerator DoSummon()
    {
        isPerformingAction = true;

        rb.velocity = Vector2.zero;
        StopMoveAndFaceTarget();

        // Triệu hồi không cần animation Attack (tránh nhầm với đánh tay)
        // Nếu có Animator trigger "Summon" riêng thì dùng, không thì bỏ qua
        // if (animator != null)
        //     animator.SetTrigger("Summon");

        // VFX
        if (summonEffectPrefab != null)
        {
            GameObject fx = Instantiate(summonEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 1.5f);
        }

        yield return new WaitForSeconds(0.5f);

        // Spawn quái
        int currentAlive = GetAliveSummonCount();
        int canSpawn = Mathf.Min(summonPerWave, maxSummonCount - currentAlive);

        for (int i = 0; i < canSpawn; i++)
        {
            if (summonPrefabs == null || summonPrefabs.Length == 0) break;

            // Chọn ngẫu nhiên loại quái
            GameObject prefab = summonPrefabs[Random.Range(0, summonPrefabs.Length)];
            if (prefab == null) continue;

            // Vị trí spawn ngẫu nhiên quanh boss
            Vector2 spawnOffset = Random.insideUnitCircle.normalized * summonRadius;
            Vector3 spawnPos = transform.position + (Vector3)spawnOffset;

            GameObject summon = Instantiate(prefab, spawnPos, Quaternion.identity);
            activeSummons.Add(summon);

            // Hiệu ứng spawn nhỏ cho từng con
            if (summonEffectPrefab != null)
            {
                GameObject spawnFx = Instantiate(summonEffectPrefab, spawnPos, Quaternion.identity);
                Destroy(spawnFx, 1f);
            }
        }

        // Cooldown
        summonTimer = summonCooldown * cooldownMultiplier;

        yield return new WaitForSeconds(0.3f);

        isPerformingAction = false;
        currentState = BossState.Idle;
    }

    // =====================================================================
    // ENRAGE SYSTEM
    // =====================================================================
    private void CheckEnrage()
    {
        if (isEnraged) return;

        if (currentHealth <= maxHealth * 0.5f)
        {
            ActivateEnrage();
        }
    }

    private void ActivateEnrage()
    {
        isEnraged = true;

        // Nhân đôi chỉ số
        damageMultiplier = 2f;
        speedMultiplier = 2f;
        speed *= 2f;              // Tốc độ di chuyển (từ EnemyBase)
        dame *= 2;                // Damage base (từ EnemyBase)
        bulletDamage *= 2;
        meleeDamage *= 2;

        // Giảm cooldown 50%
        cooldownMultiplier = 0.5f;

        Debug.Log("[BOSS] ENRAGE ACTIVATED! Tất cả chỉ số x2, cooldown giảm 50%!");
    }

    private void UpdateEnrageColor()
    {
        if (spriteRenderer == null) return;

        Color targetColor = isEnraged ? enrageColor : originalColor;
        spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, Time.deltaTime * colorTransitionSpeed);
    }

    // =====================================================================
    // SUMMON TRACKING
    // =====================================================================
    private void CleanupDeadSummons()
    {
        activeSummons.RemoveAll(s => s == null);
    }

    private int GetAliveSummonCount()
    {
        CleanupDeadSummons();
        return activeSummons.Count;
    }

    // =====================================================================
    // PROJECTILE HELPER
    // =====================================================================
    private void SpawnProjectile(Vector2 direction)
    {
        if (projectilePrefab == null) return;

        // Nếu có firePoint transform → dùng nó, 
        // nếu không → spawn cách tâm boss 1 khoảng firePointOffset theo hướng bắn
        // (phù hợp boss kích thước lớn, đạn phóng ra từ rìa body thay vì từ tâm)
        Vector2 spawnPos;
        if (firePoint != null)
        {
            spawnPos = (Vector2)firePoint.position;
        }
        else
        {
            spawnPos = (Vector2)transform.position + direction.normalized * firePointOffset;
        }

        GameObject bullet = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        var proj = bullet.GetComponent<EnemyProjecttile>();
        if (proj != null)
        {
            proj.damage = Mathf.RoundToInt(bulletDamage * damageMultiplier);
            proj.LaunchProjectile(direction, bulletSpeed);
        }
    }

    // =====================================================================
    // OVERRIDE: HEALTH & DEATH
    // =====================================================================
    public override void ChangeHealth(int amount, DameType.TypeDamage type)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        if (currentHealth <= 0)
        {
            if (animator != null)
                animator.SetBool("isDead", true);

            // Dừng mọi coroutine
            StopAllCoroutines();
            isDashing = false;
            isPerformingAction = false;

            Die();
        }
    }

    protected override void Die()
    {
        // Xoá tất cả quái triệu hồi khi boss chết
        foreach (var summon in activeSummons)
        {
            if (summon != null)
                Destroy(summon);
        }
        activeSummons.Clear();

        Debug.Log("[BOSS] Boss đã bị tiêu diệt!");

        Destroy(gameObject, 1f);
    }

    // =====================================================================
    // OVERRIDE: Boss luôn hướng mặt xuống (phía trước), không xoay 4 hướng
    // =====================================================================
    protected override void UpdateAnimator(Vector2 direction, bool isMoving)
    {
        if (animator == null) return;

        // Boss chỉ có animation hướng trước (xuống) → luôn set cố định
        animator.SetFloat("lastInputX", 0f);
        animator.SetFloat("lastInputY", -1f);
        animator.SetBool("isMoving", isMoving);
    }

    protected override void StopMoveAndFaceTarget()
    {
        // Dừng di chuyển, nhưng KHÔNG đổi hướng nhìn (boss luôn nhìn xuống)
        rb.velocity = Vector2.zero;
        UpdateAnimator(Vector2.down, false);
    }

    protected override void MoveToPlayer()
    {
        Vector2 rawDirection = ((Vector2)player.position - rb.position).normalized;
        Vector2 moveDirection = GetAvoidanceDirection(rawDirection);

        if (moveDirection.sqrMagnitude > 0.001f)
        {
            float currentSpeed = speed;
            Vector2 newPos = rb.position + moveDirection * currentSpeed * Time.fixedDeltaTime;

            // Clamp trong khu vực arena
            newPos = ClampToArena(newPos);

            rb.MovePosition(newPos);
            UpdateAnimator(Vector2.down, true);
        }
        else
        {
            UpdateAnimator(Vector2.down, false);
        }
    }

    // =====================================================================
    // ARENA BOUNDS: Giới hạn vị trí boss trong khu vực BoxCollider2D
    // =====================================================================
    private Vector2 ClampToArena(Vector2 position)
    {
        if (arenaArea == null) return position;

        Bounds bounds = arenaArea.bounds;
        position.x = Mathf.Clamp(position.x, bounds.min.x, bounds.max.x);
        position.y = Mathf.Clamp(position.y, bounds.min.y, bounds.max.y);
        return position;
    }

    // =====================================================================
    // REPOSITION: Dịch chuyển lên trên player khi player ở phía trên boss
    // =====================================================================
    private void CheckReposition()
    {
        if (player == null) return;
        if (isDashing || isRepositioning || isPerformingAction) return;
        if (repositionTimer > 0f) return;

        // Nếu player ở TRÊN boss (player.y > boss.y + ngưỡng nhỏ)
        if (player.position.y > transform.position.y + 0.5f)
        {
            StartCoroutine(DoReposition());
        }
    }

    private IEnumerator DoReposition()
    {
        isRepositioning = true;
        isPerformingAction = true;

        // Vị trí đích: phía trên player (clamp trong arena)
        Vector2 targetPos = new Vector2(player.position.x, player.position.y + repositionAboveOffset);
        targetPos = ClampToArena(targetPos);

        // Di chuyển nhanh đến vị trí đích
        while (Vector2.Distance(rb.position, targetPos) > 0.2f)
        {
            Vector2 newPos = Vector2.MoveTowards(rb.position, targetPos, repositionSpeed * Time.fixedDeltaTime);
            newPos = ClampToArena(newPos);
            rb.MovePosition(newPos);
            UpdateAnimator(Vector2.down, true);
            yield return new WaitForFixedUpdate();
        }

        rb.MovePosition(targetPos);
        rb.velocity = Vector2.zero;
        UpdateAnimator(Vector2.down, false);

        repositionTimer = repositionCooldown;
        isRepositioning = false;
        isPerformingAction = false;
        currentState = BossState.Idle;
    }

    // =====================================================================
    // DEBUG GIZMOS
    // =====================================================================
    void OnDrawGizmosSelected()
    {
        // Tầm phát hiện
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);

        // Tầm melee
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);

        // Tầm dash tối thiểu
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, minDashDistance);

        // Bán kính triệu hồi
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, summonRadius);

        // Vẽ arena bounds
        if (arenaArea != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(arenaArea.bounds.center, arenaArea.bounds.size);
        }
    }
}
