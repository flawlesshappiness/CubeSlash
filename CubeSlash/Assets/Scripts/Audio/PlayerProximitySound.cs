using System.Collections;
using UnityEngine;

public class PlayerProximitySound : MonoBehaviour
{
    public Vector2 volume_range;
    public Vector2 distance_range;
    public AnimationCurve curve_volume;
    public SoundEffectType type_sfx;

    private FMODEventInstance sfx;

    private void OnEnable()
    {
        StartCoroutine(StartSoundCr());
    }

    IEnumerator StartSoundCr()
    {
        yield return new WaitForSeconds(0.5f);
        sfx = SoundController.Instance.CreateInstance(type_sfx);
        sfx.Play();
    }

    private void OnDisable()
    {
        if(sfx != null)
        {
            sfx.Stop();
        }
    }

    private void Update()
    {
        var dist = Vector3.Distance(transform.position, Player.Instance.transform.position);
        var d_min = distance_range.x;
        var d_max = distance_range.y;
        var t = 1f - Mathf.Clamp((dist - d_min) / (d_max - d_min), 0f, 1f);
        var v = Mathf.Lerp(volume_range.x, volume_range.y, curve_volume.Evaluate(t));
        sfx.SetVolume(v);
    }
}