using System.Collections.Generic;
using UnityEngine;

public class AbilityBoomerang : Ability
{
    [Header("BOOMERANG")]
    [SerializeField] private Projectile prefab_projectile;

    private float Cooldown { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.boomerang_cooldown).ModifiedValue.float_value; } }
    private int CountProjectile { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.boomerang_count).ModifiedValue.int_value; } } // SPLIT
    private float SizeProjectile { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.boomerang_size).ModifiedValue.float_value; } }
    private float Distance { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.boomerang_distance).ModifiedValue.float_value; } }
    private float CatchCooldown { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.boomerang_catch_cooldown).ModifiedValue.float_value; } }
    private float Lifetime { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.boomerang_lifetime).ModifiedValue.float_value; } }
    private float LingerTime { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.boomerang_linger_time).ModifiedValue.float_value; } }

    private bool ChainLightning { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.boomerang_chain).ModifiedValue.bool_value; } } // CHAIN
    private bool ProjectileExplode { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.boomerang_explode).ModifiedValue.bool_value; } } // EXPLODE
    private int FragmentProjectile { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.boomerang_fragment).ModifiedValue.int_value; } } // MINES
    private bool Orbit { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.boomerang_orbit).ModifiedValue.bool_value; } } // ORBIT

    private const float PROJECTILE_SPEED = 15f;
    private const float FORCE_SELF = 300f;
    private const float FORCE_SELF_SIZE = 100f;

    public override float GetBaseCooldown() => Cooldown;

    public override Dictionary<string, string> GetStats()
    {
        var stats = base.GetStats();

        var cooldown = Cooldown * GlobalCooldownMultiplier;
        stats.Add("Cooldown", cooldown.ToString("0.00"));
        stats.Add("Size", SizeProjectile.ToString("0.00"));
        stats.Add("Lifetime", Lifetime.ToString("0.00"));
        stats.Add("Linger time", LingerTime.ToString("0.00"));

        return stats;
    }

    public override void Trigger()
    {
        base.Trigger();

        TriggerShoot();
        StartCooldown();

        SoundController.Instance.Play(SoundEffectType.sfx_boomerang_shoot);
    }

    private void TriggerShoot()
    {
        var forward = Player.MoveDirection;

        if (CountProjectile > 1)
        {
            var angle_per = 360f / 8;
            var angle_max = angle_per * CountProjectile;
            var directions = AbilitySplit.GetSplitDirections(CountProjectile, angle_max, forward);
            foreach (var direction in directions)
            {
                var velocity = direction * PROJECTILE_SPEED;
                Shoot(velocity);
            }
        }
        else
        {
            var velocity = forward * PROJECTILE_SPEED;
            Shoot(velocity);
        }

        void Shoot(Vector3 velocity)
        {
            var fragment = FragmentProjectile > 0;
            var count = Mathf.Max(1, FragmentProjectile);
            var angle_delta = fragment ? 45 : 0;
            var velocity_min = fragment ? 0.75f : 1f;
            var velocity_max = fragment ? 1.5f : 1f;
            for (int i = 0; i < count; i++)
            {
                var angle = Random.Range(-angle_delta, angle_delta);
                var q = Quaternion.AngleAxis(angle, Vector3.forward);
                var mul = Random.Range(velocity_min, velocity_max);
                ShootProjectile(q * velocity * mul);
            }
        }
    }

    public void ShootProjectile(Vector3 velocity)
    {
        var direction = velocity.normalized;
        var start_position = Player.transform.position;

        var p = ProjectileController.Instance.ShootPlayerProjectile(new ProjectileController.PlayerShootInfo
        {
            prefab = prefab_projectile,
            position_start = start_position,
            velocity = velocity,
            onKill = OnKill
        }) as BoomerangProjectile;

        p.StartPosition = start_position;
        p.Velocity = velocity;
        p.Distance = Distance;
        p.LingerDuration = LingerTime;

        p.transform.localScale = Vector3.one * SizeProjectile;
        p.Piercing = -1;
        p.Lifetime = Lifetime + LingerTime;
        p.onDestroy += () => OnDestroy(p);

        p.HasChain = ChainLightning;
        p.SetMiniOrbitEnabled(Orbit);

        var force_mul = FragmentProjectile > 0 ? 1f / FragmentProjectile : 1f;
        var force = (FORCE_SELF + FORCE_SELF_SIZE * SizeProjectile) * force_mul / CountProjectile;
        Player.Knockback(-direction * force, false, false);

        void OnDestroy(BoomerangProjectile p)
        {
            if (p.Caught)
            {
                var prev = TimeCooldownLeft;
                AdjustCooldownFlat(-CatchCooldown);
                var cur = TimeCooldownLeft;
                SoundController.Instance.PlayGroup(SoundEffectType.sfx_boomerang_catch);
            }
        }

        void OnKill(Projectile p, IKillable k)
        {
            if (ProjectileExplode)
            {
                AbilityExplode.Explode(p.transform.position, 3f, 0);
            }
        }
    }
}