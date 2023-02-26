using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityDash : Ability
{
    private bool Dashing { get; set; }

    // Values
    public float Cooldown { get; private set; }
    public float Distance { get; private set; }
    public float Speed { get; private set; }
    public float RadiusDamage { get; private set; }
    public float RadiusKnockback { get; private set; }
    public float KnockbackForceEnemy { get; private set; }
    public float KnockbackForceSelf { get; private set; }
    public float CooldownPercReducOnHit { get; private set; }
    public bool TrailEnabled { get; private set; }
    public int ShockwaveCount { get; private set; }
    public float ShockwaveSpeed { get; private set; }
    public float ShockwaveSize { get; private set; }
    public float ShockwaveDistance { get; private set; }
    public bool ShockwaveBounce { get; private set; }
    public bool OnlyShockwave { get; private set; }
    public bool ExplodeOnImpact { get; private set; }
    public bool ShockwaveLinger { get; private set; }

    [Header("DASH")]
    [SerializeField] private Projectile prefab_shockwave;
    [SerializeField] private ParticleSystem ps_bubbles, ps_trail, ps_starpower;
    [SerializeField] private AnimationCurve ac_push_enemies;

    private Coroutine cr_dash;
    private Vector3 dir_dash;

    private const float DISTANCE = 3;
    private const float SPEED = 30;
    private const float RADIUS_DAMAGE = 1.5f;
    private const float RADIUS_FORCE = 12f;
    private const float FORCE = 200;
    private const float FORCE_SELF = 750;
    private const float RIPPLE_SPEED = 25f;
    private const float RIPPLE_SIZE = 2f;
    private const float RIPPLE_DISTANCE = 7f;

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();
        Player.onTriggerEnter += OnImpact;

        ps_trail.SetEmissionEnabled(false);
    }

    private void OnDestroy()
    {
        Player.onTriggerEnter -= OnImpact;
    }

    public override void OnValuesUpdated()
    {
        base.OnValuesUpdated();

        Cooldown = GetFloatValue(StatID.dash_cooldown_flat) * GetFloatValue(StatID.dash_cooldown_perc);
        Distance = DISTANCE * GetFloatValue(StatID.dash_distance_perc);
        Speed = SPEED * GetFloatValue(StatID.dash_speed_perc);
        RadiusDamage = RADIUS_DAMAGE * GetFloatValue(StatID.dash_radius_impact_perc);
        RadiusKnockback = RADIUS_FORCE * GetFloatValue(StatID.dash_radius_knock_enemy_perc);
        KnockbackForceEnemy = FORCE * GetFloatValue(StatID.dash_force_knock_enemy_perc);
        KnockbackForceSelf = FORCE_SELF * GetFloatValue(StatID.dash_force_knock_self_perc);
        CooldownPercReducOnHit = GetFloatValue(StatID.dash_hit_cooldown_reduc);
        TrailEnabled = GetBoolValue(StatID.dash_trail);
        ShockwaveCount = GetIntValue(StatID.dash_shockwave_count);
        ShockwaveSpeed = RIPPLE_SPEED * GetFloatValue(StatID.dash_shockwave_speed_perc);
        ShockwaveSize = RIPPLE_SIZE * GetFloatValue(StatID.dash_shockwave_size_perc);
        ShockwaveDistance = RIPPLE_DISTANCE * GetFloatValue(StatID.dash_shockwave_distance_perc);
        ShockwaveBounce = GetBoolValue(StatID.dash_shockwave_bounce);
        OnlyShockwave = GetBoolValue(StatID.dash_shockwave_only);
        ExplodeOnImpact = GetBoolValue(StatID.dash_impact_explode);
        ShockwaveLinger = GetBoolValue(StatID.dash_shockwave_linger);
    }

    public override float GetBaseCooldown() => Cooldown;

    public override void Trigger()
    {
        base.Trigger();
        if (Dashing) return;
        StartDashing();
    }

    private void StartDashing()
    {
        if (OnlyShockwave)
        {
            ShootShockwaves(Player.MoveDirection);
        }
        else
        {
            Dashing = true;
            Player.MovementLock.AddLock(nameof(AbilityDash));
            Player.DragLock.AddLock(nameof(AbilityDash));
            Player.InvincibilityLock.AddLock(nameof(AbilityDash));
            cr_dash = StartCoroutine(DashCr(Player.MoveDirection));
        }
    }

    private IEnumerator DashCr(Vector3 direction)
    {
        IKillable victim = null;
        var velocity = direction * Speed;
        var pos_origin = transform.position;
        dir_dash = direction;

        SoundController.Instance.Play(SoundEffectType.sfx_dash_start);

        ps_trail.SetEmissionEnabled(true);

        ps_bubbles.Duplicate()
            .Parent(GameController.Instance.world)
            .Position(Player.transform.position)
            .Rotation(Player.Body.transform.rotation)
            .Play()
            .Destroy(5);

        while (victim == null && Vector3.Distance(Player.transform.position, pos_origin) < Distance)
        {
            Player.Rigidbody.velocity = velocity;
            yield return new WaitForFixedUpdate();
        }

        EndDash(victim);
    }

    private void OnImpact(Collider2D c)
    {
        if (!Dashing) return;
        var k = c.GetComponentInParent<IKillable>();
        if (k == null) return;
        HitEnemiesArea(k.GetPosition(), RadiusDamage);
        EndDash(k);
    }

    private void EndDash(IKillable victim)
    {
        StopCoroutine(cr_dash);
        cr_dash = null;

        ps_trail.SetEmissionEnabled(false);

        var hit_anything = victim != null;
        if (!hit_anything)
        {
            var radius = Player.Instance.PlayerBody.Size * 1.5f;
            hit_anything = HitEnemiesArea(Player.transform.position, radius) > 0; // Try to hit something
        }

        if (hit_anything)
        {
            KnockbackSelf();
            SoundController.Instance.Play(SoundEffectType.sfx_dash_impact);
            Player.PushEnemiesInArea(Player.transform.position, RadiusKnockback, KnockbackForceEnemy, ac_push_enemies);
            ShootShockwaves(dir_dash);
        }

        Dashing = false;
        Player.MovementLock.RemoveLock(nameof(AbilityDash));
        Player.DragLock.RemoveLock(nameof(AbilityDash));

        var cd = hit_anything ? Cooldown * CooldownPercReducOnHit : Cooldown;
        StartCooldown(cd);

        StartCoroutine(InvincibleCr());
        IEnumerator InvincibleCr()
        {
            yield return null;
            Player.InvincibilityLock.RemoveLock(nameof(AbilityDash));
        }
    }

    private int HitEnemiesArea(Vector3 position, float radius)
    {
        var count = 0;
        var hits = Physics2D.OverlapCircleAll(position, radius)
            .Select(hit => hit.GetComponentInParent<IKillable>())
            .Distinct()
            .Where(k => k != null && k.CanKill());

        foreach(var hit in hits)
        {
            if (!hit.CanKill()) continue;
            Player.KillEnemy(hit);
            count++;

            if (ExplodeOnImpact)
            {
                var hitPosition = hit.GetPosition();
                StartCoroutine(AbilityExplode.ExplodeCr(new AbilityExplode.ChargeInfo
                {
                    parent = GameController.Instance.world,
                    delay = 2f,
                    getPosition = () => hitPosition,
                    radius = 4f,
                    play_charge_sfx = count == 1,
                }));
            }
        }

        return count;
    }

    private void KnockbackSelf()
    {
        Player.Knockback(-dir_dash.normalized * KnockbackForceSelf, true, true);
    }

    private void ShootShockwaves(Vector3 direction)
    {
        if(ShockwaveCount > 1)
        {
            var directions = AbilitySplit.GetSplitDirections(3, 45, direction);
            foreach(var dir in directions)
            {
                ShootShockwave(dir);
            }
        }
        else
        {
            ShootShockwave(direction);
        }
    }

    private void ShootShockwave(Vector3 direction)
    {
        var speed = ShockwaveSpeed;
        var distance = ShockwaveDistance;

        var p = ProjectileController.Instance.ShootPlayerProjectile(new ProjectileController.PlayerShootInfo
        {
            prefab = prefab_shockwave,
            position_start = Player.transform.position,
            velocity = direction * speed,
            onHit = OnHit,
        });
        p.Piercing = true;
        var lifetime = Calculator.DST_Time(distance, speed);
        p.Lifetime = Mathf.Clamp(lifetime, 0.1f, 5);

        var size = ShockwaveSize;
        p.transform.localScale = Vector3.one * size;

        if (ShockwaveBounce && !OnlyShockwave)
        {
            SetRippleDirectionToClosest(p);
        }

        if (ShockwaveLinger)
        {
            p.Drag = 0.95f;
            p.StartCoroutine(AnimateSizeCr(p));
        }

        void OnHit(Projectile p, IKillable k)
        {
            if (ShockwaveBounce)
            {
                SetRippleDirectionToClosest(p);
                var lifetime = Calculator.DST_Time(5f, speed);
                p.Lifetime += Mathf.Clamp(lifetime, 0.1f, 5);
                AbilityChain.CreateImpactPS(p.transform.position);
            }
        }

        IEnumerator AnimateSizeCr(Projectile p)
        {
            var start = p.transform.localScale;
            var end = p.transform.localScale * 1.5f;
            yield return LerpEnumerator.LocalScale(p.transform, p.Lifetime, start, end);
        }
    }

    private void SetRippleDirectionToClosest(Projectile p)
    {
        var radius = 15f;
        var start_position = p.transform.position;
        var closest = Physics2D.OverlapCircleAll(p.transform.position, radius)
            .Select(hit => hit.GetComponentInParent<IKillable>())
            .Where(hit => hit != null)
            .Select(hit => hit.GetPosition())
            .OrderBy(position => Vector3.Distance(position, start_position))
            .FirstOrDefault();

        if (closest == Vector3.zero) return;
        var dir = closest - p.transform.position;
        var vel = dir.normalized * ShockwaveSpeed;
        p.SetDirection(vel);
        p.Rigidbody.velocity = vel;
    }
}
