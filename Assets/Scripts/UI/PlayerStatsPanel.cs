using System.Collections;
using System.IO;
using UnityEngine;
using TMPro;

/// <summary>
/// Hiển thị bảng thống kê người chơi ở bên phải End Scene.
/// Mỗi con số có hiệu ứng đếm lên từ 0 → giá trị thực (count-up animation).
/// Gắn script này vào Panel thống kê bên phải.
/// </summary>
public class PlayerStatsPanel : MonoBehaviour
{
    [Header("UI Text References")]
    [Tooltip("Text hiển thị số quái đã giết")]
    public TMP_Text enemiesKilledText;

    [Tooltip("Text hiển thị số quest đã hoàn thành")]
    public TMP_Text questsCompletedText;

    [Tooltip("Text hiển thị gold kiếm được")]
    public TMP_Text goldEarnedText;

    [Tooltip("Text hiển thị thời gian chơi")]
    public TMP_Text timePlayedText;

    [Header("Tiêu đề bảng thống kê")]
    [Tooltip("Text tiêu đề của bảng (ví dụ: 'HÀNH TRÌNH CỦA BẠN')")]
    public TMP_Text panelTitleText;
    public string panelTitle = "⚔ HÀNH TRÌNH CỦA BẠN";

    [Header("Hiệu ứng Count-Up")]
    [Tooltip("Thời gian để một con số đếm từ 0 lên giá trị thực (giây)")]
    public float countUpDuration = 2f;

    [Tooltip("Delay trước khi bắt đầu hiệu ứng đếm (giây) - nên chờ credits hiện ra")]
    public float startDelay = 1.5f;

    [Header("Format")]
    public string killsLabel    = "💀 Kẻ địch đã tiêu diệt";
    public string questsLabel   = "📋 Quest hoàn thành";
    public string goldLabel     = "💰 Gold đã kiếm được";
    public string timeLabel     = "⏱ Thời gian chơi";

    // ─────────────────────────────────────────────────────────────────────────
    private void Start()
    {
        // Đặt tiêu đề
        if (panelTitleText != null)
            panelTitleText.text = panelTitle;

        // Reset hiển thị về 0 ban đầu
        SetAllToZero();

        // Bắt đầu hiệu ứng đếm sau delay
        StartCoroutine(DelayThenCountUp());
    }

    private void SetAllToZero()
    {
        SetText(enemiesKilledText,   killsLabel,  "0");
        SetText(questsCompletedText, questsLabel, "0");
        SetText(goldEarnedText,      goldLabel,   "0");
        SetText(timePlayedText,      timeLabel,   "00:00");
    }

    private IEnumerator DelayThenCountUp()
    {
        yield return new WaitForSeconds(startDelay);

        // ưu tiên dùng GameStats singleton (chạy từ WorldScene)
        GameStats stats = GameStats.Instance;

        // Fallback: nếu chạy EndScene riêng (không qua WorldScene) → đọc từ file save
        GameStatsSaveData data = null;
        if (stats != null)
        {
            data = stats.ToSaveData();
        }
        else
        {
            data = LoadStatsFromSaveFile();
        }

        if (data == null)
        {
            Debug.LogWarning("[PlayerStatsPanel] Không có dữ liệu thống kê. Chạy game từ WorldScene để có đầy đủ dữ liệu.");
            DisplayFallback();
            yield break;
        }

        // Chạy tất cả hiệu ứng đếm song song
        StartCoroutine(CountUpInt(enemiesKilledText,   killsLabel,  0, data.enemiesKilled,   countUpDuration));
        StartCoroutine(CountUpInt(questsCompletedText, questsLabel, 0, data.questsCompleted, countUpDuration));
        StartCoroutine(CountUpInt(goldEarnedText,      goldLabel,   0, data.goldEarned,       countUpDuration, suffix: " G"));
        StartCoroutine(CountUpTime(timePlayedText,     timeLabel,   data.timePlayed,          countUpDuration));
    }

    /// <summary>
    /// Đọc thống kê từ file saveData.json khi chạy EndScene mà không qua WorldScene.
    /// </summary>
    private GameStatsSaveData LoadStatsFromSaveFile()
    {
        string path = Path.Combine(Application.persistentDataPath, "saveData.json");
        if (!File.Exists(path))
        {
            Debug.LogWarning("[PlayerStatsPanel] Không tìm thấy file save: " + path);
            return null;
        }

        try
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
            if (saveData?.gameStats != null)
            {
                Debug.Log("[PlayerStatsPanel] Đọc thống kê từ file save thành công.");
                return saveData.gameStats;
            }

            // File save cũ chưa có gameStats → trả về mặc định
            Debug.LogWarning("[PlayerStatsPanel] File save không có dữ liệu thống kê (save cũ). Chơi 1 lần rồi lưu lại để có thống kê.");
            return new GameStatsSaveData(); // hiện toàn 0 thay vì ?
        }
        catch (System.Exception e)
        {
            Debug.LogError("[PlayerStatsPanel] Lỗi khi đọc file save: " + e.Message);
            return null;
        }
    }

    // ─── Hiệu ứng đếm số nguyên ───────────────────────────────────────────────
    private IEnumerator CountUpInt(TMP_Text textObj, string label, int from, int to,
                                   float duration, string suffix = "")
    {
        if (textObj == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t   = Mathf.Clamp01(elapsed / duration);
            float ease = EaseOutQuad(t);
            int current = Mathf.RoundToInt(Mathf.Lerp(from, to, ease));
            SetText(textObj, label, current.ToString("N0") + suffix);
            yield return null;
        }

        // Đảm bảo hiện đúng giá trị cuối
        SetText(textObj, label, to.ToString("N0") + suffix);
    }

    // ─── Hiệu ứng đếm thời gian ──────────────────────────────────────────────
    private IEnumerator CountUpTime(TMP_Text textObj, string label, float totalSeconds, float duration)
    {
        if (textObj == null) yield break;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t       = Mathf.Clamp01(elapsed / duration);
            float ease    = EaseOutQuad(t);
            float current = Mathf.Lerp(0, totalSeconds, ease);
            SetText(textObj, label, FormatTime(current));
            yield return null;
        }

        SetText(textObj, label, FormatTime(totalSeconds));
    }

    // ─── Helpers ──────────────────────────────────────────────────────────────
    private void SetText(TMP_Text textObj, string label, string value)
    {
        if (textObj == null) return;
        textObj.text = $"{label}\n<size=140%><b>{value}</b></size>";
    }

    private string FormatTime(float seconds)
    {
        int s = Mathf.FloorToInt(seconds);
        int h = s / 3600;
        int m = (s % 3600) / 60;
        int sec = s % 60;
        if (h > 0)
            return $"{h:D2}:{m:D2}:{sec:D2}";
        return $"{m:D2}:{sec:D2}";
    }

    /// <summary>Easing function: nhanh lúc đầu, chậm dần ở cuối → tự nhiên hơn</summary>
    private float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);

    private void DisplayFallback()
    {
        SetText(enemiesKilledText,   killsLabel,  "?");
        SetText(questsCompletedText, questsLabel, "?");
        SetText(goldEarnedText,      goldLabel,   "?");
        SetText(timePlayedText,      timeLabel,   "--:--");
    }
}
