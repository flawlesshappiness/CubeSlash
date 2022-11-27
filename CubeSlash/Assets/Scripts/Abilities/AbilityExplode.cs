using System.Collections;
using System.Linq;
using UnityEngine;

public class AbilityExplode : Ability
{
    [Header("EXPLODE")]
    [SerializeField] private Projectile projectile_split_modifier;
    private ParticleSystem ps_charge;

    // Values
    public float Delay { get; private set; }
    public float Radius { get; private set; }
    public float Width { get; private set; }
    public float Knockback { get; private set; }
    public int Rings { get; private set; }
    public bool DelayPull { get; private set; }

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
        DelayPull = GetBoolValue("DelayPull");

        if (HasModifier(Type.DASH))
        {
            Delay = 0;
        }
    }

    public override void Trigger()
    {
        base.Trigger();
        InUse = true;

        if (HasModifier(Type.SPLIT))
        {
            // Shoot bullet that explodes
            var p = projectile_split_modifier;
            var start = Player.transform.position;
            var dir = Player.MoveDirection;
            var p_instance = AbilitySplit.ShootProjectile(p, start, dir, 1f, 15, (p, k) =>
            {
                TriggerExplode(p.transform.parent, p.transform.position, dir);
            });
            p_instance.OnDeath += () =>
            {
                TriggerExplode(p_instance.transform.parent, p_instance.transform.position, dir);
            };
        }
        else
        {
            TriggerExplode(Player.transform, Player.transform.position, Player.Instance.MoveDirection);
        }
    }

    private void TriggerExplode(Transform parent, Vector3 position, Vector3 direction)
    {
        if (HasModifier(Type.CHARGE))
        {
            Vector3 pos = position;
            for (int i = 0; i < Rings; i++)
            {
                var radius = Radius + Width * i;
                pos = pos + direction.normalized * radius;
                Explode(pos, radius, Width, Knockback);
            }

            InUse = false;
            StartCooldown();
        }
        else
        {
            for (int i = 0; i < Rings; i++)
            {
                StartCoroutine(ExplodeCr(parent, position, Radius + Width * i, Width, 0.2f * i));
            }
        }
    }

    private IEnumerator ExplodeCr(Transform parent, Vector3 position, float radius, float width, float ring_delay = 0)
    {
        yield return new WaitForSeconds(ring_delay);

        var psd = ps_charge.Duplicate()
            .Parent(parent)
            .Position(position)
            .Scale(Vector3.one * radius * 2);

        psd.ps.ModifyMain(m =>
        {
            m.startLifetime = new ParticleSystem.MinMaxCurve { constant = Delay };
        });
        psd.Play();
        yield return new WaitForSeconds(Delay);
        Explode(position, radius, width, Knockback);

        InUse = false;
        StartCooldown();
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
                Player.Instance.KillEnemy(k);
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