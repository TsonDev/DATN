using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    public AudioSource sfxSource;
    public AudioSource bgmSource;

    // Âm lượng BGM do người dùng đặt trong Setting (lưu vào save file)
    private float userBGMVolume = 1f;

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

    /// <summary>
    /// Gọi từ SerfaceArea: đặt âm lượng theo khu vực, nhân với userBGMVolume để tôn trọng setting người dùng.
    /// </summary>
    public void SetVolumeBGM(float areaVolume)
    {
        bgmSource.volume = Mathf.Clamp01(userBGMVolume * areaVolume);
    }

    public void SetVolumeSFX(float value)
    {
        sfxSource.volume = value;  // fix: phải là sfxSource
    }

    /// <summary>Trả về âm lượng BGM do người dùng đặt (không phải âm lượng area).</summary>
    public float GetVolumeBGM() => userBGMVolume;
    public float GetVolumeSFX() => sfxSource != null ? sfxSource.volume : 1f;

    // --- Dành cho SettingUI / GameController gọi ---
    /// <summary>Lưu âm lượng BGM do người dùng đặt và áp dụng ngay lập tức.</summary>
    public void SetMasterVolumeBGM(float value)
    {
        userBGMVolume = Mathf.Clamp01(value);
        bgmSource.volume = Mathf.Clamp01(value);
    }

    /// <summary>Điều chỉnh âm lượng hiệu ứng âm thanh (SFX). Gọi từ SettingUI.</summary>
    public void SetMasterVolumeSFX(float value)
    {
        sfxSource.volume = Mathf.Clamp01(value);
    }
}
