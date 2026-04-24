using UnityEngine;

public class EnemyShoot : EnemyBase
{
    [Header("Shoot")]
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float fireCooldown = 1.5f;

    [Header("AOE Attack")]
    public float aoeRange = 1.5f;
    public float aoeCooldown = 2f;
    public int aoeDamage = 2;
    public GameObject aoeEffectPrefab;

    private float nextAOETime;
    private float nextFireTime;

    protected override void Start()
    {
        base.Start(); // Chạy thanh máu và các logic bắt sóng Player từ EnemyBase
    }

    protected override void Update()
    {
        base.Update(); // Update thanh máu hồi phục và vị trí UI máu trên đỉnh đầu

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

    protected override void FixedUpdate()
    {
        // Ghi đè rỗng FixedUpdate để KHÔNG DÙNG chức năng chĩa mặt tự động áp sát của EnemyBase
        // Bởi vì EnemyShoot chỉ loanh quanh và bắn, không lao vào người chơi (melee)
    }

    void Shoot(Vector2 dir)
    {
        if (projectilePrefab == null) return;

        Vector2 spawnPos = firePoint != null ? firePoint.position : transform.position;

        GameObject bullet = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        var proj = bullet.GetComponent<EnemyProjecttile>();
        if (proj != null)
        {
            proj.LaunchProjectile(dir, 8f);
        }

        animator?.SetTrigger("Shoot");
    }

    void DoAOE()
    {
        animator?.SetTrigger("Attack"); 
        if (aoeEffectPrefab != null)
        {
            GameObject fx = Instantiate(aoeEffectPrefab, transform.position, Quaternion.identity);
            Destroy(fx, 1f); 
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, aoeRange);

        foreach (var hit in hits)
        {
            PlayerController playerCtrl = hit.GetComponent<PlayerController>();
            if (playerCtrl != null)
            {
                playerCtrl.TakeHit(aoeDamage, (hit.transform.position - transform.position).normalized, DameType.TypeDamage.Snow);
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