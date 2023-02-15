using UnityEngine;

public class AI_SlowMove : EnemyAI
{
    private Vector3 pos_player_prev;
    private static bool debug = true;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);
    }

    private void FixedUpdate()
    {
        pos_player_prev = IsPlayerAlive() ? Player.Instance.transform.position : pos_player_prev;
        MoveTowards(pos_player_prev);
        TurnTowards(pos_player_prev);
    }
}