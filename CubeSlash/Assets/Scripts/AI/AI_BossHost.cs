using UnityEngine;

public class AI_BossHost : EntityAI
{
    private Vector3 destination;
    private float time_wait;

    private void FixedUpdate()
    {
        if (Time.time < time_wait) return;

        if(Vector3.Distance(Position, PlayerPosition) > Screen.width)
        {
            destination = PlayerPosition;
        }

        if(Vector3.Distance(Position, destination) > 0.5f)
        {
            Self.SpeedMax = Self.Settings.speed_max;
            Self.Acceleration = Self.Settings.speed_acceleration;
            MoveTowards(destination, Self.Settings.speed_turn);
        }
        else if(IsPlayerAlive())
        {
            var r = Random.insideUnitCircle;
            destination = PlayerPosition + new Vector3(r.x, r.y) * 3f;
            time_wait = Time.time + Random.Range(0.5f, 2f);
        }
    }
}