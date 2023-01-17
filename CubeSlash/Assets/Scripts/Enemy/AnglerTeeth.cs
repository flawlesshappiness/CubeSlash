using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class AnglerTeeth : MonoBehaviour
{
    [SerializeField] private Transform pivot_animation;
    [SerializeField] private SpriteRenderer teeth_upper, teeth_lower, eye_left, eye_right;
    [SerializeField] private ParticleSystem ps_bite;
    [SerializeField] private FMODEventReference sfx_bite;
    [SerializeField] private FMODEventReference sfx_bite_charge_long;
    [SerializeField] private FMODEventReference sfx_bite_charge_short;

    private const float Y_MOUTH_CLOSED = 0.05f;
    private const float Y_MOUTH_OPEN = 0.2f;

    private void OnDisable()
    {
        sfx_bite_charge_long.Stop();
        sfx_bite_charge_short.Stop();
    }

    public void SetHidden()
    {
        teeth_upper.SetAlpha(0);
        teeth_lower.SetAlpha(0);
        eye_left.SetAlpha(0);
        eye_right.SetAlpha(0);
    }

    public Coroutine AnimateAppear(float duration, float size)
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            var scale = Vector3.one * size;
            var color_start = Color.white.SetA(0);
            var color_end = Color.white;
            var curve = EasingCurves.EaseOutQuad;
            yield return LerpEnumerator.Value(duration, f =>
            {
                var t = curve.Evaluate(f);
                pivot_animation.localScale = Vector3.Lerp(Vector3.zero, scale, t);

                teeth_upper.color = Color.Lerp(color_start, color_end, f);
                teeth_lower.color = Color.Lerp(color_start, color_end, f);
                eye_left.color = Color.Lerp(color_start, color_end, f);
                eye_right.color = Color.Lerp(color_start, color_end, f);
            });
        }
    }

    public Coroutine AnimateHide(float duration)
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            var scale = pivot_animation.localScale;
            var color_start = Color.white;
            var color_end = Color.white.SetA(0);
            var curve = EasingCurves.EaseOutQuad;
            yield return LerpEnumerator.Value(duration, f =>
            {
                var t = curve.Evaluate(f);
                var scale_end = Vector3.one * 0.25f;
                pivot_animation.localScale = Vector3.Lerp(scale, scale_end, t);

                teeth_upper.color = Color.Lerp(color_start, color_end, f);
                teeth_lower.color = Color.Lerp(color_start, color_end, f);
                eye_left.color = Color.Lerp(color_start, color_end, f);
                eye_right.color = Color.Lerp(color_start, color_end, f);
            });
        }
    }

    public Coroutine AnimateSmallBite(float duration_open)
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            pivot_animation.eulerAngles = new Vector3(0, 0, Random.Range(-30f, 30f));

            sfx_bite_charge_short.Play();

            var teeth_closed = new Vector3(0, Y_MOUTH_CLOSED);
            var teeth_open = new Vector3(0, Y_MOUTH_OPEN);
            yield return LerpEnumerator.Value(duration_open, f =>
            {
                teeth_upper.transform.localPosition = Vector3.LerpUnclamped(teeth_closed, teeth_open, f);
                teeth_lower.transform.localPosition = Vector3.LerpUnclamped(-teeth_closed, -teeth_open, f);
            });

            var curve_close = EasingCurves.EaseInBack;
            yield return LerpEnumerator.Value(0.15f, f =>
            {
                var t = curve_close.Evaluate(f);
                teeth_upper.transform.localPosition = Vector3.LerpUnclamped(teeth_open, teeth_closed, t);
                teeth_lower.transform.localPosition = Vector3.LerpUnclamped(-teeth_open, -teeth_closed, t);
            });

            ps_bite.Play();
            sfx_bite.Play();
            sfx_bite_charge_short.Stop();
        }
    }

    public Coroutine AnimateBigBite(float duration_open)
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            pivot_animation.eulerAngles = new Vector3(0, 0, Random.Range(-30f, 30f));

            sfx_bite_charge_long.Play();

            var teeth_closed = new Vector3(0, Y_MOUTH_CLOSED);
            var teeth_open = new Vector3(0, Y_MOUTH_OPEN);
            yield return LerpEnumerator.Value(duration_open, f =>
            {
                var noise_pos_upper = Random.insideUnitCircle.ToVector3() * Mathf.Lerp(0f, 1f, f) * 0.01f;
                var noise_pos_lower = Random.insideUnitCircle.ToVector3() * Mathf.Lerp(0f, 1f, f) * 0.01f;

                teeth_upper.transform.localPosition = Vector3.LerpUnclamped(teeth_closed, teeth_open, f) + noise_pos_upper;
                teeth_lower.transform.localPosition = Vector3.LerpUnclamped(-teeth_closed, -teeth_open, f) + noise_pos_lower;

            });

            var curve_close = EasingCurves.EaseInBack;
            yield return LerpEnumerator.Value(0.15f, f =>
            {
                var t = curve_close.Evaluate(f);
                teeth_upper.transform.localPosition = Vector3.LerpUnclamped(teeth_open, teeth_closed, t);
                teeth_lower.transform.localPosition = Vector3.LerpUnclamped(-teeth_open, -teeth_closed, t);
            });

            ps_bite.Play();
            sfx_bite.Play();
            sfx_bite_charge_long.Stop();
        }
    }
}