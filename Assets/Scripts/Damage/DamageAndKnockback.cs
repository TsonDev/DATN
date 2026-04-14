using UnityEngine;

public class DamageAndKnockback : MonoBehaviour
{
    [Header("Cài đặt sát thương và Đẩy lùi")]
    public int damage = 1;
    public DameType.TypeDamage damageType = DameType.TypeDamage.Monster;

    // Chạm vào dạng Trigger (Xuyên qua)
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            ApplyDamageAndKnockback(collision.gameObject);
        }
    }

    // Chạm vào dạng Collision vật lý cứng (Nhội lại)
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            ApplyDamageAndKnockback(collision.gameObject);
        }
    }

    private void ApplyDamageAndKnockback(GameObject playerObj)
    {
        PlayerController playerController = playerObj.GetComponent<PlayerController>();
        
        if (playerController != null)
        {
            // Tính toán hướng đẩy người chơi bật ra xa khỏi vật thể này
            Vector2 hitDirection = (playerObj.transform.position - transform.position).normalized;
            
            // Gọi hàm TakeHit mặc định của người chơi để vừa ăn damage vừa văng ra
            playerController.TakeHit(damage, hitDirection, damageType);
        }
    }
}
