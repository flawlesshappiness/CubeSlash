using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySplit : Ability
{
    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();
    }

    public override void InitializeValues()
    {
        base.InitializeValues();
        CooldownTime = 0.5f;
    }

    public override void InitializeModifier(Ability modifier)
    {
        base.InitializeModifier(modifier);

        CooldownTime = modifier.type switch
        {
            Type.DASH => CooldownTime + 0.5f,
            Type.CHARGE => CooldownTime + 1.0f,
            Type.SPLIT => CooldownTime + 0.5f,
        };
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
            SpawnProjectiles();
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
    }

    private Projectile SpawnProjectile(Vector3 direction)
    {
        var p = Instantiate(Resources.Load<Projectile>("Prefabs/Other/ProjectileSplit").gameObject).GetComponent<Projectile>();
        p.transform.position = Player.transform.position;
        p.Rigidbody.velocity = direction * 25;
        p.SetDirection(direction);

        p.OnHitEnemy += e =>
        {
            Player.KillEnemy(e);
            p.Kill();
        };

        return p;
    }

    private void SpawnProjectiles()
    {
        // Spawn projectiles
        var projectiles = new List<Projectile>();
        var forward = Player.MoveDirection;
        var angle_max = HasModifier(Type.CHARGE) ? 180f : 35;
        var count_projectiles = HasModifier(Type.CHARGE) ? 30 : 5;
        var directions = GetSplitDirections(count_projectiles, angle_max, forward);
        foreach(var direction in directions)
        {
            var p = SpawnProjectile(direction);
            projectiles.Add(p);
        }

        // Extra logic
        foreach(var p in projectiles)
        {
            if (HasModifier(Type.DASH))
            {
                p.StartCoroutine(SetupProjectileDash(p));
            }
            else
            {
                SetupProjectileNormal(p);
            }
        }

        // Cooldown
        StartCooldown();
    }

    public List<Vector3> GetSplitDirections(int count, float angle_max, Vector3 forward)
    {
        var odd_count = count % 2 == 1;
        var directions = new List<Vector3>();
        var count_arc = ((count - 1) / 2);
        var angle_per = angle_max / count_arc;
        var i_start = odd_count ? 0 : 1;
        var i_end = count_arc + (odd_count ? 1 : 2);
        for (int i_arc = i_start; i_arc < i_end; i_arc++)
        {
            var angle = angle_per * i_arc;
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
        p.TurnSpeed = 1f;
        p.Homing = true;
        p.Lifetime = 0.75f;
    }

    private IEnumerator SetupProjectileDash(Projectile p)
    {
        p.Lifetime = 0.3f + 0.5f;
        p.SearchRadius = 30f;
        p.Drag = 0.9f;
        yield return new WaitForSeconds(0.3f);
        p.SearchForTarget = false;
        p.Homing = true;
        p.Drag = 1f;
        p.TurnSpeed = 10f;

        var dir = p.Target != null ? p.Target.transform.position - p.transform.position : p.transform.up;
        p.Rigidbody.velocity = dir.normalized * 50f;
    }
}
