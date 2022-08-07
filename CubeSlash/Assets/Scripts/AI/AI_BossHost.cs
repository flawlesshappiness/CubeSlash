using UnityEngine;

public class AI_BossHost : EntityAI
{
    private Vector3 destination;
    private float time_wait;

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
        if (Time.time < time_wait) return;

        if(Vector3.Distance(Position, PlayerPosition) > Screen.width)
        {
            destination = PlayerPosition;
        }

        if(Vector3.Distance(Position, destination) > 0.5f)
        {
            MoveTowards(destination);
            TurnTowards(destination);
        }
        else if(IsPlayerAlive())
        {
            var r = Random.insideUnitCircle;
            destination = PlayerPosition + new Vector3(r.x, r.y) * 3f;
            time_wait = Time.time + Random.Range(0.5f, 2f);
        }
    }
}