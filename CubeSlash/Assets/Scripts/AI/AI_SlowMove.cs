using UnityEngine;

public class AI_SlowMove : EnemyAI
{
    private Vector3 pos_player_prev;

    private void FixedUpdate()
    {
        pos_player_prev = IsPlayerAlive() ? Player.Instance.transform.position : pos_player_prev;
        MoveTowards(pos_player_prev);
        TurnTowards(pos_player_prev);
    }
}