using UnityEngine;

public class AI_RootProjectiles : EnemyAI
{
    [SerializeField] private Projectile prefab_projectile;

    private Vector3 pos_player_prev;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);
        Self.OnDeath += OnDeath;
    }

    private void OnDeath()
    {
        var points = CircleHelper.Points(1, 3);
        foreach (var point in points)
        {
            var dir = Self.transform.rotation * point;
            Shoot(dir);
        }
    }

    private void FixedUpdate()
    {
        pos_player_prev = IsPlayerAlive() ? Player.Instance.transform.position : pos_player_prev;
        MoveTowards(pos_player_prev);
        TurnTowards(pos_player_prev);
    }

    private void Shoot(Vector3 dir)
    {
        var speed = 3;
        var p = ProjectileController.Instance.CreateProjectile(prefab_projectile);
        p.transform.position = Position;
        p.Rigidbody.velocity = dir.normalized * speed;
        p.SetDirection(dir);
        p.Lifetime = 999f;
        p.Piercing = -1;
    }
}