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
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Cho phép SoundManager mang sang scene khác
        }
        else
        {
            // Chỉ xóa GameObject SoundManager trùng lặp, không xóa luôn cả Canvas/Background nếu vô tình gắn chung
            Destroy(gameObject);
        }
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
        sfxSource.volume = value;  // fix: phải là sfxSource
    }

    // --- Dành cho SettingUI gọi ---
    /// <summary>Điều chỉnh âm lượng nhạc nền (BGM). Gọi từ SettingUI.</summary>
    public void SetMasterVolumeBGM(float value)
    {
        bgmSource.volume = Mathf.Clamp01(value);
    }

    /// <summary>Điều chỉnh âm lượng hiệu ứng âm thanh (SFX). Gọi từ SettingUI.</summary>
    public void SetMasterVolumeSFX(float value)
    {
        sfxSource.volume = Mathf.Clamp01(value);
    }
}
