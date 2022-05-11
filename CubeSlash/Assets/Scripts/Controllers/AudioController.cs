using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioController : MonoBehaviour, IInitializable
{
    public static AudioController Instance { get; private set; }

    public AudioMixerSnapshot snapshot_main;
    public AudioMixerSnapshot snapshot_menu;

    public void Initialize()
    {
        Instance = this;
    }
}
