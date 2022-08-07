using UnityEngine;

public class AI_Shooter : EntityAI
{
    [SerializeField] private float dist_max_mul_shoot;
    [SerializeField] private float cooldown_shoot;

    [Header("PROJECTILE")]
    [SerializeField] private Projectile prefab_projectile;
    [SerializeField] private float velocity_projectile;

    private Vector3 pos_player_prev;
    private float cd_shoot;
    private bool can_shoot;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);
        Self.LinearVelocity = Self.Settings.linear_velocity;
        Self.LinearAcceleration = Self.Settings.linear_acceleration;
        Self.AngularVelocity = Self.Settings.angular_velocity;
        Self.AngularAcceleration = Self.Settings.angular_acceleration;
    }

    private void FixedUpdate()
    {
        pos_player_prev = IsPlayerAlive() ? Player.Instance.transform.position : pos_player_prev;
        MoveUpdate();
        ShootUpdate();
    }

    private void MoveUpdate()
    {
        var dist = DistanceToPlayer();
        var dist_max_shoot = CameraController.Instance.Width * dist_max_mul_shoot;
        if(dist < dist_max_shoot)
        {
            Self.Rigidbody.velocity *= 0.999f;
            TurnTowards(pos_player_prev);
        }
        else
        {
            MoveTowards(pos_player_prev);
            TurnTowards(pos_player_prev);
        }
    }

    private void ShootUpdate()
    {
        var dist = DistanceToPlayer();
        var dist_max = CameraController.Instance.Width * dist_max_mul_shoot;
        if (dist > dist_max)
        {
            can_shoot = false;
            return;
        }

        if (Time.time < cd_shoot) return;
        cd_shoot = Time.time + cooldown_shoot;

        if (!can_shoot)
        {
            can_shoot = true;
        }
        else
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        var dir = DirectionToPlayer();
        var p = Instantiate(prefab_projectile.gameObject).GetComponent<Projectile>();
        p.transform.position = Position;
        p.Rigidbody.velocity = dir.normalized * velocity_projectile;
        p.SetDirection(dir);
        p.Lifetime = 999f;
        Self.Rigidbody.AddForce(-dir * 150);
    }
}