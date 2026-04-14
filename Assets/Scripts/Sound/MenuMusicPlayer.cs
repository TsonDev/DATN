using UnityEngine;

public class MenuMusicPlayer : MonoBehaviour
{
    [Tooltip("Kéo âm thanh nhạc nền của Menu vào đây")]
    public AudioClip menuBGM;

    void Start()
    {
        // Phá băng thời gian nếu người chơi thoát lúc đang ở Pause Menu (ví dụ timeScale = 0)
        Time.timeScale = 1f;

        Debug.Log("MenuMusicPlayer Start chạy. SoundManager: " + (SoundManager.Instance != null) + ", menuBGM: " + (menuBGM != null));
        if (SoundManager.Instance != null)
        {
            if (menuBGM == null)
            {
                Debug.LogError("LỖI: Bạn chưa kéo file bài nhạc Menu vào ô Menu BGM trong script MenuMusicPlayer!");
            }
            else
            {
                Debug.Log("Đang yêu cầu SoundManager đổi sang nhạc Menu.");
                SoundManager.Instance.PlayBGM(menuBGM);
            }
        }
        else
        {
            Debug.LogError("LỖI: SoundManager bị null tại thời điểm Menu load!");
        }
    }
}
