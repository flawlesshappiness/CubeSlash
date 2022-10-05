using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class AbilityExplode : Ability
{
    [Header("EXPLODE")]
    [SerializeField] private ParticleSystem ps_charge;
    [SerializeField] private ParticleSystem ps_explode;
    [SerializeField] private ParticleSystem ps_explode_point;

    // Values
    public float Delay { get; private set; }
    public float Radius { get; private set; }
    public float Width { get; private set; }
    public float Knockback { get; private set; }
    public float Rings { get; private set; }

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();
    }

    public override void OnValuesApplied()
    {
        base.OnValuesApplied();
        Delay = GetFloatValue("Delay");
        Radius = GetFloatValue("Radius");
        Width = GetFloatValue("Width");
        Knockback = GetFloatValue("Knockback");
        Rings = GetFloatValue("Rings");
    }

    public override void Pressed()
    {
        base.Pressed();
    }

    public override void Released()
    {
        base.Released();
        for (int i = 0; i < Rings; i++)
        {
            StartCoroutine(ExplodeCr(Radius + Width * i, Width, 0.2f * i));
        }
    }

    private IEnumerator ExplodeCr(float radius, float width, float delay = 0)
    {
        yield return new WaitForSeconds(delay);

        ps_charge.ModifyMain(m =>
        {
            m.startLifetime = new ParticleSystem.MinMaxCurve { constant = Delay };
        });
        ps_charge.transform.localScale = Vector3.one * radius * 2;
        ps_charge.Play();
        yield return new WaitForSeconds(Delay);
        Explode(Player.transform.position, radius, width);
    }

    private void Explode(Vector3 position, float radius, float width)
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
        Player.PushEnemiesInArea(position, radius * 3, Knockback);

        // Fx
        var count_points = (int)(radius * 10);
        var points = CircleHelper.Points(radius, count_points);
        foreach(var point in points)
        {
            var ps = ps_explode_point.Duplicate()
                .Position(position + point.ToVector3())
                .Scale(Vector3.one * width)
                .Play()
                .Destroy(5);
        }

        ps_explode.transform.localScale = Vector3.one * radius * 3;
        ps_explode.Play();
    }
}