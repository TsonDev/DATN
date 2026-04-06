using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class VolumeUI_Fight : MonoBehaviour
{
	[Header("Mixer")]
	[SerializeField] private AudioMixer fightMixer;

	[Header("UI Sliders")]
	[SerializeField] private Slider musicSlider;
	[SerializeField] private Slider sfxSlider;

	[Header("Volume Panel")]
	[SerializeField] private GameObject volumePanel;

	private bool isVisible = false;

	private const string MUSIC_KEY = "FightMusicVolume";
	private const string SFX_KEY = "FightSFXVolume";

	private void Start()
	{
		LoadVolume();

		// Sự kiện khi kéo slider
		musicSlider.onValueChanged.AddListener(delegate { SetMusicVolume(); });
		sfxSlider.onValueChanged.AddListener(delegate { SetSFXVolume(); });

		// Ẩn UI volume khi khởi đầu
		if (volumePanel != null)
			volumePanel.SetActive(false);
	}

	// 🟢 Bật / Tắt UI Volume
	public void ToggleVolumePanel()
	{
		isVisible = !isVisible;
		if (volumePanel != null)
			volumePanel.SetActive(isVisible);
	}

	// 🎵 Điều chỉnh nhạc nền
	public void SetMusicVolume()
	{
		float volume = Mathf.Clamp(musicSlider.value, 0.0001f, 1f);
		fightMixer.SetFloat("BGMFightVolume", Mathf.Log10(volume) * 20f);
		PlayerPrefs.SetFloat(MUSIC_KEY, volume);
	}

	// 💥 Điều chỉnh hiệu ứng (đấm, đá, skill, v.v…)
	public void SetSFXVolume()
	{
		float volume = Mathf.Clamp(sfxSlider.value, 0.0001f, 1f);
		fightMixer.SetFloat("SFXFightVolume", Mathf.Log10(volume) * 20f);
		PlayerPrefs.SetFloat(SFX_KEY, volume);
	}

	// 🔄 Tải giá trị đã lưu
	private void LoadVolume()
	{
		float musicVol = PlayerPrefs.GetFloat(MUSIC_KEY, 0.75f);
		float sfxVol = PlayerPrefs.GetFloat(SFX_KEY, 0.75f);

		musicSlider.value = musicVol;
		sfxSlider.value = sfxVol;

		SetMusicVolume();
		SetSFXVolume();
	}
}
