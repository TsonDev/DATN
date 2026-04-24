using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkillAoe : MonoBehaviour
{
    [SerializeField] GameObject auraObject;
    [SerializeField] float auraDuration = 3f;
    [SerializeField] GameObject player;
    
    [Header("Energy Cost")]
    public float energyCost = 30f;

    [Header("Damage Settings")]
    [SerializeField] int damage = 2;
    [SerializeField] float damageRate = 0.5f;
    [SerializeField] float radius = 3f;

    float timer;
    float damageTimer;
    bool auraActive;

    void Update()
    {
        if (!auraActive) return;

        auraObject.transform.position = player.transform.position;

        damageTimer -= Time.deltaTime;
        if (damageTimer <= 0f)
        {
            damageTimer = damageRate;
            DealDamage();
        }

        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            auraObject.SetActive(false);
            auraActive = false;
        }
    }

    public void ActivateAura()
    {
        auraObject.transform.position = player.transform.position;
        auraObject.SetActive(true);
        timer = auraDuration;
        damageTimer = 0f; // Deal damage immediately
        auraActive = true;
    }

    void DealDamage()
    {
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(player.transform.position, radius);
        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.CompareTag("Enemy"))
            {
                EnemyBase enemyBase = enemy.GetComponent<EnemyBase>();
                if (enemyBase != null)
                {
                    enemyBase.ChangeHealth(-damage, DameType.TypeDamage.Skill);
                }
                EnemyShoot enemyShoot = enemy.GetComponent<EnemyShoot>();
                if (enemyShoot != null)
                {
                    enemyShoot.ChangeHealth(-damage, DameType.TypeDamage.Skill);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(player.transform.position, radius);
        }
    }
}
