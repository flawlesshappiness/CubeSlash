using System;
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
    public float Knockback { get; private set; }
    public int Rings { get; private set; }
    public bool DelayPull { get; private set; }
    public bool ChainExplode { get; private set; }

    private bool charge_sfx_has_played = false;

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
        Knockback = GetFloatValue("Knockback");
        Rings = GetIntValue("Rings");
        DelayPull = GetBoolValue("DelayPull");
        ChainExplode = GetBoolValue("ChainExplode");

        if (HasModifier(Type.DASH))
        {
            Delay = 0;
        }
    }

    public override void Trigger()
    {
        if (InUse) return;
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
                var position = p.transform.position;
                TriggerExplode(p.transform.parent, () => position, dir);
            });
            p_instance.onDeath += () =>
            {
                var position = p_instance.transform.position;
                TriggerExplode(p_instance.transform.parent, () => position, dir);
            };
        }
        else
        {
            TriggerExplode(Player.transform, () => Player.transform.position, Player.Instance.MoveDirection);
        }
    }

    private void TriggerExplode(Transform parent, Func<Vector3> getPosition, Vector3 direction)
    {
        charge_sfx_has_played = false;

        if (HasModifier(Type.CHARGE))
        {
            Vector3 pos = getPosition();
            for (int i = 0; i < Rings; i++)
            {
                var r = Radius * (1 + 0.15f * i);
                pos = pos + direction.normalized * r;
                Explode(pos, r * (1 + 0.15f * i), Knockback);
            }

            InUse = false;
            StartCooldown();
        }
        else
        {
            for (int i = 0; i < Rings; i++)
            {
                var r = Radius * (1 + 0.5f * i);
                StartCoroutine(ExplodeCr(parent, getPosition, r, (Delay * 0.5f) * i));
            }
        }
    }

    private IEnumerator ExplodeCr(Transform parent, Func<Vector3> getPosition, float radius, float ring_delay = 0)
    {
        yield return new WaitForSeconds(ring_delay);

        var psd = ps_charge.Duplicate()
            .Parent(parent)
            .Position(getPosition())
            .Scale(Vector3.one * radius * 2);

        psd.ps.ModifyMain(m =>
        {
            m.startLifetime = new ParticleSystem.MinMaxCurve { constant = Delay };
        });
        psd.Play();

        var sfx = FMODEventReferenceDatabase.Load().sfx_explode_charge;
        if (!charge_sfx_has_played)
        {
            charge_sfx_has_played = true;
            sfx.Play();
        }

        yield return WaitForDelay(Delay, getPosition());

        sfx.Stop();

        Explode(getPosition(), radius, Knockback, OnHit);

        InUse = false;
        StartCooldown();
    }

    private void OnHit(IKillable k)
    {
        var position = k.GetPosition();
        if (ChainExplode)
        {
            StartCoroutine(ExplodeCr(position));
        }

        IEnumerator ExplodeCr(Vector3 position)
        {
            yield return new WaitForSeconds(0.25f);
            Explode(position, Radius * 0.75f, Knockback * 0.5f);
        }
    }

    public static void Explode(Vector3 position, float radius, float force, System.Action<IKillable> onHit = null)
    {
        var hits = Physics2D.OverlapCircleAll(position, radius);
        foreach(var hit in hits)
        {
            var k = hit.GetComponentInParent<IKillable>();
            if (k == null) continue;

            if (k.CanKill())
            {
                onHit?.Invoke(k);
                Player.Instance.KillEnemy(k);
            }
        }

        // Knockback
        if(force > 0)
        {
            Player.PushEnemiesInArea(position, radius * 3, force);
        }

        // Sfx
        var sfx_explode = FMODEventReferenceDatabase.Load().sfx_explode_explode;
        FMODController.Instance.PlayWithLimitDelay(sfx_explode);

        // Fx
        var template_explosion = Resources.Load<GameObject>("Particles/ExplosionEffect");
        var explosion = Instantiate(template_explosion, GameController.Instance.world);
        explosion.transform.position = position;
        explosion.transform.localScale = Vector3.one * radius * 2;
        Destroy(explosion, 2f);

        var ps_explode = Resources.Load<ParticleSystem>("Particles/ps_explode");
        ps_explode.Duplicate()
            .Scale(Vector3.one * radius * 2)
            .Position(position)
            .Play();
    }

    private IEnumerator WaitForDelay(float duration, Vector3 position)
    {
        if (DelayPull)
        {
            var r_max = Radius * 2;
            var r_min = Radius * 0.75f;
            var force_min = 5f;
            var force_max = 25f;
            var time_end = Time.time + duration;
            while(Time.time < time_end)
            {
                var enemies = Physics2D.OverlapCircleAll(position, r_max)
                    .Select(hit => hit.GetComponentInParent<Enemy>())
                    .Where(hit => hit != null)
                    .Distinct();

                foreach(var e in enemies)
                {
                    var dir = position - e.GetPosition();
                    var t = (dir.magnitude - r_min) / (r_max - r_min);
                    var force = Mathf.LerpUnclamped(force_min, force_max, t);
                    var velocity = dir.normalized * force;
                    e.Rigidbody.AddForce(velocity);
                }

                yield return null;
            }
        }
        else
        {
            yield return new WaitForSeconds(duration);
        }
    }
}