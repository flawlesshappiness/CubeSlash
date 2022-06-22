using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityDash : Ability
{
    private bool Dashing { get; set; }
    private float TimeStart { get; set; }
    private float TimeDash { get; set; }
    private float SpeedDash { get; set; }
    private float RadiusDash { get; set; }

    [Header("DASH")]
    [SerializeField] private BoxCollider2D trigger;
    public AbilityVariable VarTime { get { return Variables[0]; } }
    public AbilityVariable VarSpeed { get { return Variables[1]; } }
    public AnimationCurve ac_path_normal;
    public AnimationCurve ac_path_split;

    private CustomCoroutine cr_dash;

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();
        Variables[0] = new AbilityVariable(5, 10);
        Variables[1] = new AbilityVariable(5, 10);
    }

    public override void Pressed()
    {
        base.Pressed();

        var charge = GetModifier<AbilityCharge>(Type.CHARGE);
        if (charge)
        {
            charge.Pressed();
        }
        else
        {
            if (Dashing) return;
            StartDashing(true, true, true);
        }
    }

    
    public override void Released()
    {
        base.Released();

        var charge = GetModifier<AbilityCharge>(Type.CHARGE);
        if (charge)
        {
            charge.Released();
            if(charge.IsFullyCharged() && !Dashing)
            {
                StartDashing(true, true, true);
            }
        }
    }

    public override float GetCooldown()
    {
        float cd =
            (HasModifier(Type.SPLIT) ? 0.5f : 0) +
            (HasModifier(Type.CHARGE) ? -0.5f : 0) +
            0.5f;
        return Mathf.Clamp(cd, 0f, float.MaxValue);
    }

    private List<Enemy> _hits_dash = new List<Enemy>();
    private void StartDashing(bool reset_hits, bool hit_start, bool hit_end)
    {
        if (reset_hits) _hits_dash.Clear();

        TimeDash =
            HasModifier(Type.CHARGE) ? 0.3f :
            HasModifier(Type.SPLIT) ? 0.4f :
            0.25f;
        SpeedDash =
            HasModifier(Type.CHARGE) ? 60 :
            HasModifier(Type.SPLIT) ? 30 :
            30;
        RadiusDash =
            HasModifier(Type.SPLIT) ? 3 :
            1;
        cr_dash = CoroutineController.Instance.Run(DashPhaseCr(hit_start, hit_end), "dash_" + GetInstanceID());
    }

    private IEnumerator DashPhaseCr(bool hit_start, bool hit_end)
    {
        Dashing = true;
        Player.Instance.MovementLock.AddLock(nameof(AbilityDash));
        Player.Instance.DragLock.AddLock(nameof(AbilityDash));
        Player.Instance.InvincibilityLock.AddLock(nameof(AbilityDash));

        // Target
        var dir_dash = Player.MoveDirection;
        var target = GetTarget(10, 25);
        if (target)
        {
            dir_dash = Player.transform.DirectionTo(target.transform).normalized;
        }

        // Dash
        Player.Character.SetLookDirection(dir_dash);
        TimeStart = Time.time;
        StartDashVisual(TimeDash);
        yield return DashCr(dir_dash, TimeDash, SpeedDash);

        // Remove counters
        Player.Instance.InvincibilityLock.RemoveLock(nameof(AbilityDash));
        Player.Instance.MovementLock.RemoveLock(nameof(AbilityDash));
        Player.Instance.DragLock.RemoveLock(nameof(AbilityDash));
        Dashing = false;

        StartCooldown();
    }

    private IEnumerator DashCr(Vector3 dir, float time, float speed)
    {
        while (Time.time - TimeStart < time)
        {
            Player.Rigidbody.velocity = dir * speed;
            yield return new WaitForFixedUpdate();
        }
        Player.Rigidbody.velocity = dir * Player.SPEED_MOVE;
    }

    #region VISUAL
    private IEnumerator DashVisualCr(float time)
    {
        // Start
        Character clone = null;
        var curve = ac_path_normal;

        if (HasModifier(Type.SPLIT))
        {
            clone = GetClone();
            clone.gameObject.SetActive(true);
            clone.transform.localPosition = Player.Character.transform.localPosition;
            curve = ac_path_split;
        }

        // Dash
        while(Time.time - TimeStart < time)
        {
            var t = (Time.time - TimeStart) / time;
            var tval = curve.Evaluate(t);
            var dir = Player.MoveDirection;
            var right = Vector3.Cross(dir, Vector3.forward).normalized;
            var pos_prev = Player.Character.transform.localPosition;
            var pos_next = right * tval;

            Player.Character.transform.localPosition = pos_next;

            var dir_delta = pos_next - pos_prev;
            Player.Character.SetLookDirection(dir + dir_delta);

            var ptrigger = Player.Instance.Character.Trigger;
            trigger.size = new Vector2(Mathf.Clamp(tval.Abs() * 2 + ptrigger.radius * 2, ptrigger.radius, float.MaxValue), ptrigger.radius * 2);
            trigger.transform.rotation = Player.Instance.Character.transform.rotation;

            if (clone)
            {
                var _pos_prev = clone.transform.localPosition;
                var _pos_next = -right * tval;
                clone.transform.localPosition = _pos_next;

                var _dir_delta = _pos_next - _pos_prev;
                clone.SetLookDirection(dir + _dir_delta);
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private void StartDashVisual(float time)
    {
        CoroutineController.Instance.Run(DashVisualCr(time), "dash_visual_" + GetInstanceID())
            .OnEnd(() =>
            {
                // Player
                Player.Instance.Character.transform.localPosition = Vector3.zero;

                // Hide clone
                var clone = GetClone();
                if (clone)
                {
                    clone.gameObject.SetActive(false);
                }
            });
    }
    #endregion
    #region ENEMY
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!Dashing) return;
        var enemy = collision.GetComponentInParent<Enemy>();
        if (enemy)
        {
            var already_hit = _hits_dash.Contains(enemy);
            DashHitEnemy(enemy);

            // Stop dashing
            if (cr_dash != null)
            {
                Dashing = false;
                CoroutineController.Instance.Kill(cr_dash);
            }
            StartCoroutine(DashHitKnockbackCr());
        }
    }

    private IEnumerator DashHitKnockbackCr()
    {
        var time_end = Time.time + 0.25f;
        Player.Rigidbody.velocity = Player.MoveDirection * -15f;
        while (Time.time < time_end)
        {
            Player.Rigidbody.velocity = Player.Rigidbody.velocity * 0.995f;
            yield return null;
        }
        Player.Instance.InvincibilityLock.RemoveLock(nameof(AbilityDash));
        Player.Instance.MovementLock.RemoveLock(nameof(AbilityDash));
        Player.Instance.DragLock.RemoveLock(nameof(AbilityDash));
    }

    private void DashHitEnemy(Enemy enemy)
    {
        if (enemy && !_hits_dash.Contains(enemy))
        {
            _hits_dash.Add(enemy);

            var time = 0.25f;
            var velocity = Player.MoveDirection * 0.25f;
            var drag = 0.995f;

            if (enemy.IsParasite)
            {
                enemy.ParasiteHost.Knockback(time, velocity, drag);
            }

            if (enemy.IsKillable())
            {
                Player.KillEnemy(enemy);
            }
            else
            {
                enemy.Knockback(time, velocity, drag);
            }
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

    private class FindTargetMap
    {
        public Enemy Enemy { get; set; }
        public float Value { get; set; }
    }

    private Enemy GetTarget(float distance_max, float angle_max)
    {
        var targets = new List<FindTargetMap>();
        var hits = Physics2D.OverlapCircleAll(Player.transform.position, distance_max);
        foreach (var hit in hits)
        {
            var e = hit.gameObject.GetComponentInParent<Enemy>();
            if (e == null) continue;

            var dir = e.transform.position - Player.transform.position;
            var angle = Vector3.Angle(Player.MoveDirection.normalized, dir.normalized);
            var v_angle = 1f - (angle / 180f);
            if (angle > angle_max) continue;

            var dist = dir.magnitude;
            var v_dist = (1f - (dist / distance_max)) * 1.5f;

            var target = new FindTargetMap();
            targets.Add(target);
            target.Enemy = e;
            target.Value = v_angle + v_dist;
        }

        targets = targets.OrderByDescending(target => target.Value).ToList();
        return targets.Count == 0 ? null : targets[0].Enemy;
    }
    #endregion
    #region CLONE
    private Character clone;
    private Character GetClone()
    {
        if(clone == null)
        {
            clone = Instantiate(Player.Character.gameObject, Player.Character.transform.parent).GetComponent<Character>();
        }
        return clone;
    }
    #endregion
}
