using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SerfaceArea : MonoBehaviour
{
    //public enum serfaceType { floor, earth, sand, snow};
    public EnumSerfaceArea serfaceType;
    public AudioClip audioClip;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if(audioClip == null)
            {
                SoundManager.Instance.StopBGM();
            }
            SoundManager.Instance.PlayBGM(audioClip);
        }
    }

}
