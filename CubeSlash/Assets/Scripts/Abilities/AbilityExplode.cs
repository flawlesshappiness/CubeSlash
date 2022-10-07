using System.Collections;
using System.Linq;
using UnityEngine;

public class AbilityExplode : Ability
{
    private ParticleSystem ps_charge;

    // Values
    public float Delay { get; private set; }
    public float Radius { get; private set; }
    public float Width { get; private set; }
    public float Knockback { get; private set; }
    public int Rings { get; private set; }

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();
        ps_charge = Resources.Load<ParticleSystem>("Particles/ps_explode_charge");
    }

    public override void OnValuesApplied()
    {
        base.OnValuesApplied();
        Delay = GetFloatValue("Delay");
        Radius = GetFloatValue("Radius");
        Width = GetFloatValue("Width");
        Knockback = GetFloatValue("Knockback");
        Rings = GetIntValue("Rings");
    }

    public override void Trigger()
    {
        base.Trigger();
        for (int i = 0; i < Rings; i++)
        {
            StartCoroutine(ExplodeCr(Radius + Width * i, Width, 0.2f * i));
        }
    }

    private IEnumerator ExplodeCr(float radius, float width, float delay = 0)
    {
        yield return new WaitForSeconds(delay);

        var psd = ps_charge.Duplicate()
            .Parent(transform)
            .Position(transform.position)
            .Scale(Vector3.one * radius * 2);

        psd.ps.ModifyMain(m =>
        {
            m.startLifetime = new ParticleSystem.MinMaxCurve { constant = Delay };
        });
        psd.Play();
        yield return new WaitForSeconds(Delay);
        Explode(Player.transform.position, radius, width, Knockback);
    }

    public static void Explode(Vector3 position, float radius, float width, float force)
    {
        var wh = width * 0.5f;
        var hits = Physics2D.OverlapCircleAll(position, radius + wh);
        foreach(var hit in hits)
        {
            var ray_hit = Physics2D.RaycastAll(position, hit.transform.position - position)
                .FirstOrDefault(h => h.collider == hit);
            var dist = ray_hit.distance;
            if ((dist - radius).Abs() > wh) continue;

            var k = hit.GetComponentInParent<IKillable>();
            if (k == null) continue;

            if (k.CanKill())
            {
                k.Kill();
            }
        }

        // Knockback
        Player.PushEnemiesInArea(position, radius * 3, force);

        // Fx
        var ps_explode_point = Resources.Load<ParticleSystem>("Particles/ps_explode_point");
        var count_points = (int)(radius * 5);
        var points = CircleHelper.Points(radius, count_points);
        foreach(var point in points)
        {
            ps_explode_point.Duplicate()
                .Position(position + point.ToVector3())
                .Scale(Vector3.one * width)
                .Play()
                .Destroy(5);
        }

        var ps_explode = Resources.Load<ParticleSystem>("Particles/ps_explode");
        ps_explode.Duplicate()
            .Scale(Vector3.one * radius * 3)
            .Position(position)
            .Play();
    }
}