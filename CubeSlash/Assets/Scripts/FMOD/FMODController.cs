using UnityEngine;
using FMODUnity;
using System.Collections;
using FMOD.Studio;
using System.Collections.Generic;

public class FMODController : Singleton
{
    public static FMODController Instance { get { return Instance<FMODController>(); } }

    private Dictionary<string, LimitDelay> dicLimitDelay = new Dictionary<string, LimitDelay>();

    private Bus Music;
    private Bus SFX;
    private Bus Master;

    private class LimitDelay
    {
        public string path;
        public int count;
        public CustomCoroutine coroutine;
    }

    protected override void Initialize()
    {
        base.Initialize();

        Music = RuntimeManager.GetBus("bus:/Music");
        SFX = RuntimeManager.GetBus("bus:/SFX");
        Master = RuntimeManager.GetBus("bus:/");
    }

    public void Play(string path)
    {
        RuntimeManager.PlayOneShot(path);
    }

    public void PlayWithDelay(FMODEventReference reference, float delay, System.Action<EventInstance> modify_instance = null)
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(delay);
            reference.Play(modify_instance);
        }
    }

    public void PlayWithLimitDelay(FMODEventReference reference)
    {
        var path = reference.Path;

        if (!dicLimitDelay.ContainsKey(path))
        {
            var _ld = new LimitDelay { path = path };
            dicLimitDelay.Add(path, _ld);
        }

        var ld = dicLimitDelay[path];
        if (ld.count < 3)
        {
            ld.count++;
        }

        if (ld.coroutine == null)
        {
            ld.coroutine = this.StartCoroutineWithID(Cr(ld), $"{path}_{GetInstanceID()}");
        }

        IEnumerator Cr(LimitDelay ld)
        {
            while(ld.count > 0)
            {
                Play(path);
                ld.count--;
                yield return new WaitForSecondsRealtime(0.05f);
            }

            ld.coroutine = null;
        }
    }

    public void SetMusicVolume(float volume) => Music.setVolume(volume);
    public void SetSFXVolume(float volume) => SFX.setVolume(volume);
    public void SetMasterVolume(float volume) => Master.setVolume(volume);
    public float GetMusicVolume() => GetBusVolume(Music);
    public float GetSFXVolume() => GetBusVolume(SFX);
    public float GetMasterVolume() => GetBusVolume(Master);

    private float GetBusVolume(Bus bus)
    {
        bus.getVolume(out var volume);
        return volume;
    }
}