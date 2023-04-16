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

    private void Update()
    {
        var dist_max = 15f;
        var t = 1f - Mathf.Clamp01(DistanceToPlayer() / dist_max);
        var mul = Mathf.Lerp(0f, 0.15f, t);
        Self.Body.pivot_sprite.localPosition = Random.insideUnitCircle.ToVector3() * mul;
    }

    private void OnDeath()
    {
        trail.CreateTrail(transform.position);
    }
}