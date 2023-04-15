using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class EnemyVolatileBody : EnemyBody
{
    [Header("VOLATILE")]
    [SerializeField] private ParticleSystem ps_charge;

    public override void Initialize()
    {
        base.Initialize();
        StartCoroutine(AnimateBodyCr());
    }

    private IEnumerator AnimateBodyCr()
    {
        var curve = EasingCurves.EaseInOutSine;
        var scale1 = new Vector3(1.05f, 0.95f, 1f);
        var scale2 = new Vector3(0.95f, 1.05f, 1f);

        while (true)
        {
            yield return LerpEnumerator.LocalScale(pivot_sprite, 0.4f, scale1).Curve(curve);
            yield return LerpEnumerator.LocalScale(pivot_sprite, 0.4f, scale2).Curve(curve);
        }
    }

    public void PlayChargePS(float size)
    {
        ps_charge.ModifyMain(m => m.startSize = new ParticleSystem.MinMaxCurve(size));
        ps_charge.Play();
    }
}