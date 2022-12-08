using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class BodypartDetonator : Bodypart
{
    [SerializeField] private Transform pivot_animation;

    public Vector3 scale_in;

    private AbilityExplode explode;

    public override void Initialize(Ability ability)
    {
        base.Initialize(ability);
        explode = ability as AbilityExplode;
    }

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
            yield return Lerp.LocalScale(pivot_animation, explode.Delay, scale_in).Curve(EasingCurves.EaseOutQuad);
            yield return Lerp.LocalScale(pivot_animation, 0.05f, Vector3.one);
        }
    }
}