using UnityEngine;

/// <summary>
/// Item đạn — kế thừa từ Item.
/// Khi UseItem() được gọi (từ HotBar hoặc click), nạp đạn vào AmmoManager
/// và trừ số lượng trong inventory.
/// </summary>
public class AmmoItem : Item
{
    public override void UseItem()
    {
        if (AmmoManager.Instance == null)
        {
            Debug.LogWarning("AmmoManager not found in scene!");
            return;
        }

        // Nếu đạn đã đầy thì không nạp
        if (AmmoManager.Instance.GetCurrentAmmo() >= AmmoManager.Instance.GetMaxAmmo())
        {
            Debug.Log("Ammo is already full!");
            return;
        }

        // Nạp đạn vào AmmoManager
        int ammoToAdd = AmmoManager.Instance.GetAmmoPerItem();
        AmmoManager.Instance.AddAmmo(ammoToAdd);
        Debug.Log($"Loaded {ammoToAdd} ammo. Current: {AmmoManager.Instance.GetCurrentAmmo()}");

        // Trừ quantity trong inventory
        if (quantity > 1)
        {
            RemoveFromStack(1);
        }
        else
        {
            // Hết item → xóa khỏi slot
            Transform parentSlot = transform.parent;
            if (parentSlot != null)
            {
                Slot slot = parentSlot.GetComponent<Slot>();
                if (slot != null)
                {
                    slot.CurrentItem = null;
                }
            }
            Destroy(gameObject);
        }

        // Cập nhật inventory
        if (InventoryController.Intance != null)
        {
            InventoryController.Intance.RebuildItemCounts();
        }
    }
}
