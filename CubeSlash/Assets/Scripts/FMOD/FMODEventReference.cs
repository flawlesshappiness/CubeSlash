using UnityEngine;
using FMODUnity;
using FMOD.Studio;

[System.Serializable]
public class FMODEventReference
{
    [SerializeField]
    public EventReference reference;

    public void Play(System.Action<EventInstance> modifyInstance = null)
    {
        var instance = RuntimeManager.CreateInstance(reference);
        modifyInstance?.Invoke(instance);
        instance.start();
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