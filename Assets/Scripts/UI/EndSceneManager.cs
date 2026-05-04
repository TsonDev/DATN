using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Quản lý End Scene: khởi động credits, xử lý về menu.
/// Gắn script này vào một GameObject rỗng trong EndScene.
/// </summary>
public class EndSceneManager : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("Tên scene Menu để quay về sau khi credits xong")]
    public string menuSceneName = "MenuScence";

    [Header("References")]
    public CreditsController creditsController;

    // ─────────────────────────────────────────────────────────────────────────
    private void Start()
    {
        // Đăng ký sự kiện kết thúc credits → về menu
        if (creditsController != null)
            creditsController.onCreditsFinished.AddListener(GoToMenu);
    }

    // ─── Public ───────────────────────────────────────────────────────────────
    /// <summary>Gọi khi credits kết thúc → load scene menu</summary>
    public void GoToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(menuSceneName);
    }

    /// <summary>Cho phép nút "Bỏ qua" / Skip</summary>
    public void SkipCredits()
    {
        creditsController?.SkipCredits();
    }
}
