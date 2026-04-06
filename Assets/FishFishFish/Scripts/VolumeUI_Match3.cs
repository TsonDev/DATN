using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeUI_Match3 : MonoBehaviour
{
	[Header("Mixer")]
	[SerializeField] private AudioMixer mixerMatch3;

	[Header("Sliders")]
	[SerializeField] private Slider musicSlider;
	[SerializeField] private Slider sfxSlider;

	[Header("UI Panel")]
	[SerializeField] private GameObject volumePanel;

	private bool isPanelVisible = false;

	private void Start()
	{
		LoadVolume();

		// Gán sự kiện khi kéo slider
		musicSlider.onValueChanged.AddListener(delegate { SetMusicVolume(); });
		sfxSlider.onValueChanged.AddListener(delegate { SetSFXVolume(); });

		// Ẩn bảng UI khi mới vào
		if (volumePanel != null)
			volumePanel.SetActive(false);
	}

	// Bật / tắt panel điều chỉnh âm lượng
	public void ToggleVolumePanel()
	{
		isPanelVisible = !isPanelVisible;
		if (volumePanel != null)
			volumePanel.SetActive(isPanelVisible);
	}

	// 🎵 Điều chỉnh nhạc nền
	public void SetMusicVolume()
	{
		float v = Mathf.Clamp(musicSlider.value, 0.0001f, 1f);
		mixerMatch3.SetFloat("BGMMatch3Volume", Mathf.Log10(v) * 20f);
		PlayerPrefs.SetFloat("BGMMatch3Value", v);
	}

	// 💥 Điều chỉnh hiệu ứng (SFX)
	public void SetSFXVolume()
	{
		float v = Mathf.Clamp(sfxSlider.value, 0.0001f, 1f);
		mixerMatch3.SetFloat("SFXMatch3Volume", Mathf.Log10(v) * 20f);
		PlayerPrefs.SetFloat("SFXMatch3Value", v);
	}

	// 🔁 Tải lại giá trị đã lưu
	private void LoadVolume()
	{
		float musicVol = PlayerPrefs.GetFloat("BGMMatch3Value", 0.75f);
		float sfxVol = PlayerPrefs.GetFloat("SFXMatch3Value", 0.75f);

		musicSlider.value = musicVol;
		sfxSlider.value = sfxVol;

		SetMusicVolume();
		SetSFXVolume();
	}
}
