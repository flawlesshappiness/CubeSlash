using UnityEngine;
using FMODUnity;
using System.Collections;
using FMOD.Studio;
using System.Collections.Generic;

public class FMODController : Singleton
{
    public static FMODController Instance { get { return Instance<FMODController>(); } }

    private Dictionary<string, LimitDelay> dicLimitDelay = new Dictionary<string, LimitDelay>();

    private class LimitDelay
    {
        public string path;
        public int count;
        public CustomCoroutine coroutine;
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

    public float GetBusVolume(Bus bus)
    {
        bus.getVolume(out var volume);
        return volume;
    }

    public Bus GetMasterBus() => GetBus("");
    public Bus GetBus(string path)
    {
        return RuntimeManager.GetBus($"bus:/{path}");
    }

    public EventInstance CreateSnapshotInstance(string path)
    {
        return RuntimeManager.CreateInstance($"snapshot:/{path}");
    }
}