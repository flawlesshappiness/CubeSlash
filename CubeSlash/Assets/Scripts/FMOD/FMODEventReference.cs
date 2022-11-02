using UnityEngine;
using FMODUnity;
using FMOD.Studio;

[System.Serializable]
public class FMODEventReference
{
    [SerializeField]
    public EventReference reference;
    private float timestamp_next;

    private EventInstance current_instance;

    public void Stop(FMOD.Studio.STOP_MODE mode = FMOD.Studio.STOP_MODE.IMMEDIATE)
    {
        current_instance.stop(mode);
    }

    public void Play(System.Action<EventInstance> modifyInstance = null)
    {
        current_instance = RuntimeManager.CreateInstance(reference);
        modifyInstance?.Invoke(current_instance);
        current_instance.start();
    }

    public void PlayWithTimeLimit(float time_limit, System.Action<EventInstance> modify_instance = null)
    {
        if (Time.time < timestamp_next) return;
        timestamp_next = Time.time + time_limit;
        Play(modify_instance);
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
}