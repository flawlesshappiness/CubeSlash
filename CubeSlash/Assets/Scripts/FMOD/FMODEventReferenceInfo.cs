using FMOD.Studio;
using FMODUnity;

public class FMODEventReferenceInfo
{
    public EventDescription Description { get; private set; }
    public bool has_description;
    public string path;
    public int length;

    public FMODEventReferenceInfo(EventReference reference)
    {
        try
        {
            Description = RuntimeManager.GetEventDescription(reference);
            Description.getPath(out path);
            Description.getLength(out length);
            has_description = true;
        }
        catch (System.Exception e)
        {
            LogController.Instance.LogMessage(e.Message);
        }
    }
}