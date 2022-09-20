using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySplit : Ability
{
    [SerializeField] private Projectile prefab_projectile;
    private int Bursts { get; set; }
    private int CountProjectiles { get; set; }
    private float SpeedProjectile { get; set; }
    private float ArcProjectiles { get; set; }
    private float SizeProjectiles { get; set; }
    private bool Firing { get; set; }

    private bool Arc360;

    private float time_fire;

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();
    }

    public override void ResetValues()
    {
        base.ResetValues();
        CooldownTime = 0.5f;
        CountProjectiles = 3;
        SpeedProjectile = 25;
        ArcProjectiles = 15f;
        SizeProjectiles = 1.0f;
        Bursts = 1;
    }

    public override void ApplyUpgrade(Upgrade upgrade)
    {
        base.ApplyUpgrade(upgrade);
        /*
        if (upgrade.data.type == UpgradeData.Type.SPLIT_RATE)
        {
            if (upgrade.level >= 1)
            {
                SpeedProjectile += 5f;
                ArcProjectiles -= 5;
            }

            if (upgrade.level >= 2)
            {
                SpeedProjectile += 5f;
                ArcProjectiles -= 5;
            }

            if (upgrade.level >= 3)
            {
                Bursts += 2;
            }
        }

        if (upgrade.data.type == UpgradeData.Type.SPLIT_ARC)
        {
            if (upgrade.level >= 1)
            {
                CooldownTime += 0.2f;
                CountProjectiles += 1;
            }

            if (upgrade.level >= 2)
            {
                CooldownTime += 0.2f;
                CountProjectiles += 1;
            }

            Arc360 = upgrade.level >= 3;

            if(upgrade.level >= 3)
            {
                CooldownTime += 0.2f;
                CountProjectiles += 10;
            }
        }
        */
    }

    public override void ApplyModifier(Ability modifier)
    {
        base.ApplyModifier(modifier);

        CooldownTime = modifier.type switch
        {
            Type.DASH => CooldownTime - 0.2f,
            Type.CHARGE => CooldownTime + 1.0f,
            Type.SPLIT => CooldownTime + 0.5f,
        };

        CountProjectiles = modifier.type switch
        {
            Type.DASH => CountProjectiles + 0,
            Type.CHARGE => CountProjectiles + 0,
            Type.SPLIT => CountProjectiles + 3,
        };

        SpeedProjectile = modifier.type switch
        {
            Type.DASH => SpeedProjectile + 10,
            Type.CHARGE => SpeedProjectile + 0,
            Type.SPLIT => SpeedProjectile + 0,
        };

        ArcProjectiles = modifier.type switch
        {
            Type.DASH => ArcProjectiles + 0,
            Type.CHARGE => ArcProjectiles + 0,
            Type.SPLIT => ArcProjectiles + 0,
        };

        if (modifier.type == Type.CHARGE)
        {
            var charge = (AbilityCharge)modifier;
            charge.ChargeTime = 0.5f;
        }
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
                SpawnProjectiles();
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
            SpawnProjectiles();
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
                SpawnProjectiles();
            }
        }
    }

    private Projectile SpawnProjectile(Vector3 direction)
    {
        var p = Instantiate(prefab_projectile);
        p.transform.position = Player.transform.position;
        p.transform.localScale = Vector3.one * SizeProjectiles;
        p.Rigidbody.velocity = direction * SpeedProjectile;
        p.SetDirection(direction);

        p.OnHit += c =>
        {
            var k = c.GetComponentInParent<IKillable>();
            if(k != null)
            {
                if (k.CanKill())
                {
                    k.Kill();
                }

                if (!p.Piercing)
                {
                    p.Kill();
                }
            }
        };

        return p;
    }

    private void SpawnProjectiles()
    {
        // Spawn projectiles
        var projectiles = new List<Projectile>();
        var forward = Player.MoveDirection;
        var arc = Arc360 ? 175 : ArcProjectiles;
        var directions = GetSplitDirections(CountProjectiles, arc, forward);
        foreach(var direction in directions)
        {
            var p = SpawnProjectile(direction);
            projectiles.Add(p);
        }

        // Extra logic
        foreach(var p in projectiles)
        {
            SetupProjectileNormal(p);
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

    private void SetupProjectileNormal(Projectile p)
    {
        p.Homing = false;
        p.Lifetime = 0.75f;
        p.Piercing = HasModifier(Type.CHARGE);
    }
}
