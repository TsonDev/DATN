using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;
using UnityEngine.InputSystem;
using static DameType;


public class PlayerController : MonoBehaviour
{
    [SerializeField] int maxHealth = 100;
    int currentHealth;
    public float timeInvicible = 1f;
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

    [Header("MeleeWeapon")]
    public GameObject MeleePrefab; // gán prefab vũ khí cận chiến trong Inspector
    public bool useMelee = true;

    [Header("Melee Setup")]
    public Transform handTransform;

    private MeleeWeapon lastSpawnedMelee;

    // Pending attack state to be executed by Animation Event
    enum AttackType { None, Attack1, Attack2 }
    private AttackType _pendingAttack = AttackType.None;
    private Vector2 _pendingAttackDirection = Vector2.right;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerMovement = GetComponent<PlayerMovement>();
        rig2d = GetComponent<Rigidbody2D>();
        skillAoe = GetComponent<SkillAoe>();
        currentHealth = maxHealth;
        healthBarUI = FindAnyObjectByType<HealthBarUI>();
    }

    void Update()
    {
        if (isInvicible)
        {
            timedameCoolDown -= Time.deltaTime;
            if (timedameCoolDown < 0)
            {
                isInvicible = false;
            }
        }
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
        healthBarUI.UpdateHealth(currentHealth, maxHealth);
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

    // Attack1: now only sets pending attack and triggers animation
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
        skillAoe.ActivateAura();
        animator.SetTrigger("Skill1");
    }

    // Attack2: only sets pending attack and triggers animation
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

    // Called by Animation Event at the frame where the attack should actually spawn
    // Add an Animation Event that calls "Animation_SpawnAttack" at the desired frame in the Attack clip.
    public void Animation_SpawnAttack()
    {
        if (_pendingAttack == AttackType.None) return;

        Vector2 dir = _pendingAttackDirection;
        if (_pendingAttack == AttackType.Attack1)
        {
            // spawn projectile
            if (ProjectTilePrefab != null)
            {
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
            // spawn melee
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
                // fallback to projectile if melee not configured
                if (ProjectTilePrefab != null)
                {
                    var go = Instantiate(ProjectTilePrefab, rig2d.position + dir * 0.5f, Quaternion.identity);
                    go.GetComponent<ProjectTile>()?.LunchProTile(dir, 300);
                }
            }
        }

        // clear pending so duplicate spawn won't happen
        _pendingAttack = AttackType.None;
    }

    // Animation Events to control melee hitbox (as before)
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
        playerMovement.Move(context);
    }
}
