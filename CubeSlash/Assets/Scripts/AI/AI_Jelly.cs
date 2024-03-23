using Flawliz.Lerp;
using System.Collections;
using System.Linq;
using UnityEngine;

public class AI_Jelly : EnemyAI
{
    public float max_distance_to_player;
    public bool pushes_enemies_on_death;
    public SoundEffectType sfx_push;
    public ParticleSystem ps_death;

    private bool moving;

    private bool IsTooClose => DistanceToPlayer() < max_distance_to_player;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);
        StartCoroutine(MoveCr());

        enemy.OnDeath += OnDeath;
    }

    private void FixedUpdate()
    {
        if (!moving)
        {
            Self.Rigidbody.velocity *= 0.98f;
        }
    }

    private IEnumerator MoveCr()
    {
        while (true)
        {
            Lerp.LocalScale(Self.Body.pivot_sprite, 0.5f, new Vector3(0.8f, 1.2f, 1f))
            .Curve(EasingCurves.EaseOutQuad);

            moving = true;
            var time_move = Time.time + 0.5f;
            while (Time.time < time_move && !IsTooClose)
            {
                Self.Move(Self.MoveDirection);
                yield return new WaitForFixedUpdate();
            }
            moving = false;

            Lerp.LocalScale(Self.Body.pivot_sprite, 1f, Vector3.one)
                .Curve(EasingCurves.EaseInOutQuad);

            while (IsTooClose)
            {
                MoveToStop();
                TurnTowards(PlayerPosition);
                yield return null;
            }

            var time_wait = Time.time + 1f;
            while (Time.time < time_wait)
            {
                TurnTowards(PlayerPosition);
                yield return new WaitForFixedUpdate();
            }
        }
    }

    private void OnDeath()
    {
        if (pushes_enemies_on_death)
        {
            PushAwayEnemies();
            PushAwayPlayer();
            SoundController.Instance.SetGroupVolumeByPosition(SoundEffectType.sfx_enemy_jelly_burst, Position);
            SoundController.Instance.PlayGroup(SoundEffectType.sfx_enemy_jelly_burst);
        }

        if (ps_death != null)
        {
            var ps = ps_death.Duplicate()
                .Parent(GameController.Instance.world)
                .Position(transform.position)
                .Play()
                .Destroy(3);
        }
    }

    private void PushAwayEnemies()
    {
        var enemies = EnemyController.Instance.ActiveEnemies
            .Where(e => e != Self && Vector3.Distance(e.transform.position, transform.position) < 20)
            .ToList();

        var forward = Self.transform.up;
        var right = Self.transform.right;

        var force_base = 500f;
        var force_dot = 250f;
        var max_dist = 8f;
        var dir_player = DirectionToPlayer();

        foreach (var e in enemies)
        {
            var dir = e.transform.position - transform.position;
            var dist = dir.magnitude;
            var dot_player = Vector3.Dot(dir_player.normalized, dir.normalized);
            var mul_dot = 1f - Mathf.Abs(dot_player);
            var mul_base = 1f - Mathf.Min(dist / max_dist, 0.5f);
            var force = force_base * mul_base + force_dot * mul_dot;
            var velocity = dir.normalized * force;
            e.Knockback(velocity, true, false);
        }
    }

    private void PushAwayPlayer()
    {
        var dir = DirectionToPlayer();
        var dist = dir.magnitude;
        var max_dist = 8f;
        var mul_base = 1f - Mathf.Min(dist / max_dist, 0.5f);
        var force = 500f;
        var velocity = dir.normalized * force * mul_base;

        if (mul_base > 0)
        {
            Player.Instance.Knockback(velocity, true, false);
        }
    }
}