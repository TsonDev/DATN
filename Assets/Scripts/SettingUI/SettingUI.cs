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
    void SetMusicBGM(float value)
    {
        //set value from SoundManager
        SoundManager.Instance.SetVolumeBGM(value);
    }
    void SetMusicSFX(float value)
    {
        //set value from SoundManager
        SoundManager.Instance.SetVolumeSFX(value);
    }
}
