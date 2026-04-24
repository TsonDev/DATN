using System;
using UnityEngine;

public class AmmoManager : MonoBehaviour
{
    public static AmmoManager Instance { get; private set; }

    [Header("Ammo Settings")]
    [SerializeField] private int startAmmo = 30;
    [SerializeField] private int maxAmmo = 99;

    [Header("Ammo Item")]
    [Tooltip("ID của item đạn trong ItemDictionary. Khi UseItem() sẽ nạp đạn.")]
    [SerializeField] private int ammoItemID = -1;
    [Tooltip("Số đạn được nạp mỗi khi dùng 1 item đạn")]
    [SerializeField] private int ammoPerItem = 10;

    private int currentAmmo;

    /// <summary>
    /// Event phát ra khi số đạn thay đổi: (currentAmmo, maxAmmo)
    /// </summary>
    public event Action<int, int> OnAmmoChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        currentAmmo = startAmmo;
    }

    /// <summary>
    /// Thử tiêu thụ 1 viên đạn. Trả true nếu còn đạn, false nếu hết.
    /// </summary>
    public bool TryConsumeAmmo()
    {
        if (currentAmmo <= 0)
        {
            return false;
        }
        currentAmmo--;
        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
        return true;
    }

    /// <summary>
    /// Thêm đạn (ví dụ: khi mua từ shop hoặc sử dụng item)
    /// </summary>
    public void AddAmmo(int amount)
    {
        currentAmmo = Mathf.Min(currentAmmo + amount, maxAmmo);
        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
    }

    /// <summary>
    /// Set đạn trực tiếp (dùng khi load game)
    /// </summary>
    public void SetAmmo(int current, int max)
    {
        maxAmmo = max;
        currentAmmo = Mathf.Clamp(current, 0, maxAmmo);
        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
    }

    /// <summary>
    /// Set đạn mặc định (dùng khi new game)
    /// </summary>
    public void ResetAmmo()
    {
        currentAmmo = startAmmo;
        OnAmmoChanged?.Invoke(currentAmmo, maxAmmo);
    }

    public int GetCurrentAmmo() => currentAmmo;
    public int GetMaxAmmo() => maxAmmo;
    public int GetAmmoItemID() => ammoItemID;
    public int GetAmmoPerItem() => ammoPerItem;

    /// <summary>
    /// Kiểm tra đạn có ít hay không (dùng cho UI cảnh báo)
    /// </summary>
    public bool IsAmmoLow(int threshold = 5)
    {
        return currentAmmo <= threshold && currentAmmo > 0;
    }

    public bool IsAmmoEmpty()
    {
        return currentAmmo <= 0;
    }
}
