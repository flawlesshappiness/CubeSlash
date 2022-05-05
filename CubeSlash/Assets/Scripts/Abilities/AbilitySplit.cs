using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilitySplit : Ability
{
    public override void Pressed()
    {
        base.Pressed();

        var forward = Player.MoveDirection;
        var angle_max = 25f;
        var count_arc = 2;
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
                SpawnProjectile(direction);
            }
        }
    }

    private void SpawnProjectile(Vector3 direction)
    {
        var p = Instantiate(Resources.Load<Projectile>("Prefabs/Other/ProjectileSplit").gameObject).GetComponent<Projectile>();
        p.transform.position = Player.transform.position;
        p.Rigidbody.velocity = Player.MoveDirection * 25;
        p.SetDirection(direction);
        p.TurnSpeed = 1f;
        p.Homing = true;
        p.Destroy(2);

        p.OnHitEnemy += e =>
        {
            Player.DamageEnemy(e, 1);
            p.Destroy();
        };
    }
}
