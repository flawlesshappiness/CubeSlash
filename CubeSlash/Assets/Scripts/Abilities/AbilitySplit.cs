using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilitySplit : Ability
{
    [Header("SPLIT")]
    [SerializeField] private Projectile prefab_projectile;

    // Values
    private float Cooldown { get; set; }
    private int Bursts { get; set; }
    private int CountProjectiles { get; set; }
    private float SpeedProjectiles { get; set; }
    private float ArcProjectiles { get; set; }
    private float SizeProjectiles { get; set; }
    private float RadiusKnockback { get; set; }
    private float ForceKnockback { get; set; }
    private float HitCooldownReduc { get; set; }
    private int ProjectileFragments { get; set; }
    private bool ChainLightning { get; set; }
    private bool ProjectileLinger { get; set; }
    private bool ProjectilePenetrate { get; set; }
    private bool ProjectileExplode { get; set; }
    private bool ProjectileMerge { get; set; }
    private int ProjectileBounces { get; set; }

    private const float PROJECTILE_SPEED = 15f;
    private const float PROJECTILE_ARC = 15f;
    private const float PROJECTILE_SIZE = 0.5f;
    private const float PROJECTILE_LIFETIME = 0.75f;
    private const float FORCE_RADIUS = 5f;
    private const float FORCE = 100f;
    private const float FORCE_SELF = 100f;

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();
    }

    public override void OnValuesUpdated()
    {
        base.OnValuesUpdated();

        Cooldown = GetFloatValue(StatID.split_cooldown_flat) * GetFloatValue(StatID.split_cooldown_perc);
        CountProjectiles = Mathf.Max(1, GetIntValue(StatID.split_count));
        SpeedProjectiles = PROJECTILE_SPEED * GetFloatValue(StatID.split_speed_perc);
        ArcProjectiles = PROJECTILE_ARC * GetFloatValue(StatID.split_arc_perc);
        SizeProjectiles = PROJECTILE_SIZE * GetFloatValue(StatID.split_size_perc);
        RadiusKnockback = FORCE_RADIUS * GetFloatValue(StatID.split_radius_knock_enemy_perc);
        ForceKnockback = FORCE * GetFloatValue(StatID.split_force_knock_enemy_perc);
        Bursts = GetIntValue(StatID.split_count_bursts);
        HitCooldownReduc = GetFloatValue(StatID.split_hit_cooldown_reduc);
        ProjectileFragments = GetIntValue(StatID.split_projectile_fragments);
        ChainLightning = GetBoolValue(StatID.split_chain);
        ProjectileLinger = GetBoolValue(StatID.split_projectile_linger);
        ProjectilePenetrate = GetBoolValue(StatID.split_penetrate);
        ProjectileExplode = GetBoolValue(StatID.split_explode);
        ProjectileBounces = GetIntValue(StatID.split_bounce);
        ProjectileMerge = GetBoolValue(StatID.split_size_merge);

        if (ProjectileMerge)
        {
            CountProjectiles = 1;
        }
    }

    public override float GetBaseCooldown() => Cooldown;

    public override void Trigger()
    {
        base.Trigger();
        StartCoroutine(BurstFireCr(Bursts));

        IEnumerator BurstFireCr(int count)
        {
            for (int i = 0; i < count; i++)
            {
                ShootProjectiles();
                yield return new WaitForSeconds(0.2f);
            }
        }
    }

    private void ShootProjectiles()
    {
        // Spawn projectiles
        var projectiles = new List<Projectile>();
        var forward = Player.MoveDirection;
        var arc = ArcProjectiles;
        var directions = GetSplitDirections(CountProjectiles, arc, forward);
        foreach(var direction in directions)
        {
            var p = ProjectileController.Instance.ShootPlayerProjectile(new ProjectileController.PlayerShootInfo
            {
                prefab = prefab_projectile,
                position_start = Player.transform.position,
                velocity = direction * SpeedProjectiles,
                onKill = OnKill
            });

            p.transform.localScale = Vector3.one * SizeProjectiles;
            p.Piercing = ProjectilePenetrate;
            p.Lifetime = PROJECTILE_LIFETIME;
            p.Bounces = ProjectileBounces;
            p.BounceBack = true;
            p.BounceAngleMax = 180f;

            var force = FORCE_SELF;

            if (ProjectileLinger)
            {
                p.Drag = 0.95f;
                p.Lifetime *= 5f;
            }

            if (ChainLightning)
            {
                p.StartCoroutine(ChainLightningCr(p));
            }

            if (ProjectileMerge)
            {
                var count = GetIntValue(StatID.split_count);
                var size = GetFloatValue(StatID.split_size_perc);
                p.transform.localScale = Vector3.one * PROJECTILE_SIZE * count * size;
                force = FORCE_SELF * count * size;
            }

            Player.Knockback(-direction * force, false, false);

            projectiles.Add(p);
        }

        // Play sound
        if(projectiles.Count > 0)
        {
            SoundController.Instance.Play(SoundEffectType.sfx_split_shoot);
        }

        // Cooldown
        StartCooldown();

        IEnumerator ChainLightningCr(Projectile p)
        {
            var time_zap = Time.time + 0.25f;
            while (true)
            {
                if(Time.time > time_zap)
                {
                    var success = AbilityChain.TryChainToTarget(p.transform.position, 6f, 1, 1, 1);
                    if (success)
                    {
                        time_zap = Time.time + 0.5f;
                    }
                }
                yield return null;
            }
        }

        void OnKill(Projectile p, IKillable k)
        {
            if (ProjectileFragments > 0)
            {
                SpawnFragments(p);
            }

            if (HitCooldownReduc < 0)
            {
                TimeCooldownEnd -= Mathf.Abs(HitCooldownReduc);
            }

            if (ProjectileExplode)
            {
                AbilityExplode.Explode(p.transform.position, 3f, 50);
            }
        }

        void SpawnFragments(Projectile p)
        {
            var count = ProjectileFragments;
            var angle_delta = 360f / count;
            for (int i = 0; i < count; i++)
            {
                var d = Quaternion.AngleAxis(angle_delta * i, Vector3.forward) * p.transform.up;
                var _p = ProjectileController.Instance.ShootPlayerProjectile(new ProjectileController.PlayerShootInfo
                {
                    prefab = prefab_projectile,
                    position_start = p.transform.position,
                    velocity = d * SpeedProjectiles,
                });

                _p.transform.localScale = 0.5f * SizeProjectiles * Vector3.one;
            }
        }
    }

    public static List<Vector3> GetSplitDirections(int count, float angle_max, Vector3 forward)
    {
        if(count == 1)
        {
            return new List<Vector3> { forward };
        }

        var directions = new List<Vector3>();
        var angle_start = -angle_max * 0.5f;
        var angle_per = angle_max / (count - 1);
        for (int i = 0; i < count; i++)
        {
            var angle = angle_start + angle_per * i;
            var dir = Quaternion.AngleAxis(angle, Vector3.back) * forward;
            directions.Add(dir);
        }

        return directions;
    }
}
