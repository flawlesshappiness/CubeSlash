using UnityEngine;

public class AI_BossHost : EntityAI
{
    private Vector3 destination;

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
        if(IsPlayerAlive())
        {
            destination = PlayerPosition;
        }

        var dir = (destination - Position).normalized;
        var offset = dir * Self.Settings.size * 0.7f;
        var t = (Vector3.Distance(Position + offset, destination)) / (CameraController.Instance.Width * 0.5f);
        Self.AngularAcceleration = Mathf.Lerp(Self.Settings.angular_acceleration * 0.25f, Self.Settings.angular_acceleration, t);
        Self.AngularVelocity = Mathf.Lerp(Self.Settings.angular_velocity * 0.25f, Self.Settings.angular_velocity, t);
        MoveTowards(destination);
        TurnTowards(destination);
    }
}