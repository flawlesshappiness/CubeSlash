using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityDash : Ability
{
    private const float SPEED_DASH = 40;
    private const float TIME_DASH = 0.2f;

    private bool Dashing { get; set; }

    public override void Pressed()
    {
        base.Pressed();
        if (Dashing) return;
        StartDashing(true, true, true);
    }

    public override void EnemyCollision(Enemy enemy)
    {
        base.EnemyCollision(enemy);

        if (!Dashing) return;
        DashHitEnemy(enemy);

        if (enemy.health > 0)
        {
            StopDashing();
            Player.MoveDirection = -Player.MoveDirection;
            StartDashing(true, false, true);
        }
        else
        {
            StartCoroutine(DashHitstopCr());
        }
    }

    private List<Enemy> _hits_dash = new List<Enemy>();
    private bool dash_hitstop = false;
    private void StartDashing(bool reset_hits, bool hit_start, bool hit_end)
    {
        if (reset_hits) _hits_dash.Clear();
        _cr_dash = StartCoroutine(DashCr(hit_start, hit_end));
    }

    private void StopDashing()
    {
        if (_cr_dash != null)
        {
            StopCoroutine(_cr_dash);
            _cr_dash = null;
        }

        Dashing = false;
    }

    private Coroutine _cr_dash;
    private IEnumerator DashCr(bool hit_start, bool hit_end)
    {
        Dashing = true;
        BlockingAbilities = true;
        BlockingMovement = true;

        // Add invincibility
        Player.InvincibilityCounter++;

        // Hit everyone around
        if (hit_start)
        {
            DashHitEnemiesArea(transform.position, 1.5f);
        }

        // Dash
        var time_start = Time.time;
        while (Time.time - time_start < TIME_DASH)
        {
            if (dash_hitstop)
            {
                time_start += Time.deltaTime;
                Player.Rigidbody.velocity = Vector3.zero;
            }
            else
            {
                Player.Rigidbody.velocity = Player.MoveDirection * SPEED_DASH;
                Player.Character.SetLookDirection(Player.MoveDirection);
            }
            yield return null;
        }
        Player.Rigidbody.velocity = Player.MoveDirection * Player.SPEED_MOVE;

        // Hit everyone around
        if (hit_end)
        {
            DashHitEnemiesArea(transform.position + Player.MoveDirection * 0.5f, 1.5f);
        }

        // Remove invincibility
        Player.InvincibilityCounter--;

        BlockingAbilities = false;
        BlockingMovement = false;
        Dashing = false;
    }

    private IEnumerator DashHitstopCr()
    {
        dash_hitstop = true;
        for (int i = 0; i < 20; i++)
        {
            yield return null;
        }
        dash_hitstop = false;
    }

    private void DashHitEnemy(Enemy enemy)
    {
        if (enemy && !_hits_dash.Contains(enemy))
        {
            _hits_dash.Add(enemy);
            Player.DamageEnemy(enemy, 1);
        }
    }

    private void DashHitEnemiesArea(Vector3 position, float radius)
    {
        foreach (var hit in Physics2D.OverlapCircleAll(position, radius))
        {
            var enemy = hit.GetComponentInParent<Enemy>();
            DashHitEnemy(enemy);
        }
    }
}
