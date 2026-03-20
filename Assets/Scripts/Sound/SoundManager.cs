using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioSource sfxSource;
    public AudioSource bgmSource;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

    }
    public void PlaySound(SoundData sound)
    {
        sfxSource.pitch = sound.pitch;
        sfxSource.PlayOneShot(sound.clip, sound.volume);
    }
    public void PlayBGM(AudioClip clip)
    {
        if (bgmSource.clip == clip) return;

        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        bgmSource.Stop();
    }

    public void SetVolumeBGM(float value)
    {
        bgmSource.volume = value;
    }
    public void SetVolumeSFX(float value)
    {
        bgmSource.volume = value;
    }
}
