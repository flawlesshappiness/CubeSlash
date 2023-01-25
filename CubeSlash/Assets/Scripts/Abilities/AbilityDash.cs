using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityDash : Ability
{
    private bool Dashing { get; set; }
    private Vector3 PositionOrigin { get; set; }
    private Vector3 Direction { get; set; }

    // Values
    public float Distance { get; private set; }
    public float Speed { get; private set; }
    public float RadiusDamage { get; private set; }
    public float RadiusKnockback { get; private set; }
    public float ForceKnockback { get; private set; }
    public float SelfKnockback { get; private set; }
    public float CooldownOnHit { get; private set; }
    public bool TrailEnabled { get; private set; }
    public bool TeleportBack { get; private set; }
    public int RippleCount { get; private set; }
    public float RippleSpeed { get; private set; }
    public float RippleSize { get; private set; }
    public float RippleDistance { get; private set; }
    public bool RippleBounce { get; private set; }
    public bool OnlyRipple { get; private set; }
    public bool ExplodeOnImpact { get; private set; }

    [Header("DASH")]
    [SerializeField] private AbilityDashClone template_clone;
    [SerializeField] private Projectile prefab_shockwave;
    [SerializeField] private AnimationCurve ac_push_enemies;
    [SerializeField] private FMODEventReference event_dash_start;
    [SerializeField] private FMODEventReference event_dash_impact;

    private Coroutine cr_dash;

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
        template_clone.gameObject.SetActive(false);
        Player.onTriggerEnter += OnImpact;
    }

    private void OnDestroy()
    {
        Player.onTriggerEnter -= OnImpact;
    }

    public override void OnValuesApplied()
    {
        base.OnValuesApplied();

        Distance = DISTANCE * GetFloatValue("Distance");
        Speed = SPEED * GetFloatValue("Speed");
        RadiusDamage = RADIUS_DAMAGE * GetFloatValue("RadiusDamage");
        RadiusKnockback = RADIUS_FORCE * GetFloatValue("RadiusKnockback");
        ForceKnockback = FORCE * GetFloatValue("ForceKnockback");
        SelfKnockback = FORCE_SELF * GetFloatValue("SelfKnockback");
        CooldownOnHit = GetFloatValue("CooldownOnHit");
        TrailEnabled = GetBoolValue("TrailEnabled");
        TeleportBack = GetBoolValue("TeleportBack");
        RippleCount = GetIntValue("RippleCount");
        RippleSpeed = RIPPLE_SPEED * GetFloatValue("RippleSpeed");
        RippleSize = RIPPLE_SIZE * GetFloatValue("RippleSize");
        RippleDistance = RIPPLE_DISTANCE * GetFloatValue("RippleDistance");
        RippleBounce = GetBoolValue("RippleBounce");
        OnlyRipple = GetBoolValue("OnlyRipple");
        ExplodeOnImpact = GetBoolValue("ExplodeOnImpact");
    }

    public override void Trigger()
    {
        base.Trigger();
        if (Dashing) return;
        StartDashing();
    }

    private void StartDashing()
    {
        if (OnlyRipple)
        {
            ShootShockwaves(Player.MoveDirection);
        }
        else
        {
            Dashing = true;
            Player.MovementLock.AddLock(nameof(AbilityDash));
            Player.DragLock.AddLock(nameof(AbilityDash));
            Player.InvincibilityLock.AddLock(nameof(AbilityDash));
            PositionOrigin = Player.Instance.transform.position;
            cr_dash = StartCoroutine(DashCr(Player.MoveDirection));
        }
    }

    private IEnumerator DashCr(Vector3 direction)
    {
        IKillable victim = null;
        var velocity = direction * Speed;
        var pos_origin = transform.position;

        event_dash_start.Play();

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
        HitEnemiesArea(k.GetPosition(), RadiusDamage);
        EndDash(k);
    }

    private void EndDash(IKillable victim)
    {
        StopCoroutine(cr_dash);
        cr_dash = null;

        var hit_anything = victim != null;
        if (!hit_anything)
        {
            var radius = Player.Instance.PlayerBody.Size * 1.5f;
            hit_anything = HitEnemiesArea(Player.transform.position, radius) > 0; // Try to hit something
        }

        if (hit_anything)
        {
            KnockbackSelf();
            event_dash_impact.PlayWithTimeLimit(0.1f);
            Player.PushEnemiesInArea(Player.transform.position, RadiusKnockback, ForceKnockback, ac_push_enemies);
            ShootShockwaves(Player.MoveDirection);
        }

        Dashing = false;
        Player.MovementLock.RemoveLock(nameof(AbilityDash));
        Player.DragLock.RemoveLock(nameof(AbilityDash));

        var cd = hit_anything ? Cooldown * CooldownOnHit : Cooldown;
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
                    delay = 0.5f,
                    getPosition = () => hitPosition,
                    radius = 3f,
                    play_charge_sfx = count == 1,
                }));
            }
        }

        return count;
    }

    private void KnockbackSelf()
    {
        if (TeleportBack)
        {
            StartCoroutine(TeleportBackCr());
        }
        else
        {
            Player.Knockback(-Player.MoveDirection.normalized * SelfKnockback, true, true);
        }

        IEnumerator TeleportBackCr()
        {
            var start = Player.Instance.transform.position;
            AbilityChain.CreateZapPS(start, PositionOrigin);
            AbilityChain.CreateImpactPS(start);
            yield return new WaitForSeconds(0.1f);
            FMODEventReferenceDatabase.Load().sfx_chain_zap.Play();
            Player.Instance.transform.position = PositionOrigin;
        }
    }

    private void ShootShockwaves(Vector3 direction)
    {
        if(RippleCount > 1)
        {
            var directions = AbilitySplit.GetSplitDirections(3, 45, direction);
            foreach(var dir in directions)
            {
                ShootShockwave(dir);
            }
        }
        else if(RippleCount == 1 || OnlyRipple)
        {
            ShootShockwave(direction);
        }
    }

    private void ShootShockwave(Vector3 direction)
    {
        var speed = RippleSpeed;
        var distance = RippleDistance;

        var p = ProjectileController.Instance.ShootPlayerProjectile(new ProjectileController.PlayerShootInfo
        {
            prefab = prefab_shockwave,
            position_start = Player.transform.position,
            velocity = direction * speed,
            onHit = OnHit,
        });
        p.Piercing = true;

        p.Lifetime = distance / speed; // distance / speed = time

        var size = RippleSize;
        p.transform.localScale = Vector3.one * size;

        if (RippleBounce && !OnlyRipple)
        {
            SetRippleDirectionToClosest(p);
        }

        void OnHit(Projectile p, IKillable k)
        {
            if (RippleBounce)
            {
                SetRippleDirectionToClosest(p);
                p.Lifetime += 5f / speed; // distance / speed = time
                AbilityChain.CreateImpactPS(p.transform.position);
            }
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
        var vel = dir.normalized * RippleSpeed;
        p.SetDirection(vel);
        p.Rigidbody.velocity = vel;
    }
}
