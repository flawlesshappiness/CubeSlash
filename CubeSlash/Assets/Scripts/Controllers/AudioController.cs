using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioController : Singleton
{
    public static AudioController Instance { get { return Instance<AudioController>(); } }

    private AudioMixer mixer;

    public enum Snapshot { MUTE, MAIN, MENU }

    public override void Initialize()
    {
        mixer = Resources.Load<AudioMixer>("Audio/AudioMixer");
    }

    public void TransitionTo(Snapshot type, float time = 0)
    {
        var snapshot = mixer.FindSnapshot(type.ToString());
        snapshot.TransitionTo(time);
    }
}
