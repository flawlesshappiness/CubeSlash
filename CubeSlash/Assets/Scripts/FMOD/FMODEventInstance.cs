using FMOD.Studio;
using FMODUnity;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using UnityEngine;

public class FMODEventInstance
{
	private bool has_instance;
	private EventInstance instance;
    private FMODEventReference reference;

	private int count_played;
	private float timestamp;

	private static Dictionary<string, float> global_timestamp = new Dictionary<string, float>();

	private List<CustomCoroutine> crs_stopwith = new List<CustomCoroutine>();

	public FMODEventInstance(FMODEventReference reference)
	{
		this.reference = reference;

		if (reference != null && reference.Exists)
		{
			instance = RuntimeManager.CreateInstance(reference.reference);
			has_instance = true;
        }
	}

	public FMODEventInstance Play()
	{
		if (has_instance)
		{
			if (WasPlayedThisFrame()) return this;

            instance.start();
			UpdateTimestamp();
			count_played++;
        }
		return this;
    }

	public void Stop(FMOD.Studio.STOP_MODE mode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT)
	{
		if (has_instance)
		{
            instance.stop(mode);
        }

		crs_stopwith.ForEach(cr => cr.Kill());
		crs_stopwith.Clear();
	}

    public FMODEventInstance SetVolume(float f)
    {
		if (has_instance)
		{
            instance.setVolume(f);
        }
        return this;
    }

	public FMODEventInstance SetPitch(int half_note)
	{
		if (has_instance)
		{
            var delta_note = 1 / 12f;
            var pitch = 1f + delta_note * half_note;
            instance.setPitch(pitch);
        }
		return this;
    }

    private bool WasPlayedThisFrame()
    {
		if (has_instance)
		{
            var has_timestamp = GetTimestamp(out var timestamp);
            return has_timestamp && timestamp == Time.unscaledTime;
        }
		return false;
    }

    private bool GetTimestamp(out float timestamp)
    {
		if (has_instance)
		{
            var key = reference.Info.path;
            var contains = global_timestamp.ContainsKey(key);
            timestamp = contains ? global_timestamp[key] : 0;
            return contains;
        }

		timestamp = 0;
		return false;
    }

	private void UpdateTimestamp()
	{
        timestamp = Time.unscaledTime;

		var key = reference.Info.path;
		if (global_timestamp.ContainsKey(key))
		{
			global_timestamp[key] = timestamp;
		}
		else
		{
			global_timestamp.Add(key, timestamp);
		}
    }

	public FMODEventInstance StopWith(GameObject g)
	{
		var cr = CoroutineController.Instance.StartCoroutineWithID(Cr(), $"{reference.Info.path}_{g.GetInstanceID()}");
		crs_stopwith.Add(cr);
		return this;
		IEnumerator Cr()
		{
			while (g != null && g.activeInHierarchy)
			{
				yield return null;
			}

			Stop();
		}
	}
}