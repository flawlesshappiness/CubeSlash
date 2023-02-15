using UnityEngine;
using FMODUnity;
using System.Collections;
using FMOD.Studio;
using System.Collections.Generic;

public class FMODController : Singleton
{
    public static FMODController Instance { get { return Instance<FMODController>(); } }

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