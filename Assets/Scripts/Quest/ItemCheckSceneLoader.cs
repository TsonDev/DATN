using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Đặt component này lên GameObject có Collider2D (IsTrigger = true).
/// Khi Player bước vào, nó kiểm tra xem inventory có đủ số lượng item yêu cầu không.
///   - Nếu ĐỦ  → load scene đã chỉ định.
///   - Nếu KHÔNG đủ → hiện thông báo (tuỳ chọn) và không làm gì.
///
/// Cách dùng (không cần Quest):
///   1. Tạo GameObject → thêm Collider2D → tick IsTrigger.
///   2. Gắn ItemCheckSceneLoader vào GameObject đó.
///   3. Điền requiredItemID, requiredAmount, sceneToLoad.
///   4. (Tuỳ chọn) Gán notEnoughText để hiện thông báo khi thiếu item.
/// </summary>
public class ItemCheckSceneLoader : MonoBehaviour
{
    [Header("Item Requirement")]
    [Tooltip("ID của item cần kiểm tra (khớp với Item.ID trong Inventory).")]
    public int requiredItemID;

    [Tooltip("Số lượng tối thiểu cần có trong inventory.")]
    public int requiredAmount = 1;

    [Header("Scene To Load")]
    [Tooltip("Tên scene sẽ được load khi đủ điều kiện. Phải có trong Build Settings.")]
    public string sceneToLoad;

    [Header("Optional UI Feedback")]
    [Tooltip("(Tuỳ chọn) GameObject chứa Text thông báo khi không đủ item.")]
    public GameObject notEnoughMessageObject;

    [Tooltip("Thời gian hiển thị thông báo (giây).")]
    public float messageDisplayTime = 2f;

    [Header("Behaviour")]
    [Tooltip("Nếu bật, chỉ kiểm tra 1 lần duy nhất khi Player bước vào.")]
    public bool triggerOnce = true;

    [Tooltip("Bật để hiển thị log debug.")]
    public bool showDebugLog = true;

    // ─── Private state ────────────────────────────────────────────────────────
    private bool hasTriggered = false;
    private Coroutine hideMessageCoroutine;

    // ─── Trigger ──────────────────────────────────────────────────────────────
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggerOnce && hasTriggered) return;

        hasTriggered = true;

        // Kiểm tra InventoryController tồn tại
        if (InventoryController.Intance == null)
        {
            Debug.LogWarning("[ItemCheckSceneLoader] Không tìm thấy InventoryController!");
            return;
        }

        var itemCounts = InventoryController.Intance.GetItemCounts();
        int have = itemCounts.TryGetValue(requiredItemID, out int c) ? c : 0;

        if (showDebugLog)
            Debug.Log($"[ItemCheckSceneLoader] itemID={requiredItemID}: có {have}/{requiredAmount}");

        if (have >= requiredAmount)
        {
            // ✅ Đủ item → load scene
            if (showDebugLog)
                Debug.Log($"[ItemCheckSceneLoader] ✅ Đủ điều kiện! Load scene: \"{sceneToLoad}\"");

            if (string.IsNullOrEmpty(sceneToLoad))
            {
                Debug.LogError("[ItemCheckSceneLoader] sceneToLoad chưa được điền!");
                return;
            }

            SceneManager.LoadScene(sceneToLoad);
        }
        else
        {
            // ❌ Không đủ → hiện thông báo, reset flag để cho phép thử lại
            hasTriggered = false;

            if (showDebugLog)
                Debug.Log($"[ItemCheckSceneLoader] ❌ Chưa đủ item. Cần thêm {requiredAmount - have}.");

            ShowNotEnoughMessage();
        }
    }

    // ─── UI Feedback ──────────────────────────────────────────────────────────
    private void ShowNotEnoughMessage()
    {
        if (notEnoughMessageObject == null) return;

        if (hideMessageCoroutine != null) StopCoroutine(hideMessageCoroutine);
        notEnoughMessageObject.SetActive(true);
        hideMessageCoroutine = StartCoroutine(HideAfterDelay(messageDisplayTime));
    }

    private System.Collections.IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (notEnoughMessageObject != null)
            notEnoughMessageObject.SetActive(false);
    }

    // ─── Gizmos ───────────────────────────────────────────────────────────────
    private void OnDrawGizmos()
    {
        Gizmos.color = hasTriggered
            ? new Color(0.5f, 0.5f, 0.5f, 0.3f)
            : new Color(0.9f, 0.7f, 0.1f, 0.3f); // Vàng = chờ trigger

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
            Gizmos.DrawSphere(transform.position, 0.5f);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.9f, 0.7f, 0.1f, 0.95f);
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
