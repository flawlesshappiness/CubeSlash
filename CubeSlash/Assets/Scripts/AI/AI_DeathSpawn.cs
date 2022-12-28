using UnityEngine;

public class AI_DeathSpawn : EntityAI
{
    [SerializeField] private EnemySettings enemy_to_spawn;
    [SerializeField] private int count_to_spawn;
    [SerializeField] private float arc_spawn;

    private Vector3 pos_player_prev;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);
        enemy.OnDeath += OnDeath;
    }

    private void FixedUpdate()
    {
        pos_player_prev = IsPlayerAlive() ? Player.Instance.transform.position : pos_player_prev;
        MoveTowards(pos_player_prev);
        TurnTowards(pos_player_prev);
    }

    private void OnDeath()
    {
        var arc_half = arc_spawn / 2;
        var angle_min = -arc_half;
        var angle_max = arc_half;
        for (int i = 0; i < count_to_spawn; i++)
        {
            var t = (float)i / (count_to_spawn - 1);
            var angle = Mathf.Lerp(angle_min, angle_max, t);
            var q = Quaternion.AngleAxis(angle, Vector3.forward);
            var qsum = Self.Body.transform.rotation * q;
            var dir = qsum * Vector3.up;

            var e = EnemyController.Instance.SpawnEnemy(enemy_to_spawn, transform.position);
            e.Knockback(dir * 200, true, true);
            e.SetInvincible("spawn_" + GetInstanceID(), 0.1f);
            e.OnDeath += () => EnemyController.Instance.EnemyDeathSpawnMeat(e);
        }
    }
}