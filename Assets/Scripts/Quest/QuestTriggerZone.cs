using UnityEngine;

/// <summary>
/// Đặt component này lên bất kỳ GameObject nào có Collider2D (IsTrigger = true).
/// Khi Player bước vào vùng này, nó sẽ tự động cộng progress cho Quest loại Custom.
///
/// Cách dùng:
/// 1. Tạo GameObject -> thêm Collider2D -> tick IsTrigger.
/// 2. Gắn QuestTriggerZone vào GameObject đó.
/// 3. Điền objectiveID khớp với objectiveID trong Quest ScriptableObject loại Custom.
/// 4. Tuỳ chọn: tick triggerOnce để chỉ kích hoạt 1 lần.
/// </summary>
public class QuestTriggerZone : MonoBehaviour
{
    [Header("Quest Settings")]
    [Tooltip("Phải khớp với objectiveID trong Quest ScriptableObject loại Custom.")]
    public int objectiveID;

    [Tooltip("Số progress tăng mỗi lần kích hoạt (thường để 1).")]
    public int progressAmount = 1;

    [Tooltip("Nếu bật, trigger chỉ hoạt động 1 lần duy nhất.")]
    public bool triggerOnce = true;

    [Header("Debug")]
    [Tooltip("Bật để hiển thị log khi trigger kích hoạt.")]
    public bool showDebugLog = true;

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Chỉ phản ứng với Player
        if (!other.CompareTag("Player")) return;

        // Kiểm tra đã chạy rồi thì bỏ qua (nếu triggerOnce = true)
        if (triggerOnce && hasTriggered) return;

        if (QuestController.instance == null)
        {
            Debug.LogWarning("[QuestTriggerZone] Không tìm thấy QuestController trong scene!");
            return;
        }

        QuestController.instance.ReportCustomProgress(objectiveID, progressAmount);
        hasTriggered = true;

        if (showDebugLog)
            Debug.Log($"[QuestTriggerZone] Kích hoạt objectiveID={objectiveID}, amount={progressAmount}");

        // Nếu chỉ dùng 1 lần thì có thể tự tắt Collider để tránh check tiếp
        if (triggerOnce)
        {
            var col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;
        }
    }

    // Vẽ vùng trigger trong Scene view để designer dễ nhìn
    private void OnDrawGizmos()
    {
        Gizmos.color = hasTriggered
            ? new Color(0.5f, 0.5f, 0.5f, 0.3f)   // Xám khi đã dùng
            : new Color(0.2f, 0.9f, 0.4f, 0.3f);   // Xanh lá khi còn hiệu lực

        var col = GetComponent<Collider2D>();
        if (col is BoxCollider2D box)
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.DrawCube(box.offset, box.size);
        }
        else if (col is CircleCollider2D circle)
        {
            Gizmos.DrawSphere((Vector3)circle.offset + transform.position, circle.radius * transform.lossyScale.x);
        }
        else
        {
            // Fallback: vẽ hình cầu nhỏ tại vị trí object
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Viền rõ hơn khi được chọn
        Gizmos.color = new Color(0.2f, 0.9f, 0.4f, 0.9f);
        var col = GetComponent<Collider2D>();
        if (col is BoxCollider2D box)
        {
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
            Gizmos.DrawWireCube(box.offset, box.size);
        }
        else if (col is CircleCollider2D circle)
        {
            Gizmos.DrawWireSphere((Vector3)circle.offset + transform.position, circle.radius * transform.lossyScale.x);
        }
    }
}
