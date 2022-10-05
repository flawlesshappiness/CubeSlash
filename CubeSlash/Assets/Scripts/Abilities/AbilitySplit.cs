using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AbilitySplit : Ability
{
    [Header("SPLIT")]
    [SerializeField] private Projectile prefab_projectile;
    private bool Firing { get; set; }
    private float time_fire;

    // Values
    private int Bursts { get; set; }
    private int CountProjectiles { get; set; }
    private float SpeedProjectiles { get; set; }
    private float ArcProjectiles { get; set; }
    private float SizeProjectiles { get; set; }
    private float RadiusKnockback { get; set; }
    private float ForceKnockback { get; set; }
    private bool SplitProjectiles { get; set; }
    private bool ExplodeProjectiles { get; set; }

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();
    }

    public override void OnValuesApplied()
    {
        base.OnValuesApplied();

        CountProjectiles = GetIntValue("CountProjectiles");
        SpeedProjectiles = GetFloatValue("SpeedProjectiles");
        ArcProjectiles = GetFloatValue("ArcProjectiles");
        SizeProjectiles = GetFloatValue("SizeProjectiles");
        RadiusKnockback = GetFloatValue("RadiusKnockback");
        ForceKnockback = GetFloatValue("ForceKnockback");
        SizeProjectiles = GetFloatValue("SizeProjectiles");
        Bursts = GetIntValue("Bursts");
        ExplodeProjectiles = GetBoolValue("ExplodeProjectiles");
        SplitProjectiles = GetBoolValue("SplitProjectiles");
    }

    public override void Pressed()
    {
        base.Pressed();
        var charge = GetModifier<AbilityCharge>(Type.CHARGE);
        if (charge)
        {
            charge.Pressed();
        }
        else
        {
            StartCoroutine(BurstFireCr(Bursts));
        }

        IEnumerator BurstFireCr(int count)
        {
            for (int i = 0; i < count; i++)
            {
                ShootProjectiles();
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    public override void Released()
    {
        base.Released();
        var charge = GetModifier<AbilityCharge>(Type.CHARGE);
        if (charge)
        {
            charge.Released();
            ShootProjectiles();
        }

        Firing = false;
    }

    private void Update()
    {
        if (Firing)
        {
            if(Time.time > time_fire)
            {
                time_fire = Time.time + 0.1f;
                ShootProjectiles();
            }
        }
    }

    public static Projectile ShootProjectile(Projectile prefab, Vector3 start_position, Vector3 direction, float size, float speed, System.Action<Projectile, IKillable> onHit = null)
    {
        var p = Instantiate(prefab);
        p.transform.position = start_position;
        p.transform.localScale = Vector3.one * size;
        p.Rigidbody.velocity = direction * speed;
        p.SetDirection(direction);

        p.OnHit += c =>
        {
            var k = c.GetComponentInParent<IKillable>();
            if (k != null)
            {
                onHit?.Invoke(p, k);
                if (k.CanKill()) k.Kill();
                if (!p.Piercing) p.Kill();
            }
        };

        return p;
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
            var p = ShootProjectile(prefab_projectile, Player.transform.position, direction, SizeProjectiles, SpeedProjectiles, (p, k) =>
            {
                Player.PushEnemiesInArea(p.transform.position, RadiusKnockback, ForceKnockback);

                if (SplitProjectiles)
                {
                    var count = 3;
                    var angle_delta = 360f / count;
                    for (int i = 0; i < count; i++)
                    {
                        var d = Quaternion.AngleAxis(angle_delta * i, Vector3.forward) * direction;
                        ShootProjectile(prefab_projectile, p.transform.position, d, SizeProjectiles * 0.5f, SpeedProjectiles);
                    }
                }
            });
            projectiles.Add(p);
        }

        // Setup projectiles
        foreach(var p in projectiles)
        {
            p.Homing = false;
            p.Lifetime = 0.75f;
            p.Piercing = HasModifier(Type.CHARGE);
        }

        // Cooldown
        StartCooldown();
    }

    public static List<Vector3> GetSplitDirections(int count, float angle_max, Vector3 forward)
    {
        var odd_count = count % 2 == 1;
        var directions = new List<Vector3>();
        var count_arc = ((count - 1) / 2);
        var angle_per = angle_max / count_arc;
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
