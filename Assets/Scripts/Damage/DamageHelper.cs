using UnityEngine;

public static class DamageHelper
{
    /// <summary>
    /// Gọi hàm này từ bất kỳ Enemy nào để vừa gây sát thương, vừa tự động tính toán hướng và đẩy lùi người chơi.
    /// Ví dụ cách gọi: DamageHelper.DealDamageAndKnockback(transform, player, dame, DameType.TypeDamage.Monster);
    /// </summary>
    public static void DealDamageAndKnockback(Transform attackerTransform, Transform playerTransform, int damage, DameType.TypeDamage damageType)
    {
        if (playerTransform == null || attackerTransform == null) return;
        
        PlayerController playerController = playerTransform.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // Tự động tính toán hướng đẩy từ quái vật chĩa về phía người chơi
            Vector2 hitDirection = (playerTransform.position - attackerTransform.position).normalized;
            
            // Hàm TakeHit trong PlayerController sẽ nhận Direction và tự động đẩy người chơi lùi lại,
            // đồng thời hàm TakeHit cũng đã tự động trừ máu bên trong nó rồi.
            playerController.TakeHit(damage, hitDirection, damageType);
            playerController.ChangeHealth(-damage, DameType.TypeDamage.Monster);
        }
    }
}
