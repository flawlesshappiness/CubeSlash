using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundController : Singleton
{
    public static SoundController Instance { get { return Instance<SoundController>(); } }

    private Dictionary<string, GroupCoroutine> group_coroutines = new Dictionary<string, GroupCoroutine>();

    private class GroupCoroutine
    {
        public string path;
        public int count;
        public int max;
        public float volume = 1;
        public FMODEventReference reference;
        public CustomCoroutine coroutine;

        public GroupCoroutine(FMODEventReference reference)
        {
            this.reference = reference;
            path = reference.Info.path;
        }
    }

    public FMODEventInstance Play(SoundEffectType type)
    {
        var inst = CreateInstance(type);
        return inst.Play();
    }

    public FMODEventInstance CreateInstance(SoundEffectType type)
    {
        var entry = SoundDatabase.GetEntry(type);
        if (entry != null)
        {
            return entry.sfx.CreateInstance();
        }
        else
        {
            LogController.Instance.LogMessage($"SoundController.CreateInstance: Failed to get entry with type {type}");
            return new FMODEventInstance(null);
        }
    }

    private GroupCoroutine GetGroup(FMODEventReference reference)
    {
        var path = reference.Info.path;
        if (group_coroutines.ContainsKey(path))
        {
            return group_coroutines[path];
        }
        else
        {
            var group = new GroupCoroutine(reference);
            group_coroutines.Add(path, group);
            return group;
        }
    }

    public void PlayGroup(SoundEffectType type)
    {
        var entry = SoundDatabase.GetEntry(type);
        if (entry == null) return;
        PlayGroup(entry.sfx);
    }

    public void PlayGroup(FMODEventReference reference)
    {
        if (!reference.Exists) return;
        var path = reference.Info.path;
        var group = GetGroup(reference);
        group.count += (group.count < 3 ? 1 : 0);

        if (group.coroutine == null)
        {
            group.coroutine = this.StartCoroutineWithID(Cr(group), $"{path}_{GetInstanceID()}");
        }

        IEnumerator Cr(GroupCoroutine group)
        {
            while (group.count > 0)
            {
                var instance = reference.CreateInstance();
                instance.SetVolume(group.volume);
                instance.Play();
                group.count--;
                yield return new WaitForSecondsRealtime(0.05f);
            }

            group.coroutine = null;
        }
    }

    public void SetGroupVolumeByPosition(SoundEffectType type, Vector3 position)
    {
        var entry = SoundDatabase.GetEntry(type);
        if (entry == null) return;
        SetGroupVolumeByPosition(entry.sfx, position);
    }

    public void SetGroupVolumeByPosition(FMODEventReference reference, Vector3 position)
    {
        if (!reference.Exists) return;
        var group = GetGroup(reference);
        group.volume = FMODEventInstance.GetVolumeByPosition(position);
    }
}