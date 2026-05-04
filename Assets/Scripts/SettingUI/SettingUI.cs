using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    public Slider sliderBGM;
    public Slider sliderSFX;

    void Start()
    {
        sliderBGM.onValueChanged.AddListener(SetMusicBGM);
        sliderSFX.onValueChanged.AddListener(SetMusicSFX);
    }

    // Đồng bộ slider mỗi khi panel Setting được mở
    void OnEnable()
    {
        if (SoundManager.Instance == null) return;
        // Tắt listener tạm để tránh trigger event khi set giá trị
        sliderBGM.onValueChanged.RemoveListener(SetMusicBGM);
        sliderSFX.onValueChanged.RemoveListener(SetMusicSFX);

        sliderBGM.value = SoundManager.Instance.GetVolumeBGM();
        sliderSFX.value = SoundManager.Instance.GetVolumeSFX();

        sliderBGM.onValueChanged.AddListener(SetMusicBGM);
        sliderSFX.onValueChanged.AddListener(SetMusicSFX);
    }

    void SetMusicBGM(float value)
    {
        SoundManager.Instance.SetMasterVolumeBGM(value);
    }

    void SetMusicSFX(float value)
    {
        SoundManager.Instance.SetVolumeSFX(value);
    }
}
