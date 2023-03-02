using System.Collections;
using System.Linq;
using UnityEngine;

public class AbilityExplode : Ability
{
    [Header("EXPLODE")]
    [SerializeField] private Projectile projectile_split_modifier;
    [SerializeField] private Projectile projectile_fragment;

    // Values
    public float Cooldown { get; private set; }
    public float Delay { get; private set; }
    public float Radius { get; private set; }
    public float Knockback { get; private set; }
    public bool DelayPull { get; private set; }
    public bool ChainExplode { get; private set; }
    public bool HasFragments { get; private set; }
    public bool HasProjectile { get; private set; }
    public bool IsFront { get; private set; }
    public bool DelayInvulnerable { get; private set; }

    private const float DELAY = 1.5f;
    private const float RADIUS = 4f;
    private const float FORCE = 200f;

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();
    }

    public override void OnValuesUpdated()
    {
        base.OnValuesUpdated();
        Cooldown = GetFloatValue(StatID.explode_cooldown_flat) * GetFloatValue(StatID.explode_cooldown_perc);
        Delay = DELAY * GetFloatValue(StatID.explode_delay_perc);
        Radius = RADIUS * GetFloatValue(StatID.explode_radius_perc);
        Knockback = FORCE * GetFloatValue(StatID.explode_force_knock_enemy_perc);
        DelayPull = GetBoolValue(StatID.explode_delay_pull);
        ChainExplode = GetBoolValue(StatID.explode_chain);
        HasFragments = GetBoolValue(StatID.explode_fragments);
        HasProjectile = GetBoolValue(StatID.explode_projectile);
        IsFront = GetBoolValue(StatID.explode_front);
        DelayInvulnerable = GetBoolValue(StatID.explode_invulnerable);
    }

    public override float GetBaseCooldown() => Cooldown;

    public override void Trigger()
    {
        if (InUse) return;
        base.Trigger();
        InUse = true;
        Player.AbilityLock.AddLock(nameof(AbilityExplode));

        if (HasProjectile)
        {
            // Shoot bullet that explodes
            var p = projectile_split_modifier;
            var start = Player.transform.position;
            var dir = Player.MoveDirection;
            var speed = 15;
            var velocity = dir * speed;
            var p_instance = ProjectileController.Instance.ShootPlayerProjectile(new ProjectileController.PlayerShootInfo
            {
                prefab = p,
                position_start = start,
                velocity = velocity,
                onKill = ProjectileExplode
            });

            p_instance.Lifetime = Calculator.DST_Time(7f, speed);
            p_instance.onDeath += () => ProjectileExplode(p_instance);
        }
        else
        {
            TriggerExplode(Player.transform, () => Player.transform.position, Player.Instance.MoveDirection);
        }

        void ProjectileExplode(Projectile p, IKillable k = null)
        {
            var position = p.transform.position;
            TriggerExplode(p.transform.parent, () => position, p.transform.up);
        }
    }

    private void TriggerExplode(Transform parent, System.Func<Vector3> getPosition, Vector3 direction)
    {
        if (IsFront)
        {
            Vector3 pos = getPosition() + direction.normalized * Radius;
            Explode(pos, Radius, Knockback);
            OnExplode(pos);

            InUse = false;
            Player.AbilityLock.RemoveLock(nameof(AbilityExplode));
            StartCooldown();
        }
        else
        {
            ExplodeWithDelay();
        }

        void ExplodeWithDelay()
        {
            InUse = true;
            Player.Instance.AbilityLock.AddLock(nameof(AbilityExplode));

            if (DelayInvulnerable)
            {
                Player.Instance.InvincibilityLock.AddLock(nameof(AbilityExplode));
            }

            StartCoroutine(ExplodeCr(new ChargeInfo
            {
                parent = parent,
                radius = Radius,
                delay = Delay,
                force = Knockback,
                pull_enemies = DelayPull,
                getPosition = getPosition,
                play_charge_sfx = true,
                onHit = OnHit,
                onExplode = OnExplode,
            }));

            InUse = false;
            StartCooldown();
            Player.Instance.AbilityLock.RemoveLock(nameof(AbilityExplode));
        }

        void OnExplode(Vector3 position)
        {
            if (HasFragments)
            {
                var fragments = AbilityMines.ShootFragments(position, projectile_fragment, 10, 20, 0.75f);
                foreach(var fragment in fragments)
                {
                    fragment.Lifetime = Random.Range(0.5f, 1f);
                }
            }

            if (DelayInvulnerable)
            {
                Player.Instance.InvincibilityLock.RemoveLock(nameof(AbilityExplode));
            }
        }
    }

    public class ChargeInfo
    {
        public Transform parent;
        public float radius;
        public float delay;
        public float force;
        public bool pull_enemies;
        public bool play_charge_sfx;
        public System.Func<Vector3> getPosition;
        public System.Action<IKillable> onHit;
        public System.Action<Vector3> onExplode;
    }

    public static IEnumerator ExplodeCr(ChargeInfo info)
    {
        var parent = info.parent;
        var radius = info.radius;
        var delay = info.delay;
        var force = info.force;
        var pull = info.pull_enemies;
        var onHit = info.onHit;
        var getPosition = info.getPosition;

        CreateChargeEffect(parent, getPosition(), radius, delay);

        var sfx_charge = SoundController.Instance.CreateInstance(SoundEffectType.sfx_explode_charge);
        if (info.play_charge_sfx) sfx_charge.Play();

        yield return WaitForDelay(delay, radius, getPosition(), pull);

        sfx_charge.Stop();

        var position = getPosition();
        Explode(position, radius, force, onHit);
        info.onExplode?.Invoke(position);
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
            Explode(position, Radius * 0.75f, Knockback * 0.1f);
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

        // Fx
        CreateExplodeEffect(position, radius);
    }

    private static IEnumerator WaitForDelay(float duration, float radius, Vector3 position, bool pull_enemies)
    {
        if (pull_enemies)
        {
            var r_max = radius * 3;
            var r_min = radius * 0.75f;
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

    public static void CreateChargeEffect(Transform parent, Vector3 position, float radius, float duration)
    {
        var ps = Resources.Load<ParticleSystem>("Particles/ps_explode_charge");
        var psd = ps.Duplicate()
            .Parent(parent)
            .Position(position)
            .Scale(Vector3.one * radius * 2);

        psd.ps.ModifyMain(m =>
        {
            m.startLifetime = new ParticleSystem.MinMaxCurve { constant = duration };
        });
        psd.Play();
    }

    public static void CreateExplodeEffect(Vector3 position, float radius, Color? color = null)
    {
        Color actual_color = color ?? Color.white;

        var template_explosion = Resources.Load<ExplosionEffect>("Particles/ExplosionEffect");
        var explosion = Instantiate(template_explosion, GameController.Instance.world);
        explosion.transform.position = position;
        explosion.transform.localScale = Vector3.one * radius * 2;
        explosion.SetColor(actual_color);
        Destroy(explosion, 2f);

        var ps_explode = Resources.Load<ParticleSystem>("Particles/ps_explode");
        ps_explode.Duplicate()
            .Scale(Vector3.one * radius * 2)
            .Position(position)
            .Play();

        // Sfx
        SoundController.Instance.PlayGroup(SoundEffectType.sfx_explode_release);
    }
}