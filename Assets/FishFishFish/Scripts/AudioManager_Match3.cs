using UnityEngine;
using UnityEngine.Audio;

public class AudioManager_Match3 : MonoBehaviour
{
	public static AudioManager_Match3 instance;

	[Header("Mixer & Sources")]
	public AudioMixer mixerMatch3;
	public AudioSource musicSource;
	public AudioSource sfxSource;

	[Header("Clips")]
	public AudioClip bgmClip;
	public AudioClip swapClip;
	public AudioClip matchClip;
	public AudioClip comboClip;
	public AudioClip failSwapClip;

	void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
			return;
		}

		if (mixerMatch3 != null)
		{
			var musicGroup = mixerMatch3.FindMatchingGroups("Music")[0];
			var sfxGroup = mixerMatch3.FindMatchingGroups("SFX")[0];

			musicSource.outputAudioMixerGroup = musicGroup;
			sfxSource.outputAudioMixerGroup = sfxGroup;
		}
	}

	// Bỏ auto play, chỉ play khi được gọi
	public void PlayBGM()
	{
		if (bgmClip == null || musicSource == null) return;
		musicSource.clip = bgmClip;
		musicSource.loop = true;
		musicSource.Play();
	}

	public void StopBGM()
	{
		if (musicSource != null && musicSource.isPlaying)
			musicSource.Stop();
	}

	// ==== SFX ====
	public void PlaySwap()
	{
		if (swapClip != null)
			sfxSource.PlayOneShot(swapClip);
	}

	public void PlayMatch()
	{
		if (matchClip != null)
			sfxSource.PlayOneShot(matchClip);
	}

	public void PlayCombo()
	{
		if (comboClip != null)
			sfxSource.PlayOneShot(comboClip);
	}

	public void PlayFailSwap()
	{
		if (failSwapClip != null)
			sfxSource.PlayOneShot(failSwapClip);
	}
}
