using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DropItem
{
    [Tooltip("Prefab của vật phẩm muốn rớt ra")]
    public GameObject itemPrefab;
    
    [Tooltip("Tỉ lệ rớt ra vật phẩm này (0% đến 100%)")]
    [Range(0f, 100f)]
    public float dropChance = 50f;
}

public class LootDrop : MonoBehaviour
{
    [Header("Loot Settings")]
    [Tooltip("Danh sách các vật phẩm có thể rớt kèm tỉ lệ")]
    public List<DropItem> droppableItems;

    [Tooltip("Có cho phép rớt nhiều vật phẩm cùng lúc không? Nếu tắt, nó chỉ bốc thăm rớt tối đa 1 món đầu tiên trúng tỉ lệ.")]
    public bool dropMultipleItems = true;

    [Header("Spawn Position")]
    [Tooltip("Khoảng cách tối đa vật phẩm nảy ra xung quanh vị trí vật thể bị hủy")]
    public float scatterRange = 0.5f;

    /// <summary>
    /// Hàm này được gọi tự động khi GameObject bị xóa khỏi Scene (Destroy)
    /// </summary>
    private void OnDestroy()
    {
        // Chống lỗi khi game đang tắt (quit) hoặc load scene mới mà xóa object
        if (!gameObject.scene.isLoaded) return;

        DropLoot();
    }

    /// <summary>
    /// Hàm xử lý logic rớt đồ ngẫu nhiên
    /// </summary>
    public void DropLoot()
    {
        if (droppableItems == null || droppableItems.Count == 0) return;

        foreach (DropItem drop in droppableItems)
        {
            if (drop.itemPrefab == null) continue;
            float randomValue = Random.Range(0f, 100f);
            if (randomValue <= drop.dropChance)
            {
                // Tạo một vị trí nảy đồ ra bán kính xung quanh
                Vector2 dropPos = (Vector2)transform.position + Random.insideUnitCircle * scatterRange;

                // Triệu hồi vật phẩm
                GameObject spawnedItem = Instantiate(drop.itemPrefab, dropPos, Quaternion.identity);

                // Nếu vật phẩm có sẵn Script BounceEffect (Nảy tưng tưng), hãy gọi nó
                BounceEffect bounce = spawnedItem.GetComponent<BounceEffect>();
                if (bounce != null)
                {
                    bounce.StartBounce();
                }
                if (!dropMultipleItems)
                {
                    break;
                }
            }
        }
    }
}
