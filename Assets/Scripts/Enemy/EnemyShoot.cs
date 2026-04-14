using UnityEngine;

public class EnemyShoot : MonoBehaviour
{
    [Header("Detect")]
    public float detectRange = 5f;

    [Header("Shoot")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireCooldown = 1.5f;

    [Header("AOE Attack")]
    public float aoeRange = 1.5f;
    public float aoeCooldown = 2f;
    public int aoeDamage = 2;
    public GameObject aoeEffectPrefab;
    [Header("Health")]
    public int maxHealth = 10;
    public int currentHealth;

    private float nextAOETime;

    private Transform player;
    private Animator animator;
    private WaypointMover waypointMover;

    private float nextFireTime;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        animator = GetComponent<Animator>();
        waypointMover = GetComponent<WaypointMover>();
        currentHealth = maxHealth;
        
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= detectRange)
        {
            if (waypointMover != null)
                waypointMover.enabled = false;

            animator?.SetBool("isMoving", false);

            Vector2 dir = (player.position - transform.position).normalized;
            animator?.SetFloat("lastInputX", dir.x);
            animator?.SetFloat("lastInputY", dir.y);

            // Ưu tiên AOE nếu ở gần
            if (dist <= aoeRange)
            {
                if (Time.time >= nextAOETime)
                {
                    DoAOE();
                    nextAOETime = Time.time + aoeCooldown;
                }
            }
            else
            {
                // Xa thì bắn
                if (Time.time >= nextFireTime)
                {
                    Shoot(dir);
                    nextFireTime = Time.time + fireCooldown;
                }
            }
        }
        else
        {
            // ngoài vùng → đi lại bằng waypoint
            if (waypointMover != null)
                waypointMover.enabled = true;
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
    void Shoot(Vector2 dir)
    {
        if (projectilePrefab == null) return;

        Vector2 spawnPos = firePoint != null ? firePoint.position : transform.position;

        GameObject bullet = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        // KHÔNG cần rigidbody cũng được
        var proj = bullet.GetComponent<EnemyProjecttile>();
        if (proj != null)
        {
            proj.LaunchProjectile(dir, 8f);
        }

        // animation
        animator?.SetTrigger("Shoot");
    }
    void DoAOE()
    {
        // animation
        animator?.SetTrigger("Attack"); // trigger Attack trong Animator
        if (aoeEffectPrefab != null)
        {
            GameObject fx = Instantiate(aoeEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 1f); // tự hủy sau 1s
        }
        // gây damage trong vùng
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aoeRange);

        foreach (var hit in hits)
        {
            PlayerController playerCtrl = hit.GetComponent<PlayerController>();
            if (playerCtrl != null)
            {
                playerCtrl.ChangeHealth(-aoeDamage, DameType.TypeDamage.Snow);
            }
        }
    }
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aoeRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}