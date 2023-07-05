using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class UIFloatingIdleAnimation : MonoBehaviour
{
    [SerializeField] private Transform pivot;

    public float position_frequency = 0.4f;
    public float position_scale = 10f;
    public float scale_min = 0.95f;
    public float scale_max = 1.0f;
    public float scale_duration_min = 2.0f;
    public float scale_duration_max = 3.0f;

    private void Start()
    {
        StartCoroutine(AnimateIdleScaleCr());
        StartCoroutine(AnimateIdlePositionCr());
    }

    private IEnumerator AnimateIdleScaleCr()
    {
        var start = Vector3.one * scale_min;
        var end = Vector3.one * scale_max;
        while (true)
        {
            var duration = Random.Range(scale_duration_min, scale_duration_max);
            yield return LerpEnumerator.LocalScale(pivot, duration, start, end).Curve(EasingCurves.EaseInOutSine).UnscaledTime();
            yield return LerpEnumerator.LocalScale(pivot, duration, end, start).Curve(EasingCurves.EaseInOutSine).UnscaledTime();
        }
    }

    private IEnumerator AnimateIdlePositionCr()
    {
        var offset = new Vector2(Random.Range(0f, 100f), Random.Range(0f, 100f));
        var scale = position_scale;
        var freq = position_frequency;
        while (true)
        {
            var time = Time.time * freq;
            var x = Mathf.PerlinNoise(time + offset.x, 0) * 2f - 1f;
            var y = Mathf.PerlinNoise(0f, time + offset.y) * 2f - 1f;
            pivot.localPosition = new Vector2(x, y) * scale;
            yield return null;
        }
    }
}