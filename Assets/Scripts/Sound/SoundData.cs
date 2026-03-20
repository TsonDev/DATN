using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName ="Audio/Sound")]
public class SoundData : ScriptableObject
{
    public AudioClip clip;
    public float volume = 1f;
    public float pitch = 1f;
}
