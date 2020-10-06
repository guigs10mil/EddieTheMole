using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound
{
    public string name;

    public string group;

    public AudioClip clip;

    [Range(0.0f, 1.0f)]
    public float volume = 1.0f;

    [HideInInspector]
    public AudioSource source;

}
