using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySplit : Ability
{
    [Header("SPLIT")]
    [SerializeField] private SplitProjectile prefab_projectile;
    [SerializeField] private Projectile prefab_fragment;

    // Values
    private float Cooldown { get; set; }
    private int CountProjectiles { get; set; }
    private float ArcProjectiles { get; set; }
    private float SizeProjectiles { get; set; }
    private int ProjectileFragments { get; set; }
    private int Piercing { get; set; }
    private bool ChainLightning { get; set; }
    private bool Trail { get; set; }
    private bool ProjectileExplode { get; set; }

    private const float PROJECTILE_SPEED = 15f;
    private const float PROJECTILE_ARC = 40f;
    private const float PROJECTILE_SIZE = 0.5f;
    private const float PROJECTILE_LIFETIME = 0.75f;
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
        ArcProjectiles = PROJECTILE_ARC * GetFloatValue(StatID.split_arc_perc);
        SizeProjectiles = PROJECTILE_SIZE * GetFloatValue(StatID.split_size_perc);
        ProjectileFragments = GetIntValue(StatID.split_projectile_fragments);
        ChainLightning = GetBoolValue(StatID.split_chain);
        Trail = GetBoolValue(StatID.split_trail);
        ProjectileExplode = GetBoolValue(StatID.split_explode);
        Piercing = GetIntValue(StatID.split_piercing_count);
    }

    public override float GetBaseCooldown() => Cooldown;

    public override void Trigger()
    {
        base.Trigger();
        ShootProjectiles();
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
                velocity = direction * PROJECTILE_SPEED,
                onKill = OnKill
            }) as SplitProjectile;

            p.transform.localScale = Vector3.one * SizeProjectiles;
            p.Piercing = Piercing;
            p.Lifetime = PROJECTILE_LIFETIME;
            p.onDeath += () => OnDeath(p);

            p.HasTrail = Trail;
            p.HasChain = ChainLightning;
            p.ChainRadius = 4f * GetFloatValue(StatID.split_size_perc);

            var force = FORCE_SELF;

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

        void OnKill(Projectile p, IKillable k)
        {
            if (ProjectileFragments > 0)
            {
                SpawnFragments(p);
            }

            if (ProjectileExplode)
            {
                AbilityExplode.Explode(p.transform.position, 3f, 50);
            }
        }

        void OnDeath(Projectile p)
        {
            if (ProjectileExplode)
            {
                AbilityExplode.Explode(p.transform.position, 3f, 50);
            }
        }

        void SpawnFragments(Projectile p)
        {
            AbilityMines.ShootFragments(p.transform.position, prefab_fragment, ProjectileFragments, PROJECTILE_SPEED, SizeProjectiles);
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
