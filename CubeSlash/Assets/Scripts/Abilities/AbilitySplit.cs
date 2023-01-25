using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AbilitySplit : Ability
{
    [Header("SPLIT")]
    [SerializeField] private Projectile prefab_projectile;
    [SerializeField] private FMODEventReference sfx_shoot_projectiles;

    // Values
    private int Bursts { get; set; }
    private int CountProjectiles { get; set; }
    private float SpeedProjectiles { get; set; }
    private float ArcProjectiles { get; set; }
    private float SizeProjectiles { get; set; }
    private float RadiusKnockback { get; set; }
    private float ForceKnockback { get; set; }
    private bool SplitProjectiles { get; set; }
    private bool ChainLightning { get; set; }
    private float HitCooldownReduc { get; set; }

    private const float PROJECTILE_SPEED = 20f;
    private const float PROJECTILE_ARC = 15f;
    private const float PROJECTILE_SIZE = 1f;
    private const float FORCE_RADIUS = 5f;
    private const float FORCE = 100f;

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();
    }

    public override void OnValuesApplied()
    {
        base.OnValuesApplied();

        CountProjectiles = GetIntValue("CountProjectiles");
        SpeedProjectiles = PROJECTILE_SPEED * GetFloatValue("SpeedProjectiles");
        ArcProjectiles = PROJECTILE_ARC * GetFloatValue("ArcProjectiles");
        SizeProjectiles = PROJECTILE_SIZE * GetFloatValue("SizeProjectiles");
        RadiusKnockback = FORCE_RADIUS * GetFloatValue("RadiusKnockback");
        ForceKnockback = FORCE * GetFloatValue("ForceKnockback");
        Bursts = GetIntValue("Bursts");
        SplitProjectiles = GetBoolValue("SplitProjectiles");
        ChainLightning = GetBoolValue("ChainLightning");
        HitCooldownReduc = GetFloatValue("HitCooldownReduc");
    }

    public override void Trigger()
    {
        base.Trigger();
        StartCoroutine(BurstFireCr(Bursts));

        IEnumerator BurstFireCr(int count)
        {
            for (int i = 0; i < count; i++)
            {
                ShootProjectiles();
                yield return new WaitForSeconds(0.1f);
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
                onHit = OnHit
            });

            p.transform.localScale = Vector3.one * SizeProjectiles;

            if (ChainLightning)
            {
                p.StartCoroutine(ChainLightningCr(p));
            }

            projectiles.Add(p);
        }

        // Setup projectiles
        foreach(var p in projectiles)
        {
            p.Homing = false;
            p.Lifetime = 0.75f;
            p.Piercing = HasModifier(Type.CHARGE);
        }

        // Play sound
        if(projectiles.Count > 0)
        {
            sfx_shoot_projectiles.Play();
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

        void OnHit(Projectile p, IKillable k)
        {
            Player.PushEnemiesInArea(p.transform.position, RadiusKnockback, ForceKnockback);

            if (SplitProjectiles)
            {
                var count = 3;
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

            if (HitCooldownReduc < 0)
            {
                TimeCooldownEnd -= Mathf.Abs(HitCooldownReduc);
            }

            if (HasModifier(Type.EXPLODE))
            {
                AbilityExplode.Explode(p.transform.position, 3f, 50);
            }
        }
    }

    public static List<Vector3> GetSplitDirections(int count, float angle_max, Vector3 forward)
    {
        var odd_count = count % 2 == 1;
        var directions = new List<Vector3>();
        var count_arc = ((count - 1) / 2);
        var angle_per = angle_max == 0 ? 0 : angle_max / count_arc;
        var i_start = odd_count ? 0 : 1;
        var i_end = count_arc + (odd_count ? 1 : 2);
        for (int i_arc = i_start; i_arc < i_end; i_arc++)
        {
            var angle = (angle_per * i_arc) - (odd_count ? 0 : angle_per * 0.5f);
            var count_sides = i_arc == 0 && odd_count ? 1 : 2;
            for (int i_side = 0; i_side < count_sides; i_side++)
            {
                var sign = i_side == 0 ? 1 : -1;
                var angle_signed = angle * sign;
                var direction = Quaternion.AngleAxis(angle_signed, Vector3.back) * forward;
                directions.Add(direction);
            }
        }
        return directions;
    }
}
