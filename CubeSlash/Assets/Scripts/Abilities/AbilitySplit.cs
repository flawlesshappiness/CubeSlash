using System.Collections.Generic;
using UnityEngine;

public class AbilitySplit : Ability
{
    [Header("SPLIT")]
    [SerializeField] private SplitProjectile prefab_projectile;
    [SerializeField] private Projectile prefab_fragment;

    // Values
    private float Cooldown { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.split_cooldown).ModifiedValue.float_value; } }
    private int CountProjectiles { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.split_count).ModifiedValue.int_value; } }
    private float ArcProjectiles { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.split_arc).ModifiedValue.float_value; } }
    private float SizeProjectiles { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.split_size).ModifiedValue.float_value; } }
    private int ProjectileFragments { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.split_fragments).ModifiedValue.int_value; } }
    private int Piercing { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.split_piercing_count).ModifiedValue.int_value; } }
    private bool ChainLightning { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.split_chain).ModifiedValue.bool_value; } }
    private bool ProjectileExplode { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.split_explode).ModifiedValue.bool_value; } }

    private const float PROJECTILE_SPEED = 15f;
    private const float PROJECTILE_LIFETIME = 0.75f;
    private const float FORCE_SELF = 100f;

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();
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
        foreach (var direction in directions)
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

            p.HasTrail = false;
            p.HasChain = ChainLightning;
            p.ChainRadius = 4f * SizeProjectiles;

            var force = FORCE_SELF;

            Player.Knockback(-direction * force, false, false);

            projectiles.Add(p);
        }

        // Play sound
        if (projectiles.Count > 0)
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
        if (count == 1)
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
