using Flawliz.Lerp;
using FMOD;
using System.Collections;
using UnityEngine;

public class ChargeBeam : MonoBehaviour
{
    [SerializeField] private Transform pivot_sprite;
    [SerializeField] private SpriteRenderer spr_beam;
    [SerializeField] private ParticleSystem ps_dust_charge, ps_dust_fire;

    private float length;
    private float width;

    public void SetLength(float length)
    {
        this.length = length;
        UpdateVisual();
    }

    public void SetWidth(float width)
    {
        this.width = width;
        UpdateVisual();
    }

    public void SetDirection(Vector2 dir)
    {
        var angle = Vector3.SignedAngle(dir, Vector3.up, Vector3.back);
        var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = rotation;
    }

    public void SetPosition(Vector3 position)
    {
        transform.position = position;
    }

    public void SetAlpha(float alpha)
    {
        spr_beam.color = spr_beam.color.SetA(alpha);
    }

    public void UpdateVisual()
    {
        var position = new Vector3(0, length * 0.5f);
        var scale = new Vector3(width, length);
        pivot_sprite.localScale = scale;

        UpdatePS(ps_dust_charge);
        UpdatePS(ps_dust_fire);

        ps_dust_charge.ModifyEmission(m => m.rateOverTime = new ParticleSystem.MinMaxCurve { constant = 1 * length * width });
        ps_dust_charge.ModifyEmission(m => m.rateOverDistance = new ParticleSystem.MinMaxCurve { constant = 1 * length * width });
        ps_dust_fire.ModifyEmission(m => m.SetBurst(0, new ParticleSystem.Burst { count = (int)(5 * length * width), cycleCount = 1 }));
        void UpdatePS(ParticleSystem ps) => ps.ModifyShape(m => { m.position = position; m.scale = scale; });
    }

    public CustomCoroutine AnimateShowPreview(bool show, float duration = 0.25f)
    {
        return this.StartCoroutineWithID(Cr());
        IEnumerator Cr()
        {
            ps_dust_charge.SetEmissionEnabled(show);

            var scale_start = new Vector3(0f, length);
            var scale_end = new Vector3(width * 0.5f, length);
            Lerp.LocalScale(pivot_sprite, duration, scale_start, scale_end);

            var start = show ? 0 : spr_beam.color.a;
            var end = show ? 0.25f : 0;
            yield return Lerp.Alpha(spr_beam, duration, start, end);
        }
    }

    public CustomCoroutine AnimateFire()
    {
        SetAlpha(0);
        return this.StartCoroutineWithID(Cr());
        IEnumerator Cr()
        {
            ps_dust_charge.SetEmissionEnabled(false);
            ps_dust_fire.Duplicate()
                .Position(ps_dust_fire.transform.position)
                .Rotation(ps_dust_fire.transform.rotation)
                .Parent(GameController.Instance.world)
                .Play()
                .Destroy(5);

            yield return AnimateFireBeam();
        }
    }

    private CustomCoroutine AnimateFireBeam()
    {
        var beam = Instantiate(pivot_sprite, GameController.Instance.world);
        var spr = beam.GetComponentInChildren<SpriteRenderer>();
        return CoroutineController.Instance.StartCoroutineWithID(Cr(), spr_beam);

        IEnumerator Cr()
        {
            beam.position = pivot_sprite.position;
            beam.rotation = pivot_sprite.rotation;

            var scale = beam.localScale;
            var scale_full = new Vector3(width, length);
            var scale_end = new Vector3(0, length);

            Lerp.Alpha(spr, 0.1f, 1f).Connect(beam.gameObject);
            yield return LerpEnumerator.LocalScale(beam, 0.1f, scale, scale_full);
            yield return LerpEnumerator.LocalScale(beam, 0.25f, scale_end).Curve(EasingCurves.EaseOutQuad);
            Destroy(beam.gameObject);
        }
    }
}