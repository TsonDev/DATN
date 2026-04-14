using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerfaceArea : MonoBehaviour
{
    //public enum serfaceType { floor, earth, sand, snow};
    public EnumSerfaceArea serfaceType;
    public AudioClip audioClip;
    public SoundData soundData;
    [Range(0f, 1f)]
    [Tooltip("Điều chỉnh âm lượng nền chung cho khu vực này")]
    public float bgmVolume = 1f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if(audioClip == null)
            {
                SoundManager.Instance.StopBGM();
            }
            else
            {
                SoundManager.Instance.PlayBGM(audioClip);
                SoundManager.Instance.SetVolumeBGM(bgmVolume);
            }
        }
    }
    public void OnBGMSliderChanged(float value)
    {
        soundData.volume = value; // Cập nhật âm lượng trong SoundData
        SoundManager.Instance.SetMasterVolumeBGM(value);
    }
}
