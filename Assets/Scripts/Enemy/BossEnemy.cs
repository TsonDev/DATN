using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BossEnemy : EnemyBase
{
    // =========================================================
    // ENUMS
    // =========================================================
    enum BossState { Idle, Chase, ShootAtPlayer, Shoot360, MeleeAttack, DashToPlayer, Summon }

    // =========================================================
    // INSPECTOR – Shoot
    // =========================================================
    [Header("=== BOSS: Bắn đạn ===")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float bulletSpeed      = 8f;
    public int   bulletDamage     = 1;
    public float firePointOffset  = 1.5f;
    public float shootCooldown    = 2f;
    public int   burstCount       = 3;
    public float burstInterval    = 0.15f;

    // =========================================================
    // INSPECTOR – Shoot 360
    // =========================================================
    [Header("=== BOSS: Bắn 360° ===")]
    public float shoot360Cooldown    = 6f;
    public int   shoot360BulletCount = 12;

    // =========================================================
    // INSPECTOR – Melee
    // =========================================================
    [Header("=== BOSS: Đánh gần ===")]
    public float meleeRange    = 2.5f;
    public float meleeCooldown = 1.5f;
    public int   meleeDamage   = 3;

    // =========================================================
    // INSPECTOR – Dash
    // =========================================================
    [Header("=== BOSS: Dash ===")]
    public float dashCooldown    = 4f;
    public float dashSpeed       = 20f;
    public float dashDuration    = 0.25f;
    public float minDashDistance = 3f;
    public float dashRandomExtra = 2f;

    // =========================================================
    // INSPECTOR – Summon
    // =========================================================
    [Header("=== BOSS: Triệu hồi ===")]
    public GameObject[] summonPrefabs;
    public int   maxSummonCount = 3;
    public float summonCooldown = 8f;
    public int   summonPerWave  = 2;
    public float summonRadius   = 2.5f;

    // =========================================================
    // INSPECTOR – Arena
    // =========================================================
    [Header("=== BOSS: Arena ===")]
    public BoxCollider2D arenaArea;

    // =========================================================
    // INSPECTOR – Enrage
    // =========================================================
    [Header("=== BOSS: Enrage (≤50% HP) ===")]
    public Color enrageColor           = new Color(1f, 0.25f, 0.25f, 1f);
    public float colorTransitionSpeed  = 2f;

    // =========================================================
    // INSPECTOR – Animation mode
    // =========================================================
    [Header("=== Animation ===")]
    [Tooltip("True = boss có animation riêng 4 hướng (MoveUp/Down/Left/Right). " +
             "False = chỉ dùng 1 hướng + flip sprite theo trục X.")]
    public bool use4DirectionAnim = false;

    // =========================================================
    // INSPECTOR – Reposition (boss lên trên player)
    // =========================================================
    [Header("=== BOSS: Dịch vị (lên trên player) ===")]
    [Tooltip("Ngưỡng Y: player cần cao hơn boss bao nhiêu thì boss mới dịch vị")]
    public float repositionYThreshold  = 1.0f;
    [Tooltip("Boss sẽ đứng cao hơn player bao nhiêu đơn vị sau khi dịch vị")]
    public float repositionAboveOffset = 2.0f;
    [Tooltip("Tốc độ di chuyển khi dịch vị (rất nhanh)")]
    public float repositionSpeed       = 14f;
    [Tooltip("Cooldown giữa các lần dịch vị (giây)")]
    public float repositionCooldown    = 5f;
    [Tooltip("Lực đẩy xuống gây ra cho player khi boss đã đứng phía trên")]
    public float repositionKnockdownForce = 8f;

    // =========================================================
    // INSPECTOR – VFX
    // =========================================================
    [Header("=== VFX ===")]
    public GameObject dashTrailPrefab;
    public GameObject summonEffectPrefab;
    public GameObject shoot360EffectPrefab;

    // =========================================================
    // PRIVATE STATE
    // =========================================================
    private BossState currentState = BossState.Idle;
    private SpriteRenderer spriteRend;

    // Cooldown timers
    private float shootTimer;
    private float shoot360Timer;
    private float meleeTimer;
    private float dashTimer;
    private float summonTimer;

    // Dash
    private bool    isDashing;
    private Vector2 dashDir;
    private float   dashTimeLeft;

    // Enrage
    private bool  isEnraged;
    private Color originalColor  = Color.white;
    private float dmgMult        = 1f;
    private float cooldownMult   = 1f;
    private float speedMult      = 1f;

    // Summons
    private List<GameObject> activeSummons = new List<GameObject>();

    // Action lock
    private bool isPerformingAction;

    // Arena / activation
    private bool bossActivated;

    // Reposition
    private bool  isRepositioning;
    private float repositionTimer;

    // =========================================================
    // LIFECYCLE
    // =========================================================
    protected override void Start()
    {
        base.Start();

        spriteRend = GetComponentInChildren<SpriteRenderer>();
        if (spriteRend != null) originalColor = spriteRend.color;

        // Stagger cooldowns so boss doesn't do everything at once on start
        shootTimer    = 1f;
        shoot360Timer = shoot360Cooldown * 0.6f;
        meleeTimer    = 0f;
        dashTimer     = dashCooldown * 0.4f;
        summonTimer   = summonCooldown * 0.5f;
    }

    protected override void Update()
    {
        base.Update();          // health-regen + healthbar follow

        if (player == null) return;
        if (currentHealth <= 0) return;

        // ── Arena activation ──────────────────────────────────
        if (arenaArea != null)
        {
            bool inArena = arenaArea.bounds.Contains(player.position);
            if (!bossActivated)
            {
                if (!inArena) return;
                bossActivated = true;
                Debug.Log("[BOSS] Kích hoạt!");
                StartCoroutine(BossEntrance());
            }
            isChasing = true;
        }
        else
        {
            float dist = Vector2.Distance(transform.position, player.position);
            if (!bossActivated && dist <= detectRange)
            {
                bossActivated = true;
                StartCoroutine(BossEntrance());
            }
            isChasing = bossActivated;
        }

        // ── Disable waypoint while chasing ───────────────────
        if (waypointMover != null) waypointMover.enabled = !isChasing;

        if (!isChasing) return;

        // ── Enrage check ──────────────────────────────────────
        CheckEnrage();
        UpdateEnrageColor();
        CleanupDeadSummons();

        // ── Reposition cooldown ───────────────────────────────
        repositionTimer -= Time.deltaTime;

        if (isDashing || isRepositioning || isPerformingAction) return;

        // ── Kiểm tra dịch vị khi player lên cao ──────────────
        if (repositionTimer <= 0f)
            CheckReposition();

        if (isRepositioning) return;

        // ── Cooldown tick ─────────────────────────────────────
        shootTimer    -= Time.deltaTime;
        shoot360Timer -= Time.deltaTime;
        meleeTimer    -= Time.deltaTime;
        dashTimer     -= Time.deltaTime;
        summonTimer   -= Time.deltaTime;

        ChooseAction();
    }

    // Override base FixedUpdate completely
    protected override void FixedUpdate()
    {
        if (player == null || currentHealth <= 0) return;

        if (isDashing)        { PerformDash(); return; }
        if (isRepositioning)  return;   // coroutine tự xử lý di chuyển
        if (!isChasing || isPerformingAction) return;

        float dist = Vector2.Distance(transform.position, player.position);
        if (dist > stopDistance) MoveToPlayer();
        else                     StopMoveAndFaceTarget();
    }

    // =========================================================
    // ENTRANCE
    // =========================================================
    private IEnumerator BossEntrance()
    {
        // Flash 3 times to signal boss appearance
        if (spriteRend != null)
        {
            for (int i = 0; i < 3; i++)
            {
                spriteRend.color = Color.red;
                yield return new WaitForSeconds(0.15f);
                spriteRend.color = originalColor;
                yield return new WaitForSeconds(0.15f);
            }
        }
    }

    // =========================================================
    // REPOSITION – Boss lên trên player rồi đẩy player xuống
    // =========================================================
    private void CheckReposition()
    {
        if (player == null) return;
        // Chỉ kích hoạt khi player đứng cao hơn boss đủ ngưỡng
        if (player.position.y > transform.position.y + repositionYThreshold)
            StartCoroutine(DoReposition());
    }

    private IEnumerator DoReposition()
    {
        isRepositioning    = true;
        isPerformingAction = true;
        rb.velocity        = Vector2.zero;

        // 1. Cảnh báo (flash tím)
        if (spriteRend != null)
        {
            Color warnCol = new Color(0.6f, 0f, 1f, 1f);
            for (int i = 0; i < 2; i++)
            {
                spriteRend.color = warnCol;
                yield return new WaitForSeconds(0.15f);
                spriteRend.color = isEnraged ? enrageColor : originalColor;
                yield return new WaitForSeconds(0.15f);
            }
        }

        // 2. Tính vị trí đích: ngay phía TRÊN player
        Vector2 target = ClampToArena(
            new Vector2(player.position.x, player.position.y + repositionAboveOffset));

        // 3. Di chuyển nhanh lên trên, liên tục cập nhật đích
        UpdateAnimator(Vector2.up, true);
        while (Vector2.Distance(rb.position, target) > 0.15f)
        {
            Vector2 newPos = Vector2.MoveTowards(
                rb.position, target, repositionSpeed * Time.fixedDeltaTime);
            rb.MovePosition(ClampToArena(newPos));
            yield return new WaitForFixedUpdate();

            // Cập nhật lại đích nếu player đang di chuyển
            target = ClampToArena(
                new Vector2(player.position.x, player.position.y + repositionAboveOffset));
        }

        rb.velocity = Vector2.zero;
        UpdateAnimator(Vector2.down, false);

        // 4. Đứng yên 0.1s rồi đẩy player xuống dưới
        yield return new WaitForSeconds(0.1f);

        if (player != null)
        {
            // Gây sát thương nhẹ + knockback mạnh xuống dưới
            var pc = player.GetComponent<PlayerController>();
            if (pc != null)
            {
                int pushDmg = Mathf.RoundToInt(meleeDamage * 0.5f * dmgMult);
                pc.TakeHit(pushDmg, Vector2.down, DameType.TypeDamage.Monster);
            }

            // Đẩy thêm bằng velocity (nếu player có Rigidbody2D)
            var playerRb = player.GetComponent<Rigidbody2D>();
            if (playerRb != null)
                playerRb.AddForce(Vector2.down * repositionKnockdownForce, ForceMode2D.Impulse);
        }

        // 5. Reset
        repositionTimer    = repositionCooldown;
        isRepositioning    = false;
        isPerformingAction = false;
        currentState       = BossState.Idle;
    }

    // =========================================================
    // AI: CHOOSE ACTION
    // =========================================================
    private void ChooseAction()
    {
        float dist = Vector2.Distance(transform.position, player.position);

        // Priority 1: Melee when very close
        float meleeTrigger = stopDistance + 0.6f;
        if (dist <= meleeTrigger && meleeTimer <= 0f)
        {
            currentState = BossState.MeleeAttack;
            StartCoroutine(DoMeleeAttack());
            return;
        }

        // Priority 2: Shoot burst at player
        if (shootTimer <= 0f && dist > meleeTrigger)
        {
            currentState = BossState.ShootAtPlayer;
            StartCoroutine(DoShootAtPlayer());
            return;
        }

        // Priority 3: Shoot 360
        if (shoot360Timer <= 0f)
        {
            currentState = BossState.Shoot360;
            StartCoroutine(DoShoot360());
            return;
        }

        // Priority 4: Dash toward player
        if (dist >= minDashDistance && dashTimer <= 0f)
        {
            currentState = BossState.DashToPlayer;
            StartDash();
            return;
        }

        // Priority 5: Summon (enrage only)
        if (isEnraged && summonTimer <= 0f && GetAliveSummonCount() < maxSummonCount)
        {
            currentState = BossState.Summon;
            StartCoroutine(DoSummon());
            return;
        }

        currentState = BossState.Chase;
    }

    // =========================================================
    // CHIÊU 1: BẮN BURST
    // =========================================================
    private IEnumerator DoShootAtPlayer()
    {
        isPerformingAction = true;
        rb.velocity = Vector2.zero;

        // Warning flash
        yield return StartCoroutine(WarnFlash(0.2f, 1));

        if (animator != null) animator.SetTrigger("Shoot");

        for (int i = 0; i < burstCount; i++)
        {
            if (player == null) break;
            Vector2 dir = ((Vector2)player.position - (Vector2)transform.position).normalized;
            SpawnProjectile(dir);
            if (i < burstCount - 1) yield return new WaitForSeconds(burstInterval);
        }

        shootTimer = shootCooldown * cooldownMult;
        yield return new WaitForSeconds(0.25f);

        isPerformingAction = false;
        currentState = BossState.Idle;
    }

    // =========================================================
    // CHIÊU 2: BẮN 360°
    // =========================================================
    private IEnumerator DoShoot360()
    {
        isPerformingAction = true;
        rb.velocity = Vector2.zero;

        // Warning – spin effect
        yield return StartCoroutine(WarnFlash(0.15f, 3));

        if (animator != null) animator.SetTrigger("Shoot");

        if (shoot360EffectPrefab != null)
            Destroy(Instantiate(shoot360EffectPrefab, transform.position, Quaternion.identity), 1.5f);

        yield return new WaitForSeconds(0.3f);

        float step = 360f / shoot360BulletCount;
        for (int i = 0; i < shoot360BulletCount; i++)
        {
            float angle = i * step * Mathf.Deg2Rad;
            Vector2 dir = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            SpawnProjectile(dir);
        }

        shoot360Timer = shoot360Cooldown * cooldownMult;
        yield return new WaitForSeconds(0.5f);

        isPerformingAction = false;
        currentState = BossState.Idle;
    }

    // =========================================================
    // CHIÊU 3: ĐÁNH GẦN
    // =========================================================
    private IEnumerator DoMeleeAttack()
    {
        isPerformingAction = true;
        rb.velocity = Vector2.zero;

        if (animator != null) animator.SetTrigger("Attack");

        // Brief lunge toward player
        if (player != null)
        {
            Vector2 lungeDir = ((Vector2)player.position - rb.position).normalized;
            rb.velocity = lungeDir * (speed * 3f);
        }
        yield return new WaitForSeconds(0.12f);
        rb.velocity = Vector2.zero;

        // Damage check
        int finalDmg = Mathf.RoundToInt(meleeDamage * dmgMult);
        foreach (var hit in Physics2D.OverlapCircleAll(transform.position, meleeRange))
        {
            if (!hit.CompareTag("Player")) continue;
            var pc = hit.GetComponent<PlayerController>();
            if (pc != null)
            {
                Vector2 kb = (hit.transform.position - transform.position).normalized;
                pc.TakeHit(finalDmg, kb, DameType.TypeDamage.Monster);
            }
        }

        meleeTimer = meleeCooldown * cooldownMult;
        yield return new WaitForSeconds(0.3f);

        isPerformingAction = false;
        currentState = BossState.Idle;
    }

    // =========================================================
    // CHIÊU 4: DASH
    // =========================================================
    private void StartDash()
    {
        if (player == null) return;

        isDashing          = true;
        isPerformingAction = true;
        dashDir            = ((Vector2)player.position - rb.position).normalized;
        dashTimeLeft       = dashDuration;

        if (dashTrailPrefab != null)
        {
            var trail = Instantiate(dashTrailPrefab, transform.position, Quaternion.identity);
            trail.transform.SetParent(transform);
            Destroy(trail, dashDuration + 0.5f);
        }

        if (animator != null) animator.SetBool("isMoving", true);

        dashTimer = (dashCooldown + Random.Range(0f, dashRandomExtra)) * cooldownMult;
    }

    private void PerformDash()
    {
        dashTimeLeft -= Time.fixedDeltaTime;
        if (dashTimeLeft <= 0f)
        {
            isDashing          = false;
            isPerformingAction = false;
            rb.velocity        = Vector2.zero;
            currentState       = BossState.Idle;
            if (animator != null) animator.SetBool("isMoving", false);
            return;
        }

        float spd    = dashSpeed * speedMult;
        Vector2 newPos = ClampToArena(rb.position + dashDir * spd * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        // Damage player on dash contact
        float contactDist = Vector2.Distance(transform.position, player.position);
        if (contactDist <= stopDistance + 0.3f)
        {
            var pc = player.GetComponent<PlayerController>();
            if (pc != null)
                pc.TakeHit(Mathf.RoundToInt(meleeDamage * dmgMult), dashDir, DameType.TypeDamage.Monster);
            // Stop dash early on hit
            isDashing          = false;
            isPerformingAction = false;
            rb.velocity        = Vector2.zero;
            currentState       = BossState.Idle;
        }
    }

    // =========================================================
    // CHIÊU 5: TRIỆU HỒI
    // =========================================================
    private IEnumerator DoSummon()
    {
        isPerformingAction = true;
        rb.velocity = Vector2.zero;

        yield return StartCoroutine(WarnFlash(0.2f, 2));

        if (summonEffectPrefab != null)
            Destroy(Instantiate(summonEffectPrefab, transform.position, Quaternion.identity), 1.5f);

        yield return new WaitForSeconds(0.4f);

        int alive    = GetAliveSummonCount();
        int canSpawn = Mathf.Min(summonPerWave, maxSummonCount - alive);

        for (int i = 0; i < canSpawn; i++)
        {
            if (summonPrefabs == null || summonPrefabs.Length == 0) break;
            var prefab = summonPrefabs[Random.Range(0, summonPrefabs.Length)];
            if (prefab == null) continue;

            Vector2 offset   = Random.insideUnitCircle.normalized * summonRadius;
            Vector3 spawnPos = transform.position + (Vector3)offset;
            activeSummons.Add(Instantiate(prefab, spawnPos, Quaternion.identity));

            if (summonEffectPrefab != null)
                Destroy(Instantiate(summonEffectPrefab, spawnPos, Quaternion.identity), 1f);
        }

        summonTimer = summonCooldown * cooldownMult;
        yield return new WaitForSeconds(0.3f);

        isPerformingAction = false;
        currentState = BossState.Idle;
    }

    // =========================================================
    // WARNING FLASH – nhấp nháy trước chiêu lớn
    // =========================================================
    private IEnumerator WarnFlash(float halfDuration, int times)
    {
        if (spriteRend == null) yield break;
        Color warnColor = new Color(1f, 0.8f, 0f, 1f); // vàng
        for (int i = 0; i < times; i++)
        {
            spriteRend.color = warnColor;
            yield return new WaitForSeconds(halfDuration);
            spriteRend.color = isEnraged ? enrageColor : originalColor;
            yield return new WaitForSeconds(halfDuration);
        }
    }

    // =========================================================
    // ENRAGE
    // =========================================================
    private void CheckEnrage()
    {
        if (isEnraged) return;
        if (currentHealth <= maxHealth * 0.5f) ActivateEnrage();
    }

    private void ActivateEnrage()
    {
        isEnraged      = true;
        dmgMult        = 2f;
        speedMult      = 1.5f;
        cooldownMult   = 0.6f;
        speed         *= 1.5f;
        dame          *= 2;
        bulletDamage  *= 2;
        meleeDamage   *= 2;

        // Reset some cooldowns immediately to attack right away
        shootTimer    = 0f;
        dashTimer     = 0f;

        Debug.Log("[BOSS] ENRAGE! Chỉ số x2, cooldown x0.6!");
        StartCoroutine(EnrageEffect());
    }

    private IEnumerator EnrageEffect()
    {
        // 5 flashes on enrage
        for (int i = 0; i < 5; i++)
        {
            if (spriteRend != null) spriteRend.color = Color.white;
            yield return new WaitForSeconds(0.08f);
            if (spriteRend != null) spriteRend.color = enrageColor;
            yield return new WaitForSeconds(0.08f);
        }
    }

    private void UpdateEnrageColor()
    {
        if (spriteRend == null) return;
        Color target = isEnraged ? enrageColor : originalColor;
        spriteRend.color = Color.Lerp(spriteRend.color, target, Time.deltaTime * colorTransitionSpeed);
    }

    // =========================================================
    // ANIMATION – 4 hướng hoặc 1 hướng + flip
    // =========================================================
    protected override void UpdateAnimator(Vector2 direction, bool isMoving)
    {
        if (animator == null) return;
        animator.SetBool("isMoving", isMoving);

        if (!isMoving) return;

        if (use4DirectionAnim)
        {
            // Truyền vector hướng chuẩn cho Blend Tree 2D
            animator.SetFloat("lastInputX", direction.x);
            animator.SetFloat("lastInputY", direction.y);
        }
        else
        {
            // Chỉ dùng 1 animation dọc → flip sprite theo trục X
            animator.SetFloat("lastInputX", 0f);
            animator.SetFloat("lastInputY", direction.y >= 0 ? 1f : -1f);

            if (spriteRend != null)
                spriteRend.flipX = direction.x < 0f;
        }
    }

    protected override void MoveToPlayer()
    {
        if (player == null) return;
        Vector2 rawDir  = ((Vector2)player.position - rb.position).normalized;
        Vector2 moveDir = GetAvoidanceDirection(rawDir);

        if (moveDir.sqrMagnitude > 0.001f)
        {
            Vector2 newPos = ClampToArena(rb.position + moveDir * speed * speedMult * Time.fixedDeltaTime);
            rb.MovePosition(newPos);
            UpdateAnimator(moveDir, true);
        }
        else
        {
            UpdateAnimator(rawDir, false);
        }
    }

    protected override void StopMoveAndFaceTarget()
    {
        rb.velocity = Vector2.zero;
        if (player != null)
        {
            Vector2 dir = ((Vector2)player.position - rb.position).normalized;
            UpdateAnimator(dir, false);
        }
    }

    // =========================================================
    // HEALTH / DEATH
    // =========================================================
    public override void ChangeHealth(int amount, DameType.TypeDamage type)
    {
        if (amount < 0 && type != DameType.TypeDamage.Heal)
        {
            lastDamageTime = Time.time;
            regenTimer     = 0f;
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);

        if (spawnedHealthBar != null)
            spawnedHealthBar.UpdateHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            if (animator != null) animator.SetBool("isDead", true);
            StopAllCoroutines();
            isDashing          = false;
            isPerformingAction = false;
            Die();
        }
    }

    protected override void Die()
    {
        foreach (var s in activeSummons)
            if (s != null) Destroy(s);
        activeSummons.Clear();

        if (spawnedHealthBarObj != null)
            Destroy(spawnedHealthBarObj);

        if (enemyID != 0) QuestController.instance?.ReportEnemyKilled(enemyID);
        GameStats.Instance?.AddKill();

        Debug.Log("[BOSS] Boss đã bị tiêu diệt!");
        Destroy(gameObject, 1f);
    }

    // =========================================================
    // HELPERS
    // =========================================================
    private void SpawnProjectile(Vector2 direction)
    {
        if (projectilePrefab == null) return;

        Vector2 spawnPos = firePoint != null
            ? (Vector2)firePoint.position
            : (Vector2)transform.position + direction.normalized * firePointOffset;

        var bullet = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
        var proj   = bullet.GetComponent<EnemyProjecttile>();
        if (proj != null)
        {
            proj.damage = Mathf.RoundToInt(bulletDamage * dmgMult);
            proj.LaunchProjectile(direction, bulletSpeed);
        }
    }

    private Vector2 ClampToArena(Vector2 pos)
    {
        if (arenaArea == null) return pos;
        Bounds b = arenaArea.bounds;
        pos.x = Mathf.Clamp(pos.x, b.min.x, b.max.x);
        pos.y = Mathf.Clamp(pos.y, b.min.y, b.max.y);
        return pos;
    }

    private void CleanupDeadSummons() => activeSummons.RemoveAll(s => s == null);
    private int  GetAliveSummonCount() { CleanupDeadSummons(); return activeSummons.Count; }

    // =========================================================
    // GIZMOS
    // =========================================================
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, minDashDistance);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, summonRadius);
        if (arenaArea != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(arenaArea.bounds.center, arenaArea.bounds.size);
        }
    }
}
