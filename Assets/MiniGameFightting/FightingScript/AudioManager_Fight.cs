using UnityEngine;
using UnityEngine.Audio;

public class AudioManager_Fight : MonoBehaviour
{
	public static AudioManager_Fight instance;

	[Header("Mixer & Sources")]
	public AudioMixer fightMixer;
	public AudioSource musicSource;
	public AudioSource sfxSource;

	[Header("Music")]
	public AudioClip bgmClip;

	[Header("Character SFX")]
	public AudioClip moveClip;
	public AudioClip jumpClip;
	public AudioClip attackClip;
	public AudioClip dashClip;
	public AudioClip blockClip;
	public AudioClip hurtClip;
	public AudioClip skill1Clip;
	public AudioClip skill2Clip;
	public AudioClip skill3Clip;

	[Header("ButtonClips")]
	public AudioClip ButtonClip;

	[Header("Announcer / Voice")]
	public AudioClip fightClip; // "3...2...1...Fight!"

	private void Awake()
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

		// Gán output group cho từng source
		if (fightMixer != null)
		{
			var musicGroup = fightMixer.FindMatchingGroups("Music")[0];
			var sfxGroup = fightMixer.FindMatchingGroups("SFX")[0];

			musicSource.outputAudioMixerGroup = musicGroup;
			sfxSource.outputAudioMixerGroup = sfxGroup;
		}
	}

	private void Start()
	{
		PlayBGM();
	}

	// 🎵 Nhạc nền
	public void PlayBGM()
	{
		if (bgmClip == null || musicSource == null) return;
		musicSource.clip = bgmClip;
		musicSource.loop = true;
		musicSource.Play();
	}


	// 🕹 Hành động nhân vật
	public void PlayMove() { if (moveClip != null) sfxSource.PlayOneShot(moveClip); }
	public void PlayJump() { if (jumpClip != null) sfxSource.PlayOneShot(jumpClip); }
	public void PlayAttack() { if (attackClip != null) sfxSource.PlayOneShot(attackClip); }
	public void PlayDash() { if (dashClip != null) sfxSource.PlayOneShot(dashClip); }
	public void PlayBlock() { if (blockClip != null) sfxSource.PlayOneShot(blockClip); }
	public void PlayHurt() { if (hurtClip != null) sfxSource.PlayOneShot(hurtClip); }
	public void PlaySkill1() { if (skill1Clip != null) sfxSource.PlayOneShot(skill1Clip); }
	public void PlaySkill2() { if (skill2Clip != null) sfxSource.PlayOneShot(skill2Clip); }
	public void PlaySkill3() { if (skill3Clip != null) sfxSource.PlayOneShot(skill3Clip); }
	public void VoiceFight() { if (fightClip != null) sfxSource.PlayOneShot(fightClip); }
	public void PlayButton() { if (ButtonClip != null) sfxSource.PlayOneShot(ButtonClip); }

	public void StopBGM()
	{
		if (musicSource.isPlaying)
			musicSource.Stop();
	}
}
