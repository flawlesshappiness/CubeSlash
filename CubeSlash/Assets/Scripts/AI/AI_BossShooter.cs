using System.Collections;
using UnityEngine;

public class AI_BossShooter : EntityAI
{
    [SerializeField] private Projectile prefab_projectile;

    private enum MoveState { WATCH, MOVE_TO_PLAYER }
    private MoveState state = MoveState.WATCH;

    private Vector3 destination;

    private bool attacking;
    private float time_attack;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);
        ResetSpeed();
    }

    private void ResetSpeed()
    {
        Self.LinearVelocity = Self.Settings.linear_velocity;
        Self.LinearAcceleration = Self.Settings.linear_acceleration;
        Self.AngularVelocity = Self.Settings.angular_velocity;
        Self.AngularAcceleration = Self.Settings.angular_acceleration;
    }

    private void FixedUpdate()
    {
        if(state == MoveState.WATCH)
        {
            if(Vector3.Distance(Position, PlayerPosition) > CameraController.Instance.Width * 0.45f)
            {
                state = MoveState.MOVE_TO_PLAYER;
            }
            else
            {
                MoveToStop(1f);
                TurnTowards(PlayerPosition);

                if (!attacking && Time.time > time_attack)
                {
                    StartCoroutine(AttackShootFreq());
                }
            }
        }
        else if(state == MoveState.MOVE_TO_PLAYER)
        {
            if (Vector3.Distance(Position, PlayerPosition) > CameraController.Instance.Width * 0.25f)
            {
                destination = IsPlayerAlive() ? PlayerPosition : Position;
                MoveTowards(destination);
                TurnTowards(destination);
            }
            else
            {
                state = MoveState.WATCH;
            }
        }
    }

    private IEnumerator AttackShootFreq()
    {
        attacking = true;
        var time_start = Time.time;
        var time_end = Time.time + 5f;

        while(Time.time < time_end)
        {
            var t = Mathf.Clamp01((Time.time - time_start) / (time_end - time_start));
            Shoot();
            Self.AngularVelocity = Mathf.Lerp(Self.Settings.angular_velocity * 0.25f, Self.Settings.angular_velocity, t);
            yield return new WaitForSeconds(0.25f + 0.35f * t);
        }

        ResetSpeed();
        time_attack = Time.time + Random.Range(4f, 6f);
        attacking = false;
    }

    private void Shoot()
    {
        var velocity_projectile = 10;
        var dir = transform.up;
        var p = Instantiate(prefab_projectile.gameObject).GetComponent<Projectile>();
        p.transform.position = Position;
        p.Rigidbody.velocity = dir.normalized * velocity_projectile;
        p.SetDirection(dir);
        p.Lifetime = 999f;
        Self.Rigidbody.AddForce(-dir * 150 * Self.Rigidbody.mass);
    }
}