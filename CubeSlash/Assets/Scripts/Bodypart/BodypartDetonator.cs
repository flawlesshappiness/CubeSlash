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
        explode.onChargeStart += OnChargeStart;
        explode.onExplode += OnExplode;
    }

    private void OnDisable()
    {
        explode.onChargeStart -= OnChargeStart;
        explode.onExplode -= OnExplode;
    }

    private void OnChargeStart()
    {
        this.StartCoroutineWithID(Cr(), GetID());
        IEnumerator Cr()
        {
            yield return Lerp.LocalScale(pivot_animation, explode.ChargeTime, scale_in).Curve(EasingCurves.EaseOutQuad);
        }
    }

    private void OnExplode()
    {
        this.StartCoroutineWithID(Cr(), GetID());
        IEnumerator Cr()
        {
            yield return Lerp.LocalScale(pivot_animation, 0.05f, Vector3.one);
        }
    }

    private string GetID() => $"animate_{GetInstanceID()}";
}