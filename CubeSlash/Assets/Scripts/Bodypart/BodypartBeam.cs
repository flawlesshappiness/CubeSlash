using Flawliz.Lerp;
using UnityEngine;

public class BodypartBeam : Bodypart
{
    [SerializeField] private Transform pivot_animation;

    public Vector3 scale_in;
    public Vector3 scale_out;

    private AbilityCharge charge;
    private Lerp lerp_animate;

    public override void Initialize(Ability ability)
    {
        base.Initialize(ability);
        charge = ability as AbilityCharge;
    }

    private void Update()
    {
        if (charge.Charging)
        {
            var t = charge.GetCharge();
            var target = Vector3.Lerp(Vector3.one, scale_out, t);
            pivot_animation.localScale = Vector3.Lerp(pivot_animation.localScale, target, Time.deltaTime * 5f);
        }
        else if(lerp_animate == null || lerp_animate.Ended)
        {
            pivot_animation.localScale = Vector3.Lerp(pivot_animation.localScale, Vector3.one, Time.deltaTime * 5f);
        }
    }

    protected override void OnAbilityTrigger()
    {
        base.OnAbilityTrigger();
        lerp_animate = AnimateOut();
    }

    private Lerp AnimateOut()
    {
        return Lerp.LocalScale(pivot_animation, 0.15f, pivot_animation.localScale, scale_in);
    }
}