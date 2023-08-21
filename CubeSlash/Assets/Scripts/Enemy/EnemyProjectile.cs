using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class EnemyProjectile : Projectile
{
    [SerializeField] private SpriteRenderer spr;
    [SerializeField] private bool scale_from_zero;
    [SerializeField] private float anim_scale_duration;
    [SerializeField] private float anim_rotation;
    [SerializeField] private AnimationCurve curve_scale;

    public bool rotates;

    protected override void Start()
    {
        base.Start();

        if (rotates)
        {
            AnimateRotation();
        }

        if (scale_from_zero)
        {
            StartCoroutine(AnimateScaleBeginCr());
        }
        else
        {
            StartCoroutine(AnimateScaleCr());
        }
    }

    private IEnumerator AnimateScaleBeginCr()
    {
        var end = Vector3.one * curve_scale.Evaluate(0);
        yield return LerpEnumerator.LocalScale(pivot_animation, 0.2f, Vector3.zero, end);
        StartCoroutine(AnimateScaleCr());
    }

    private IEnumerator AnimateScaleCr()
    {
        while (true)
        {
            yield return LerpEnumerator.Value(anim_scale_duration, f =>
            {
                var t = curve_scale.Evaluate(f);
                pivot_animation.localScale = Vector3.LerpUnclamped(Vector3.zero, Vector3.one, t);
            });
        }
    }

    public Coroutine AnimateRotation()
    {
        return StartCoroutine(AnimateRotationCr());
    }

    private IEnumerator AnimateRotationCr()
    {
        var angle = Random.Range(-anim_rotation, anim_rotation);
        while (true)
        {
            var z = (pivot_animation.eulerAngles.z + angle * Time.deltaTime) % 360f;
            pivot_animation.eulerAngles = pivot_animation.eulerAngles.SetZ(z);
            yield return null;
        }
    }
}