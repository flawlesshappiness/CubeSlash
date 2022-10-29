using UnityEngine;
using FMODUnity;

public class FMODController : Singleton
{
    public static FMODController Instance { get { return Instance<FMODController>(); } }

    public void PlayAudio(string path)
    {
        RuntimeManager.PlayOneShot(path);
    }
}