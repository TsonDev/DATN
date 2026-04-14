using UnityEngine;

public class EnemyMelee : EnemyBase
{
    [Header("Melee Settings")]
    public float attackRange = 1.0f;

    [Header("Weapon Visual")]
    [Tooltip("Prefab vũ khí (ví dụ như thanh kiếm) sẽ spawn ra khi đánh")]
    public GameObject weaponPrefab;
    
    [Tooltip("Điều chỉnh góc quay của vũ khí (nếu vũ khí bị ngược, hãy thử các số như 0, 90, 180, -90)")]
    public float weaponRotationOffset = 90f;
    
    [Tooltip("Vị trí tay cấm vũ khí (có thể để trống, sẽ lấy tâm của quái làm gốc)")]
    public Transform handTransform;
    
    protected override void Update()
    {
        base.Update();
        
        // Nếu có WaypointMover, tắt nó đi khi đang đuổi theo Player, bật lại khi ở ngoài tầm
        if (waypointMover != null)
        {
            waypointMover.enabled = !isChasing;
        }
    }
    
    protected override void Attack()
    {
        // Reset timer
        attackTimer = attackCooldown;

        // Chạy animation Attack
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // Hiện vũ khí giống người chơi
        if (weaponPrefab != null)
        {
            GameObject weaponObj = Instantiate(weaponPrefab, transform.position, Quaternion.identity);
            
            // Xoay vũ khí theo hướng nhìn cộng thêm góc lệch chỉnh sẵn
            float angle = Mathf.Atan2(lastLookDir.y, lastLookDir.x) * Mathf.Rad2Deg;
            weaponObj.transform.rotation = Quaternion.Euler(0, 0, angle + weaponRotationOffset);
            
            // Đặt vào vị trí tay hoặc vị trí quái
            Transform parentTransform = handTransform != null ? handTransform : transform;
            weaponObj.transform.SetParent(parentTransform);
            weaponObj.transform.localPosition = (Vector3)lastLookDir * 0.5f; // Đẩy ra xa một chút theo hướng nhìn
            
            // Nếu Prefab có script để tự gây sát thương (như MeeleWeapon), có thể loại bỏ đoạn sát thương bên dưới.
            // Nếu đây chỉ là vũ khí hình ảnh, nó sẽ tự hủy sau nửa giây
            Destroy(weaponObj, 0.3f); 
        }

        // Deal damage:
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        // Kiểm tra xem Player còn trong tầm đánh không trước khi chém
        // Tự động lấy max giữa attackRange và stopDistance để tránh trường hợp quái đánh trúng hình ảnh nhưng hụt damage do cài đặt sai.
        float effectiveAttackRange = Mathf.Max(attackRange, stopDistance + 0.1f);
        if (distanceToPlayer <= effectiveAttackRange)
        {
            Debug.Log("EnemyMelee hit dir = " + lastLookDir);

            // GỌI HÀM TỪ CLASS MỚI TẠO Ở ĐÂY ĐỂ ĐẨY LÙI NGƯỜI CHƠI
            DamageHelper.DealDamageAndKnockback(transform, player, dame, DameType.TypeDamage.Monster);
        }
    }
    
    // Debug để vẽ hitbox tầm đánh trên Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
