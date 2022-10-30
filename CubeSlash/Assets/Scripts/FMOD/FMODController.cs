using UnityEngine;
using FMODUnity;
using System.Collections;
using FMOD.Studio;

public class FMODController : Singleton
{
    public static FMODController Instance { get { return Instance<FMODController>(); } }

    public void Play(string path)
    {
        RuntimeManager.PlayOneShot(path);
    }

    public void PlayWithDelay(FMODEventReference reference, float delay, System.Action<EventInstance> modify_instance = null)
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(delay);
            reference.Play(modify_instance);
        }
    }
}