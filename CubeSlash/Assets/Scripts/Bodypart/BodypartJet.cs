using System.Collections;
using UnityEngine;
using Flawliz.Lerp;

public class BodypartJet : Bodypart
{
    [SerializeField] private Transform pivot_animation;

    public Vector3 scale_in;
    public Vector3 scale_out;

    private Lerp lerp_animate;

    private void Update()
    {
        if (Ability.IsOnCooldown)
        {
            if(lerp_animate != null)
            {
                lerp_animate.Kill();
                lerp_animate = null;
            }

            var p = Ability.CooldownPercentage;
            var target = Vector3.Lerp(scale_out, scale_in, p);
            pivot_animation.localScale = Vector3.Lerp(pivot_animation.localScale, target, Time.deltaTime * 5f);
        }
        else
        {
            pivot_animation.localScale = Vector3.Lerp(pivot_animation.localScale, scale_in, Time.deltaTime * 5f);
        }
    }

    protected override void OnAbilityTrigger()
    {
        base.OnAbilityTrigger();
        lerp_animate = AnimateOut();
    }

    private Lerp AnimateOut()
    {
        return Lerp.LocalScale(pivot_animation, 0.25f, pivot_animation.localScale, scale_out);
    }
}