using UnityEngine;

public class AI_SlowMove : EntityAI
{
    private Vector3 pos_player_prev;

    private void FixedUpdate()
    {
        pos_player_prev = IsPlayerAlive() ? Player.Instance.transform.position : pos_player_prev;
        Self.LinearVelocity = Self.Settings.linear_velocity;
        Self.LinearAcceleration = Self.Settings.linear_acceleration;
        Self.AngularAcceleration = Self.Settings.angular_acceleration;
        Self.AngularVelocity = Self.Settings.angular_velocity;
        MoveTowards(pos_player_prev);
        TurnTowards(pos_player_prev);
    }
}