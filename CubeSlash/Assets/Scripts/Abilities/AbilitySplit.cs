using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySplit : Ability
{
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
            Player.DamageEnemy(e, 1);
            p.Destroy();
        };

        return p;
    }

    private void SpawnProjectiles()
    {
        var charge = GetModifier<AbilityCharge>(Type.CHARGE);
        var t_charge = charge ? charge.GetCharge() : 0;

        // Spawn projectiles
        var projectiles = new List<Projectile>();
        var forward = Player.MoveDirection;
        var angle_max = t_charge == 1 ? 180f :
            charge ? Mathf.Lerp(40, 20, t_charge) :
            30f;
        var count_arc = t_charge == 1 ? 10 :
            charge ? (int)Mathf.Clamp(Mathf.Lerp(1, 3, t_charge), 1, 2) :
            1;
        var angle_per = angle_max / count_arc;
        for (int i_arc = 0; i_arc < count_arc + 1; i_arc++)
        {
            var angle = angle_per * i_arc;
            var count_sides = i_arc == 0 ? 1 : 2;
            for (int i_side = 0; i_side < count_sides; i_side++)
            {
                var sign = i_side == 0 ? 1 : -1;
                var angle_signed = angle * sign;
                var direction = Quaternion.AngleAxis(angle_signed, Vector3.back) * forward;
                var p = SpawnProjectile(direction);
                projectiles.Add(p);
            }
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
    }

    private void SetupProjectileNormal(Projectile p)
    {
        p.TurnSpeed = 1f;
        p.Homing = true;
        p.Destroy(2);
    }

    private IEnumerator SetupProjectileDash(Projectile p)
    {
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
