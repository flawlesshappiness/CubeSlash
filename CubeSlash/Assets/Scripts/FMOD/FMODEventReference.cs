using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;

[System.Serializable]
public class FMODEventReference
{
    [SerializeField]
    public EventReference reference;

    private EventInstance current_instance;

    private static Dictionary<string, int> dicPlayCount = new Dictionary<string, int>();
    private static Dictionary<string, float> dicPlayTimestamp = new Dictionary<string, float>();

    public int PlayCount { get { return dicPlayCount.ContainsKey(reference.Path) ? dicPlayCount[reference.Path] : 0; } }

    public void Stop(FMOD.Studio.STOP_MODE mode = FMOD.Studio.STOP_MODE.IMMEDIATE)
    {
        current_instance.stop(mode);
    }

    public void Play(System.Action<EventInstance> modifyInstance = null)
    {
        current_instance = RuntimeManager.CreateInstance(reference);
        modifyInstance?.Invoke(current_instance);
        current_instance.start();
        IncrementPlayCount();
        SetPlayTimestamp();
    }

    public void PlayWithTimeLimit(float time_limit, System.Action<EventInstance> modify_instance = null)
    {
        var has_timestamp = GetPlayTimestamp(out var timestamp);
        if(!has_timestamp || Time.time >= timestamp + time_limit)
        {
            Play(modify_instance);
        }
    }

    public void PlayWithPitch(int pitch)
    {
        var delta_note = 1 / 12f;
        var delta_pitch = delta_note * pitch;
        Play(e =>
        {
            e.setPitch(delta_pitch);
        });
    }

    private void IncrementPlayCount()
    {
        if (!dicPlayCount.ContainsKey(reference.Path))
        {
            dicPlayCount.Add(reference.Path, 0);
        }

        dicPlayCount[reference.Path]++;
    }

    private void SetPlayTimestamp()
    {
        if (!dicPlayTimestamp.ContainsKey(reference.Path))
        {
            dicPlayTimestamp.Add(reference.Path, Time.time);
        }
        else
        {
            dicPlayTimestamp[reference.Path] = Time.time;
        }
    }

    private bool GetPlayTimestamp(out float timestamp)
    {
        var key = reference.Path;
        var contains = dicPlayTimestamp.ContainsKey(key);
        timestamp = contains ? dicPlayTimestamp[key] : 0;
        return contains;
    }

    public EventDescription GetDescription()
    {
        return RuntimeManager.GetEventDescription(reference);
    }

    public int GetLength()
    {
        GetDescription().getLength(out var length);
        return length;
    }
}