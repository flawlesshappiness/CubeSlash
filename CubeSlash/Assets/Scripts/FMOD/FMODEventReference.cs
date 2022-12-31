using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;

[System.Serializable]
public class FMODEventReference
{
    public const bool DEBUG = false;

    [SerializeField]
    public EventReference reference;

    private bool init_desc;
    private EventInstance current_instance;

    private FMODEventReferenceInfo _info;
    public FMODEventReferenceInfo Info { get { return _info ?? GetInfo(); } }
    public string Path { get { return GetInfo().path; } }

    private static Dictionary<string, int> dicPlayCount = new Dictionary<string, int>();
    private static Dictionary<string, float> dicPlayTimestamp = new Dictionary<string, float>();

    public int PlayCount { get { return GetPlayCount(); } }

    public FMODEventReferenceInfo GetInfo()
    {
        if(_info == null)
        {
            _info = new FMODEventReferenceInfo(reference);
        }

        return _info;
    }

    public void Stop(FMOD.Studio.STOP_MODE mode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT)
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
        var delta_pitch = 1f + delta_note * pitch;
        Play(e =>
        {
            e.setPitch(delta_pitch);
        });
    }

    private void IncrementPlayCount()
    {
        if (!dicPlayCount.ContainsKey(Path))
        {
            dicPlayCount.Add(Path, 0);
        }

        dicPlayCount[Path]++;
    }

    private void SetPlayTimestamp()
    {
        if (!dicPlayTimestamp.ContainsKey(Path))
        {
            dicPlayTimestamp.Add(Path, Time.time);
        }
        else
        {
            dicPlayTimestamp[Path] = Time.time;
        }
    }

    private bool GetPlayTimestamp(out float timestamp)
    {
        var key = Path;
        var contains = dicPlayTimestamp.ContainsKey(key);
        timestamp = contains ? dicPlayTimestamp[key] : 0;
        return contains;
    }

    private int GetPlayCount()
    {
        return dicPlayCount.ContainsKey(Path) ? dicPlayCount[Path] : 0;
    }
}

public class FMODEventReferenceInfo
{
    public EventDescription Description { get; private set; }
    public string path;
    public int length;

    public FMODEventReferenceInfo(EventReference reference)
    {
        try
        {
            Description = RuntimeManager.GetEventDescription(reference);
            Description.getPath(out path);
            Description.getLength(out length);
        }
        catch (System.Exception e)
        {
            DebugLog(e.StackTrace);
        }
    }

    private void DebugLog(string text)
    {
        if (FMODEventReference.DEBUG)
        {
            Debug.Log(text);
        }
    }
}