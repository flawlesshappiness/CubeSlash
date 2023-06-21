using Flawliz.Lerp;
using UnityEngine;

public class AI_RootPull : EnemyAI
{
    [SerializeField] private RootPullVine _vine;

    public float max_dist_to_player;

    private Vector3 pos_player_prev;

    private bool _attached;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);
        enemy.OnDeath += OnDeath;
    }

    private void OnDeath()
    {
        _vine.PlayDissolveFX();
    }

    private void FixedUpdate()
    {
        pos_player_prev = IsPlayerAlive() ? Player.Instance.transform.position : pos_player_prev;
        MoveUpdate();
    }

    private void MoveUpdate()
    {
        var dist = DistanceToPlayer();
        var dist_max = CameraController.Instance.Width * max_dist_to_player;
        if (dist < dist_max)
        {
            MoveToStop(0.5f);
            TurnTowards(pos_player_prev);
            AttachToPlayer();
        }
        else
        {
            MoveTowards(pos_player_prev);
            TurnTowards(pos_player_prev);
        }
    }

    private void AttachToPlayer()
    {
        if (_attached) return;
        if (Player.Instance == null) return;
        if (Player.Instance.IsDead) return;
        _attached = true;
        _vine.Attach();
        _vine.AnimateToTarget();

        var sfx = SoundController.Instance.Play(SoundEffectType.sfx_enemy_root);
    }
}