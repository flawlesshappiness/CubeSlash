using UnityEngine;

public class AI_Shooter : EntityAI
{
    [SerializeField] private float dist_max_mul_shoot;
    [SerializeField] private float cooldown_shoot;

    [Header("PROJECTILE")]
    [SerializeField] private Projectile prefab_projectile;
    [SerializeField] private float velocity_projectile;

    [Header("CURVES")]
    [SerializeField] private AnimationCurve ac_move_mul;
    [SerializeField] private AnimationCurve ac_turn_mul;

    private float cd_shoot;
    private bool can_shoot;


    private void Update()
    {
        if (!PlayerIsAlive()) return;
        MoveUpdate();
        ShootUpdate();
    }

    private void MoveUpdate()
    {
        var dist = DistanceToPlayer();
        var dist_max = CameraController.Instance.Width * 0.5f;
        var t_dist = dist / dist_max;
        Self.SpeedMax = Self.Settings.speed_max * ac_move_mul.Evaluate(t_dist);
        Self.Acceleration = Self.Settings.speed_acceleration * ac_move_mul.Evaluate(t_dist);
        var turn = Self.Settings.speed_turn * ac_turn_mul.Evaluate(t_dist);
        MoveTowards(Player.Instance.transform.position, turn);
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