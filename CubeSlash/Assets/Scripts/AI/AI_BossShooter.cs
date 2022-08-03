using UnityEngine;

public class AI_BossShooter : EntityAI
{
    [SerializeField] private Projectile prefab_projectile;

    private enum State { WATCH, MOVE_TO_PLAYER }
    private State state = State.WATCH;

    private Vector3 destination;

    private void FixedUpdate()
    {
        if(state == State.WATCH)
        {
            if(Vector3.Distance(Position, PlayerPosition) > CameraController.Instance.Width * 0.4f)
            {
                state = State.MOVE_TO_PLAYER;
            }
            else
            {
                destination = IsPlayerAlive() ? PlayerPosition : Position;
                Self.SpeedMax = 0;
                Self.Acceleration = 0;
                MoveTowards(destination, 10);
            }
        }
        else if(state == State.MOVE_TO_PLAYER)
        {
            if (Vector3.Distance(Position, PlayerPosition) > CameraController.Instance.Width * 0.25f)
            {
                destination = IsPlayerAlive() ? PlayerPosition : Position;
                Self.SpeedMax = Self.Settings.speed_max;
                Self.Acceleration = Self.Settings.speed_acceleration;
                MoveTowards(destination, 25);
            }
            else
            {
                state = State.WATCH;
            }
        }
    }

    private void Shoot()
    {
        var velocity_projectile = 10;
        var dir = DirectionToPlayer();
        var p = Instantiate(prefab_projectile.gameObject).GetComponent<Projectile>();
        p.transform.position = Position;
        p.Rigidbody.velocity = dir.normalized * velocity_projectile;
        p.SetDirection(dir);
        p.Lifetime = 999f;
        Self.Rigidbody.AddForce(-dir * 150);
    }
}