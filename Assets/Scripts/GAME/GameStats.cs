using System;
using UnityEngine;

/// <summary>
/// Singleton tồn tại xuyên suốt các scene (DontDestroyOnLoad).
/// Theo dõi thống kê gameplay: số quái đã giết, thời gian chơi,
/// số quest hoàn thành, gold kiếm được.
/// Truy cập từ bất kỳ đâu: GameStats.Instance.AddKill();
/// </summary>
public class GameStats : MonoBehaviour
{
    public static GameStats Instance { get; private set; }

    // ─── Thống kê ─────────────────────────────────────────────────────────────
    [SerializeField] private int _enemiesKilled;
    [SerializeField] private int _questsCompleted;
    [SerializeField] private int _goldEarned;
    [SerializeField] private float _timePlayed; // tính bằng giây

    // ─── Public getters ───────────────────────────────────────────────────────
    public int EnemiesKilled   => _enemiesKilled;
    public int QuestsCompleted => _questsCompleted;
    public int GoldEarned      => _goldEarned;
    public float TimePlayed    => _timePlayed;

    /// <summary>Thời gian chơi định dạng hh:mm:ss</summary>
    public string TimePlayedFormatted
    {
        get
        {
            TimeSpan t = TimeSpan.FromSeconds(_timePlayed);
            if (t.Hours > 0)
                return $"{t.Hours:D2}:{t.Minutes:D2}:{t.Seconds:D2}";
            return $"{t.Minutes:D2}:{t.Seconds:D2}";
        }
    }

    // ─── Tracking time ────────────────────────────────────────────────────────
    private bool _trackTime = true;

    // ══════════════════════════════════════════════════════════════════════════
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (_trackTime)
            _timePlayed += Time.deltaTime;
    }

    // ─── Public API ───────────────────────────────────────────────────────────
    public void AddKill()           => _enemiesKilled++;
    public void AddQuestCompleted() => _questsCompleted++;
    public void AddGoldEarned(int amount)
    {
        if (amount > 0) _goldEarned += amount;
    }

    /// <summary>Tắt đếm thời gian (khi vào EndScene)</summary>
    public void StopTimer() => _trackTime = false;

    /// <summary>Load thống kê từ SaveData (khi tiếp tục game)</summary>
    public void LoadFromSave(GameStatsSaveData data)
    {
        if (data == null) return;
        _enemiesKilled   = data.enemiesKilled;
        _questsCompleted = data.questsCompleted;
        _goldEarned      = data.goldEarned;
        _timePlayed      = data.timePlayed;
    }

    /// <summary>Xuất thống kê để lưu vào SaveData</summary>
    public GameStatsSaveData ToSaveData() => new GameStatsSaveData
    {
        enemiesKilled   = _enemiesKilled,
        questsCompleted = _questsCompleted,
        goldEarned      = _goldEarned,
        timePlayed      = _timePlayed
    };

    /// <summary>Reset toàn bộ thống kê (khi New Game)</summary>
    public void Reset()
    {
        _enemiesKilled   = 0;
        _questsCompleted = 0;
        _goldEarned      = 0;
        _timePlayed      = 0f;
        _trackTime       = true;
    }
}

/// <summary>Dữ liệu thống kê được serialize vào file save</summary>
[Serializable]
public class GameStatsSaveData
{
    public int   enemiesKilled;
    public int   questsCompleted;
    public int   goldEarned;
    public float timePlayed;
}
