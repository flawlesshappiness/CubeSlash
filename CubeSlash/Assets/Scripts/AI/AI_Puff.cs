using UnityEngine;

public class AI_Puff : EnemyAI
{
    [SerializeField] private DamageTrail trail;

    private Vector3 pos_player_prev;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);
        Self.OnDeath += OnDeath;
        trail.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        pos_player_prev = IsPlayerAlive() ? Player.Instance.transform.position : pos_player_prev;
        MoveTowards(pos_player_prev);
        TurnTowards(pos_player_prev);
    }

    private void OnDeath()
    {
        trail.CreateTrail(transform.position);
    }
}