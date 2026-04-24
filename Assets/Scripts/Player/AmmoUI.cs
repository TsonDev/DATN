using System.Collections;
using TMPro;
using UnityEngine;

/// <summary>
/// Hiển thị số đạn còn lại trên màn hình.
/// Gán TMP_Text từ Canvas vào ammoText trong Inspector.
/// </summary>
public class AmmoUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text ammoText;

    [Header("Warning Settings")]
    [SerializeField] private int lowAmmoThreshold = 5;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color lowAmmoColor = new Color(1f, 0.3f, 0.3f, 1f); // Đỏ nhạt
    [SerializeField] private Color emptyAmmoColor = Color.red;
    [SerializeField] private float blinkSpeed = 3f;

    private Coroutine blinkCoroutine;

    void Start()
    {
        if (AmmoManager.Instance != null)
        {
            AmmoManager.Instance.OnAmmoChanged += UpdateAmmoDisplay;
            // Hiển thị ban đầu
            UpdateAmmoDisplay(AmmoManager.Instance.GetCurrentAmmo(), AmmoManager.Instance.GetMaxAmmo());
        }
    }

    void OnDestroy()
    {
        if (AmmoManager.Instance != null)
        {
            AmmoManager.Instance.OnAmmoChanged -= UpdateAmmoDisplay;
        }
    }

    private void UpdateAmmoDisplay(int current, int max)
    {
        if (ammoText == null) return;

        if (current <= 0)
        {
            // Hết đạn
            ammoText.text = "0";
            ammoText.color = emptyAmmoColor;
            StartBlinking();
        }
        else if (current <= lowAmmoThreshold)
        {
            // Đạn sắp hết
            ammoText.text = $"{current}";
            ammoText.color = lowAmmoColor;
            StartBlinking();
        }
        else
        {
            // Bình thường
            ammoText.text = $"{current}";
            ammoText.color = normalColor;
            StopBlinking();
        }
    }

    private void StartBlinking()
    {
        if (blinkCoroutine == null)
        {
            blinkCoroutine = StartCoroutine(BlinkRoutine());
        }
    }

    private void StopBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
            if (ammoText != null)
            {
                var c = ammoText.color;
                c.a = 1f;
                ammoText.color = c;
            }
        }
    }

    private IEnumerator BlinkRoutine()
    {
        while (true)
        {
            if (ammoText == null) yield break;

            // Nhấp nháy alpha
            float alpha = (Mathf.Sin(Time.unscaledTime * blinkSpeed) + 1f) / 2f;
            alpha = Mathf.Lerp(0.3f, 1f, alpha);

            var color = ammoText.color;
            color.a = alpha;
            ammoText.color = color;

            yield return null;
        }
    }
}
