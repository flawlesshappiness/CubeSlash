using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class BodypartGuns : Bodypart
{
    [SerializeField] private Transform pivot_animation;

    public Vector3 scale_in;

    protected override void OnAbilityTrigger()
    {
        base.OnAbilityTrigger();
        Animate();
    }

    private CustomCoroutine Animate()
    {
        return this.StartCoroutineWithID(Cr(), $"animate_{GetInstanceID()}");
        IEnumerator Cr()
        {
            var half_cd = Ability.GetBaseCooldown() * 0.5f;
            var time = Mathf.Min(half_cd, 0.15f);
            yield return Lerp.LocalScale(pivot_animation, time, scale_in).Curve(EasingCurves.EaseOutQuad);
            yield return Lerp.LocalScale(pivot_animation, time, Vector3.one).Curve(EasingCurves.EaseInOutQuad);
        }
    }
}