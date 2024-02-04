using FMODUnity;
using UnityEngine;

[System.Serializable]
public class FMODEventReference
{
    [SerializeField] public EventReference reference;

    public FMODEventReferenceInfo Info { get { return _info ?? GetInfo(); } }
    private FMODEventReferenceInfo _info;

    public bool Exists { get { return Info.has_description; } }

    public FMODEventReferenceInfo GetInfo()
    {
        _info = _info ?? new FMODEventReferenceInfo(reference);
        return _info;
    }

    public FMODEventInstance CreateInstance()
    {
        return new FMODEventInstance(this);
    }
}