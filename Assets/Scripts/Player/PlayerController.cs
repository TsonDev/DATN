using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using static DameType;

public class PlayerController : MonoBehaviour
{
    public int indexScene = 0;
    [SerializeField] public int maxHealth = 100;
    public int currentHealth;
    public float timeInvicible = 1f;

    [Header("Energy System")]
    public float maxEnergy = 100f;
    public float currentEnergy;
    public float energyRegenRate = 5f; // Số năng lượng tự hồi phục mỗi giây
    public ManaBarUI energyBarUI; 
    float timedameCoolDown;
    bool isNatureInvicible;
    bool isInvicible;
    private HealthBarUI healthBarUI;

    [SerializeField] float fireCooldown = 0.12f;
    float nextFireTime;
    public GameObject ProjectTilePrefab;
    SkillAoe skillAoe;
    Animator animator;
    PlayerMovement playerMovement;
    Rigidbody2D rig2d;
    Collider2D playerCol;

    [Header("MeleeWeapon")]
    public GameObject MeleePrefab;
    public bool useMelee = true;

    [Header("Melee Setup")]
    public Transform handTransform;

    [Header("Knockback")]
    [SerializeField] float knockbackDistance = 1.8f;
    [SerializeField] float knockbackDuration = 0.08f;

    bool isKnockback;
    Coroutine knockbackCoroutine;

    private MeleeWeapon lastSpawnedMelee;

    enum AttackType { None, Attack1, Attack2 }
    private AttackType _pendingAttack = AttackType.None;
    private Vector2 _pendingAttackDirection = Vector2.right;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        rig2d = GetComponent<Rigidbody2D>();
        playerCol = GetComponent<Collider2D>();
        skillAoe = GetComponent<SkillAoe>();
        currentHealth = maxHealth;
        
        // Tìm toàn bộ HealthBarUI trên scene. 
        // Lấy cái nào KHÔNG phải là con của các enemy (thường nằm trực tiếp ở vòng ngoài UI tên là HealthBar)
        HealthBarUI[] allHealthBars = FindObjectsOfType<HealthBarUI>();
        foreach (var hb in allHealthBars)
        {
            // HealthBar của người chơi thường nằm trong UI Canvas chính, ko nằm trong quái
            if (hb.transform.parent == null || hb.transform.root.GetComponent<EnemyBase>() == null)
            {
                healthBarUI = hb;
                break;
            }
        }

        currentEnergy = maxEnergy;
        if (energyBarUI != null)
        {
            energyBarUI.UpdateMana(currentEnergy, maxEnergy);
        }
      
    }

    void Update()
    {
        if (isInvicible)
        {
            timedameCoolDown -= Time.deltaTime;
            if (timedameCoolDown <= 0f)
            {
                isInvicible = false;
            }
        }

        // Tự động hồi năng lượng
        if (currentEnergy < maxEnergy)
        {
            currentEnergy += energyRegenRate * Time.deltaTime;
            currentEnergy = Mathf.Clamp(currentEnergy, 0f, maxEnergy);
            if (energyBarUI != null)
            {
                energyBarUI.UpdateMana(currentEnergy, maxEnergy);
            }
        }
    }

    public bool TryConsumeEnergy(float amount)
    {
        if (currentEnergy >= amount)
        {
            currentEnergy -= amount;
            if (energyBarUI != null)
                energyBarUI.UpdateMana(currentEnergy, maxEnergy);
            return true;
        }
        return false;
    }

    public void ChangeHealth(int amount, DameType.TypeDamage type)
    {
        if (amount < 0)
        {
            if (isInvicible) return;
            if (type == DameType.TypeDamage.Snow && isNatureInvicible) return;

            isInvicible = true;
            timedameCoolDown = timeInvicible;
        }

        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        if (healthBarUI != null)
            healthBarUI.UpdateHealth(currentHealth, maxHealth);
        if(currentHealth <= 0)
        {
            Die();
        }
    }

    public void TakeHit(int damage, Vector2 hitDirection, DameType.TypeDamage type)
    {
        if (damage <= 0) return;
        if (isInvicible) return;

        ChangeHealth(-damage, type);

        if (playerMovement != null)
            playerMovement.moveInput = Vector2.zero;

        if (knockbackCoroutine != null)
            StopCoroutine(knockbackCoroutine);

        knockbackCoroutine = StartCoroutine(KnockbackRoutine(hitDirection.normalized));
    }
    public void Die()
    {
        // Handle player death (e.g., play animation, disable controls, etc.)
        StartCoroutine(LoadDelay(indexScene));
        Debug.Log("Player has died.");
    }
    IEnumerator LoadDelay(int indexScence)
    {
        yield return new WaitForSeconds(1.5f); //  thời gian load giả

        SceneManager.LoadScene(indexScence);
    }

    IEnumerator KnockbackRoutine(Vector2 dir)
    {
        isKnockback = true;

        if (playerCol != null)
            playerCol.enabled = false;

        if (rig2d != null)
            rig2d.velocity = Vector2.zero;

        float elapsed = 0f;
        Vector3 startPos = transform.position;
        Vector3 targetPos = startPos + (Vector3)(dir * knockbackDistance);

        while (elapsed < knockbackDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / knockbackDuration;

            transform.position = Vector3.Lerp(startPos, targetPos, t);
            yield return null;
        }

        transform.position = targetPos;

        if (rig2d != null)
            rig2d.velocity = Vector2.zero;

        if (playerCol != null)
            playerCol.enabled = true;

        isKnockback = false;
        knockbackCoroutine = null;
    }

    public bool IsKnockback()
    {
        return isKnockback;
    }

    public void ActivateNatureResistant(float time)
    {
        StartCoroutine(NatureResistantCoroutine(time));
    }

    IEnumerator NatureResistantCoroutine(float time)
    {
        isNatureInvicible = true;
        yield return new WaitForSeconds(time);
        isNatureInvicible = false;
    }

    public void ActivateInvincibleForDash(float time)
    {
        if (!isInvicible || timedameCoolDown < time)
        {
            isInvicible = true;
            timedameCoolDown = time;
        }
    }

    public void Attack1(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + fireCooldown;

        Vector2 dir = (playerMovement.moveInput.sqrMagnitude > 0.001f)
            ? playerMovement.moveInput.normalized
            : playerMovement.lastDir;

        _pendingAttack = AttackType.Attack1;
        _pendingAttackDirection = dir;

        animator.SetFloat("lastInputX", dir.x);
        animator.SetFloat("lastInputY", dir.y);
        animator.SetTrigger("Attack1");
    }

    public void Skill1(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (skillAoe != null)
        {
            if (TryConsumeEnergy(skillAoe.energyCost))
            {
                skillAoe.ActivateAura();
                animator.SetTrigger("Skill1");
            }
            else
            {
                Debug.Log("Không đủ năng lượng hoặc thể lực để dùng Skill 1!");
            }
        }
    }

    public void Attack2(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (Time.time < nextFireTime) return;
        nextFireTime = Time.time + fireCooldown;

        Vector2 dir = (playerMovement.moveInput.sqrMagnitude > 0.001f)
            ? playerMovement.moveInput.normalized
            : playerMovement.lastDir;

        _pendingAttack = AttackType.Attack2;
        _pendingAttackDirection = dir;

        animator.SetFloat("lastInputX", dir.x);
        animator.SetFloat("lastInputY", dir.y);
        animator.SetTrigger("Attack2");
    }
    public void Untilmate(InputAction.CallbackContext context)
    {
        //Use ultimate skill here
        FindObjectOfType<UltimateEffect>().PlayUltimate();
    }

    public void Animation_SpawnAttack()
    {
        if (_pendingAttack == AttackType.None) return;

        Vector2 dir = _pendingAttackDirection;

        if (_pendingAttack == AttackType.Attack1)
        {
            if (ProjectTilePrefab != null)
            {
                // Kiểm tra đạn trước khi bắn
                if (AmmoManager.Instance != null && !AmmoManager.Instance.TryConsumeAmmo())
                {
                    Debug.Log("Out of ammo!");
                    _pendingAttack = AttackType.None;
                    return;
                }
                var go = Instantiate(ProjectTilePrefab, rig2d.position + dir * 0.5f, Quaternion.identity);
                go.GetComponent<ProjectTile>()?.LunchProTile(dir, 300);
            }
            else
            {
                Debug.LogWarning("ProjectTilePrefab is null.");
            }
        }
        else if (_pendingAttack == AttackType.Attack2)
        {
            if (useMelee && MeleePrefab != null)
            {
                var go = Instantiate(MeleePrefab, rig2d.position, Quaternion.identity);
                var mw = go.GetComponent<MeleeWeapon>();

                if (mw != null)
                {
                    mw.Setup(dir, handTransform != null ? handTransform : transform);
                    lastSpawnedMelee = mw;
                }
                else
                {
                    Debug.LogWarning("MeleePrefab missing MeleeWeapon component.");
                    Destroy(go);
                }
            }
            else
            {
                if (ProjectTilePrefab != null)
                {
                    // Kiểm tra đạn trước khi bắn (fallback projectile)
                    if (AmmoManager.Instance != null && !AmmoManager.Instance.TryConsumeAmmo())
                    {
                        Debug.Log("Out of ammo!");
                        _pendingAttack = AttackType.None;
                        return;
                    }
                    var go = Instantiate(ProjectTilePrefab, rig2d.position + dir * 0.5f, Quaternion.identity);
                    go.GetComponent<ProjectTile>()?.LunchProTile(dir, 300);
                }
            }
        }

        _pendingAttack = AttackType.None;
    }

    public void Animation_EnableMeleeHit()
    {
        lastSpawnedMelee?.EnableHitbox();
    }

    public void Animation_DisableMeleeHit()
    {
        lastSpawnedMelee?.DisableHitbox();
    }

    public void Animation_EndMelee()
    {
        lastSpawnedMelee?.EndSwing();
        lastSpawnedMelee = null;
    }

    public void MovePlayer(InputAction.CallbackContext context)
    {
        if (isKnockback) return;
        playerMovement.Move(context);
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (playerMovement != null)
        {
            playerMovement.TryDash();
        }
    }
}