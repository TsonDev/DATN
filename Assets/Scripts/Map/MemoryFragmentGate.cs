using UnityEngine;
public class MemoryFragmentGate : MonoBehaviour
{

    [Tooltip("ID của item 'mảnh kí ức' trong ItemDictionary.")]
    public int memoryFragmentItemID = 10;   // đổi thành ID item thực tế của bạn

    [Tooltip("Số mảnh kí ức cần thu thập để mở cửa.")]
    public int requiredAmount = 2;

   
    [Tooltip("Sprite hiện khi chưa đủ mảnh kí ức (cửa đóng).")]
    public Sprite closedSprite;

    [Tooltip("Sprite hiện khi đã đủ mảnh kí ức (cửa mở).")]
    public Sprite openedSprite;


    [Tooltip("GameObject điểm dịch chuyển. Sẽ được bật khi đủ điều kiện.")]
    public GameObject teleportPoint;

    // ─── Private ──────────────────────────────────────────────────────────────
    private SpriteRenderer spriteRenderer;
    private bool isUnlocked = false;

    // ─── Unity ────────────────────────────────────────────────────────────────
    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("[MemoryFragmentGate] Thiếu SpriteRenderer trên GameObject cánh cửa!");
        }
    }

    private void Start()
    {
        // Trạng thái ban đầu: chưa đủ điều kiện
        ApplyState(false);

        // Đăng ký lắng nghe khi inventory thay đổi
        if (InventoryController.Intance != null)
        {
            InventoryController.Intance.OnInventoryChanged += OnInventoryChanged;
            // Kiểm tra ngay lần đầu (ví dụ khi load game đã có đủ mảnh)
            OnInventoryChanged();
        }
        else
        {
            // InventoryController chưa sẵn sàng → thử lại ở frame sau
            Invoke(nameof(TrySubscribe), 0.5f);
        }
    }

    private void OnDestroy()
    {
        if (InventoryController.Intance != null)
            InventoryController.Intance.OnInventoryChanged -= OnInventoryChanged;
    }

    // ─── Logic ────────────────────────────────────────────────────────────────

    /// <summary>Thử đăng ký nếu InventoryController chưa khởi tạo kịp lúc Start.</summary>
    private void TrySubscribe()
    {
        if (InventoryController.Intance != null)
        {
            InventoryController.Intance.OnInventoryChanged += OnInventoryChanged;
            OnInventoryChanged();
        }
        else
        {
            Debug.LogWarning("[MemoryFragmentGate] Không tìm thấy InventoryController!");
        }
    }

    /// <summary>Được gọi mỗi khi inventory thay đổi.</summary>
    private void OnInventoryChanged()
    {
        var counts = InventoryController.Intance.GetItemCounts();
        int have = counts.TryGetValue(memoryFragmentItemID, out int c) ? c : 0;

        bool shouldUnlock = have >= requiredAmount;

        if (shouldUnlock != isUnlocked)
        {
            isUnlocked = shouldUnlock;
            ApplyState(isUnlocked);

            Debug.Log(isUnlocked
                ? $"[MemoryFragmentGate]  Đủ {requiredAmount} mảnh kí ức → Mở cửa & kích hoạt điểm dịch chuyển!"
                : $"[MemoryFragmentGate]  Chỉ có {have}/{requiredAmount} mảnh → Cửa đóng.");
        }
    }

    /// <summary>Áp dụng trạng thái (mở/đóng) lên sprite và điểm dịch chuyển.</summary>
    private void ApplyState(bool unlocked)
    {
        // --- Sprite cánh cửa ---
        if (spriteRenderer != null)
        {
            Sprite target = unlocked ? openedSprite : closedSprite;
            if (target != null)
                spriteRenderer.sprite = target;
        }

        // --- Điểm dịch chuyển ---
        if (teleportPoint != null)
            teleportPoint.SetActive(unlocked);
    }

    // ─── Gizmos (hỗ trợ debug trong Editor) ──────────────────────────────────
    private void OnDrawGizmosSelected()
    {
        if (teleportPoint == null) return;
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(transform.position, teleportPoint.transform.position);
        Gizmos.DrawWireSphere(teleportPoint.transform.position, 0.4f);
    }
}
