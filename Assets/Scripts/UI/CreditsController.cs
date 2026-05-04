using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Điều khiển hiệu ứng credits chạy từ dưới lên giống phim.
/// Gắn script này vào một GameObject trong End Scene.
/// </summary>
public class CreditsController : MonoBehaviour
{
    // ─── References ───────────────────────────────────────────────────────────
    [Header("Data")]
    [Tooltip("Kéo CreditsData ScriptableObject vào đây")]
    public CreditsData creditsData;

    [Header("UI References")]
    [Tooltip("ScrollRect chứa nội dung credits")]
    public ScrollRect scrollRect;

    [Tooltip("RectTransform của Content bên trong ScrollRect")]
    public RectTransform contentRect;

    [Tooltip("Prefab một dòng ROLE (in nghiêng, màu vàng nhạt)")]
    public GameObject roleLinePrefab;

    [Tooltip("Prefab một dòng NAME (in đậm, màu trắng)")]
    public GameObject nameLinePrefab;

    [Tooltip("Prefab cho Section Header (to, in đậm, màu vàng)")]
    public GameObject sectionHeaderPrefab;

    [Tooltip("Prefab dòng trống (spacer)")]
    public GameObject spacerPrefab;

    [Tooltip("Text hiển thị tiêu đề game lớn ở đầu")]
    public TMP_Text gameTitleText;

    [Tooltip("Overlay fade in/out (Image màu đen)")]
    public Image fadeOverlay;

    // ─── Settings ─────────────────────────────────────────────────────────────
    [Header("Tốc độ & Thời gian")]
    [Tooltip("Tốc độ cuộn (pixels/giây). Tăng để nhanh hơn.")]
    public float scrollSpeed = 80f;

    [Tooltip("Thời gian fade in lúc bắt đầu (giây)")]
    public float fadeInDuration = 1.5f;

    [Tooltip("Thời gian dừng trước khi bắt đầu cuộn (giây)")]
    public float delayBeforeScroll = 1f;

    [Tooltip("Thời gian fade out ở cuối (giây)")]
    public float fadeOutDuration = 2f;

    [Tooltip("Thời gian chờ sau khi credits xong trước khi gọi sự kiện kết thúc")]
    public float delayAfterEnd = 1f;

    [Header("Tùy chọn")]
    [Tooltip("Cho phép người chơi nhấn phím bất kỳ để tăng tốc (×3)")]
    public bool allowSpeedUp = true;

    [Tooltip("Sự kiện gọi khi credits kết thúc (ví dụ: về menu)")]
    public UnityEngine.Events.UnityEvent onCreditsFinished;

    // ─── Private ──────────────────────────────────────────────────────────────
    private bool _isScrolling = false;
    private bool _speedUp = false;
    private float _totalContentHeight;
    private float _viewportHeight;
    private Coroutine _scrollCoroutine;

    // ══════════════════════════════════════════════════════════════════════════
    private void Start()
    {
        if (creditsData == null)
        {
            Debug.LogError("[CreditsController] Chưa gán CreditsData!");
            return;
        }

        // Kiểm tra và cảnh báo các prefab bị thiếu
        ValidatePrefabs();

        // Ẩn fade overlay ban đầu → sẽ fade in
        if (fadeOverlay != null)
        {
            fadeOverlay.color = new Color(0, 0, 0, 1f);
        }

        BuildCreditsContent();
        _scrollCoroutine = StartCoroutine(RunCredits());
    }

    private void ValidatePrefabs()
    {
        if (roleLinePrefab == null)
            Debug.LogError("[CreditsController] ❌ roleLinePrefab chưa được gán! Kéo prefab dòng ROLE vào Inspector.");
        if (nameLinePrefab == null)
            Debug.LogWarning("[CreditsController] ⚠️ nameLinePrefab chưa được gán! Sẽ dùng roleLinePrefab thay thế để hiển thị tên.");
        if (sectionHeaderPrefab == null)
            Debug.LogWarning("[CreditsController] ⚠️ sectionHeaderPrefab chưa được gán! Tiêu đề section sẽ dùng roleLinePrefab thay thế.");
        if (spacerPrefab == null)
            Debug.LogWarning("[CreditsController] ⚠️ spacerPrefab chưa được gán! Khoảng cách giữa các dòng có thể bị sai.");
    }

    private void Update()
    {
        if (!allowSpeedUp) return;

        // Nhấn bất kỳ phím hoặc click để tăng tốc
        if (Input.anyKey)
            _speedUp = true;
        else
            _speedUp = false;
    }

    // ─── Xây dựng nội dung credits từ data ────────────────────────────────────
    private void BuildCreditsContent()
    {
        // Xoá content cũ nếu có
        foreach (Transform child in contentRect)
            Destroy(child.gameObject);

        // Tiêu đề game (nếu có text riêng ở đầu content)
        if (gameTitleText != null && creditsData.gameTitle != "")
            gameTitleText.text = creditsData.gameTitle;

        // Thêm spacer đầu để content bắt đầu bên dưới viewport
        AddSpacer(GetViewportHeight() + 60f);

        // Sinh các dòng từ data
        foreach (var entry in creditsData.entries)
        {
            // Extra spacing phía trên
            if (entry.extraSpacingAbove > 0)
                AddSpacer(entry.extraSpacingAbove);

            if (entry.isSectionHeader)
            {
                SpawnLine(sectionHeaderPrefab, entry.role);
            }
            else
            {
                if (!string.IsNullOrEmpty(entry.role))
                    SpawnLine(roleLinePrefab, entry.role);

                if (!string.IsNullOrEmpty(entry.names))
                {
                    // Nếu nameLinePrefab chưa gán → fallback sang roleLinePrefab
                    GameObject namePrefab = nameLinePrefab != null ? nameLinePrefab : roleLinePrefab;
                    SpawnLine(namePrefab, entry.names);
                }
            }

            // Spacer giữa các entry
            AddSpacer(24f);
        }

        // Closing message
        if (!string.IsNullOrEmpty(creditsData.closingMessage))
        {
            AddSpacer(80f);
            SpawnLine(sectionHeaderPrefab, creditsData.closingMessage);
        }

        // Spacer cuối để nội dung cuộn hết qua viewport
        AddSpacer(GetViewportHeight() + 60f);

        // Rebuild layout để Unity tính lại kích thước
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        Canvas.ForceUpdateCanvases();

        _totalContentHeight = contentRect.rect.height;

        // Đặt scroll về đầu (giá trị normalizedPosition 0 = dưới cùng với vertical scroll ngược)
        scrollRect.verticalNormalizedPosition = 0f;
    }

    private void SpawnLine(GameObject prefab, string text)
    {
        if (prefab == null)
        {
            Debug.LogError($"[CreditsController] ❌ Không thể hiển thị '{text}' vì prefab bị null. Kiểm tra các slot Prefab trong Inspector của CreditsController.");
            return;
        }
        var go = Instantiate(prefab, contentRect);
        var tmp = go.GetComponentInChildren<TMP_Text>();
        if (tmp != null)
        {
            tmp.text = text;
        }
        else
        {
            Debug.LogError($"[CreditsController] ❌ Prefab '{prefab.name}' không có TMP_Text bên trong! Kiểm tra lại cấu trúc prefab.");
        }
    }

    private void AddSpacer(float height)
    {
        if (spacerPrefab == null) return;
        var go = Instantiate(spacerPrefab, contentRect);
        var rt = go.GetComponent<RectTransform>();
        if (rt != null)
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, height);
        var le = go.GetComponent<LayoutElement>();
        if (le != null)
            le.minHeight = height;
    }

    private float GetViewportHeight()
    {
        if (scrollRect != null)
            return scrollRect.viewport.rect.height;
        return Screen.height;
    }

    // ─── Coroutine chính điều khiển toàn bộ luồng ────────────────────────────
    private IEnumerator RunCredits()
    {
        // 1. Fade in
        yield return StartCoroutine(Fade(1f, 0f, fadeInDuration));

        // 2. Delay trước khi cuộn
        yield return new WaitForSeconds(delayBeforeScroll);

        // 3. Cuộn credits
        _isScrolling = true;
        yield return StartCoroutine(ScrollCredits());
        _isScrolling = false;

        // 4. Delay sau khi xong
        yield return new WaitForSeconds(delayAfterEnd);

        // 5. Fade out
        yield return StartCoroutine(Fade(0f, 1f, fadeOutDuration));

        // 6. Gọi sự kiện kết thúc
        onCreditsFinished?.Invoke();
    }

    private IEnumerator ScrollCredits()
    {
        // Cuộn bằng cách di chuyển anchoredPosition của contentRect lên
        float startY = contentRect.anchoredPosition.y;

        // Tổng quãng đường cần cuộn = toàn bộ chiều cao content
        // ScrollRect normalizedPosition: 0 = bottom, 1 = top
        // Ta sẽ dùng anchoredPosition để kiểm soát tốt hơn

        // Reset về đầu content (position y = 0)
        contentRect.anchoredPosition = new Vector2(contentRect.anchoredPosition.x, 0f);

        float viewH = scrollRect.viewport.rect.height;
        float contentH = contentRect.rect.height;
        float maxScroll = contentH - viewH;
        if (maxScroll < 0) maxScroll = contentH;

        float currentY = 0f;

        while (currentY < maxScroll)
        {
            float speed = _speedUp ? scrollSpeed * 3f : scrollSpeed;
            currentY += speed * Time.deltaTime;
            currentY = Mathf.Clamp(currentY, 0, maxScroll);

            contentRect.anchoredPosition = new Vector2(
                contentRect.anchoredPosition.x,
                currentY
            );

            yield return null;
        }
    }

    // ─── Fade Overlay ──────────────────────────────────────────────────────────
    private IEnumerator Fade(float fromAlpha, float toAlpha, float duration)
    {
        if (fadeOverlay == null) yield break;

        float elapsed = 0f;
        Color c = fadeOverlay.color;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            c.a = Mathf.Lerp(fromAlpha, toAlpha, t);
            fadeOverlay.color = c;
            yield return null;
        }

        c.a = toAlpha;
        fadeOverlay.color = c;
    }

    // ─── Public API ───────────────────────────────────────────────────────────
    /// <summary>Dừng credits ngay lập tức</summary>
    public void SkipCredits()
    {
        if (_scrollCoroutine != null)
            StopCoroutine(_scrollCoroutine);

        StartCoroutine(SkipToEnd());
    }

    private IEnumerator SkipToEnd()
    {
        yield return StartCoroutine(Fade(fadeOverlay != null ? fadeOverlay.color.a : 0f, 1f, 0.5f));
        onCreditsFinished?.Invoke();
    }
}
