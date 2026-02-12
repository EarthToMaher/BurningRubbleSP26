using System;
using UnityEngine;
using UnityEngine.Audio;

[Serializable]
public class Music
{
    /* DO NOT touch this class script unless you speak to me first please
    * Thank you,
    * ~ RV
    */

    public string _name;

    public AudioClip _clip;

    [Range(0f, 100f)]
    public float _volume;
    [Range(0f, 1f)]
    public float _pitch;

    public bool _loop;
    public bool _playOnAwake;

    [HideInInspector]
    public AudioSource _source;

}
