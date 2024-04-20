using System;
using System.Collections;
using System.Linq;
using UnityEngine;

public class PlayerDodge : MonoBehaviour
{
    [SerializeField] private ParticleSystem ps_dodge, ps_knockback;

    public bool Dashing { get; private set; }
    public float Distance { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.dodge_distance).ModifiedValue.float_value; } }
    public float Cooldown { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.dodge_cooldown).ModifiedValue.float_value; } }
    public float Knockback { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.dodge_knockback).ModifiedValue.float_value; } }
    public Player Player { get { return Player.Instance; } }

    public float Percentage { get { return (Time.time - time_cooldown_start) / (Cooldown * Player.GlobalCooldownMultiplier); } }
    public bool IsReady => is_ready;

    private bool is_ready;
    private float time_cooldown_start;
    private float distance_dashed;
    private Vector3 dir_dash;
    private Coroutine cr_dash;

    private const float TIME = 0.2f;
    private const float KNOCKBACK_RADIUS = 10f;

    private void Start()
    {
        Player.onHurt += OnHurt;
    }

    private void Update()
    {
        if (IsReady) return;
        if (Percentage < 1) return;
        is_ready = true;
    }

    private void OnHurt()
    {
        if (Dashing)
        {
            EndDash();
        }
    }

    public void Press()
    {
        try
        {
            StartDashing();
        }
        catch (Exception e)
        {
            EndDash();
            LogController.LogException(e);
        }
    }

    private void StartDashing()
    {
        if (!IsReady) return;
        if (Dashing) return;
        Dashing = true;

        Player.InputLock.AddLock(nameof(PlayerDodge));
        Player.DragLock.AddLock(nameof(PlayerDodge));
        Player.AbilityLock.AddLock(nameof(PlayerDodge));

        SoundController.Instance.Play(SoundEffectType.sfx_dash_start);

        if (Knockback > 0)
        {
            ps_knockback?.Play();
            PushAwayEnemies();
        }

        ps_dodge.transform.rotation = Player.Body.transform.rotation;
        ps_dodge?.Play();
        cr_dash = StartCoroutine(DashCr(Player.MoveDirection));
    }

    private IEnumerator DashCr(Vector3 direction)
    {
        var speed = Calculator.DST_Speed(Distance, TIME);
        var velocity = direction * speed;
        var pos_prev = Player.transform.position;
        distance_dashed = 0f;
        dir_dash = direction;
        while (distance_dashed < Distance)
        {
            if (RaycastObstacle()) break;

            Player.ResetStun();

            // Update distance
            var pos_cur = Player.transform.position;
            distance_dashed += Vector3.Distance(pos_prev, pos_cur);
            pos_prev = pos_cur;

            // Update direction
            var input = Player.MoveDirection;
            if (input.magnitude > 0.5f)
            {
                var right = Vector3.Cross(direction, Vector3.forward);
                var dot = Vector3.Dot(right, input);
                var sign = Mathf.Sign(dot);
                var angle = -2 * sign;
                direction = Quaternion.AngleAxis(angle, Vector3.forward) * direction;
                velocity = direction * speed;
            }

            Player.Rigidbody.velocity = velocity;
            Player.Body.SetLookDirection(direction);

            yield return new WaitForFixedUpdate();
        }

        EndDash();
    }

    private void EndDash()
    {
        if (cr_dash != null) StopCoroutine(cr_dash);
        cr_dash = null;

        Dashing = false;
        Player.InputLock.RemoveLock(nameof(PlayerDodge));
        Player.DragLock.RemoveLock(nameof(PlayerDodge));
        Player.AbilityLock.RemoveLock(nameof(PlayerDodge));

        StartCooldown();
    }

    private void StartCooldown()
    {
        is_ready = false;
        time_cooldown_start = Time.time;
    }

    private void PushAwayEnemies()
    {
        var force_base = Knockback;
        var enemies = EnemyController.Instance.ActiveEnemies
            .Where(e => Vector3.Distance(e.transform.position, transform.position) < KNOCKBACK_RADIUS)
            .ToList();

        foreach (var e in enemies)
        {
            var dir = e.transform.position - transform.position;
            var dist = dir.magnitude;
            var mul_dist = 1f - Mathf.Min(dist / KNOCKBACK_RADIUS, 0.5f);
            var force = force_base * mul_dist;
            var velocity = dir.normalized * force;
            e.Knockback(velocity, true, false);
        }
    }

    private bool RaycastObstacle()
    {
        var hits = Physics2D.RaycastAll(Player.Instance.transform.position, Player.MoveDirection.normalized, 1f);
        var any_obstacle = hits.Any(hit => hit.collider.GetComponentInParent<Obstacle>() != null);
        return any_obstacle;
    }
}