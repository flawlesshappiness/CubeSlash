using UnityEngine;

public class AI_SlowMove : EntityAI
{
    private Vector3 pos_player_prev;

    private void FixedUpdate()
    {
        pos_player_prev = IsPlayerAlive() ? Player.Instance.transform.position : pos_player_prev;
        Self.SpeedMax = Self.Settings.speed_max;
        Self.Acceleration = Self.Settings.speed_acceleration;
        MoveTowards(pos_player_prev, Self.Settings.speed_turn);
    }
}