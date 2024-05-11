public class AI_SlowMove : EnemyAI
{
    private void FixedUpdate()
    {
        MoveTowards(PlayerPosition);
        TurnTowards(PlayerPosition);
    }
}