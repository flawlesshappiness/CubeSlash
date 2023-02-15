using FMODUnity;
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
        public CustomCoroutine coroutine;
    }

    public FMODEventInstance Play(SoundEffectType type)
    {
        var inst = CreateInstance(type);
        return inst.Play();
    }

    public FMODEventInstance CreateInstance(SoundEffectType type)
    {
        var entry = SoundDatabase.GetEntry(type);
        if(entry != null)
        {
            return entry.sfx.CreateInstance();
        }
        else
        {
            LogController.Instance.LogMessage($"SoundController.CreateInstance: Failed to get entry with type {type}");
            return new FMODEventInstance(null);
        }
    }

    public void PlayGroup(FMODEventReference reference)
    {
        if (!reference.Exists) return;
        var path = reference.Info.path;

        var group = GetGroup();
        group.count += (group.count < 3 ? 1 : 0);

        if (group.coroutine == null)
        {
            group.coroutine = this.StartCoroutineWithID(Cr(group), $"{path}_{GetInstanceID()}");
        }

        IEnumerator Cr(GroupCoroutine group)
        {
            while (group.count > 0)
            {
                RuntimeManager.PlayOneShot(path);
                group.count--;
                yield return new WaitForSecondsRealtime(0.05f);
            }

            group.coroutine = null;
        }

        GroupCoroutine GetGroup()
        {
            if (group_coroutines.ContainsKey(path))
            {
                return group_coroutines[path];
            }
            else
            {
                var group = new GroupCoroutine { path = path };
                group_coroutines.Add(path, group);
                return group;
            }
        }
    }
}