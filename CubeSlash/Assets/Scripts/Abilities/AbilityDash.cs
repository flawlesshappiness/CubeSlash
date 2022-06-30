using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityDash : Ability
{
    private bool Dashing { get; set; }
    private Vector3 PosDashStart { get; set; }
    private float Distance { get; set; }
    private float Speed { get; set; }
    private float Radius { get; set; }

    [Header("DASH")]
    [SerializeField] private BoxCollider2D trigger;
    public AbilityVariable VarSpeed { get { return Variables[0]; } }
    public AbilityVariable VarDistance { get { return Variables[1]; } }
    public AnimationCurve ac_path_normal;
    public AnimationCurve ac_path_split;

    private CustomCoroutine cr_dash;

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();
    }

    public override void InitializeValues()
    {
        base.InitializeValues();
        CooldownTime = 0.5f;
        Speed = 20f + (80f * VarSpeed.Percentage);
        Distance = 5f + (10f * VarDistance.Percentage);
        Radius = 1;

        InitializeBody();
    }

    public override void InitializeModifier(Ability modifier)
    {
        base.InitializeModifier(modifier);

        CooldownTime = modifier.type switch
        {
            Type.DASH => CooldownTime + 0.5f,
            Type.CHARGE => CooldownTime - 0.5f,
            Type.SPLIT => CooldownTime + 0.5f,
        };

        Distance = modifier.type switch
        {
            Type.DASH => Distance + 1.0f,
            Type.CHARGE => Distance + 2.0f,
            Type.SPLIT => Distance + 1.0f,
        };

        Speed = modifier.type switch
        {
            Type.DASH => Speed + 10,
            Type.CHARGE => Speed + 40,
            Type.SPLIT => Speed + 0,
        };

        Radius = modifier.type switch
        {
            Type.DASH => Radius + 0.5f,
            Type.CHARGE => Radius + 0.5f,
            Type.SPLIT => Radius + 2,
        };

        if(modifier.type == Type.CHARGE)
        {
            var charge = (AbilityCharge)modifier;
            charge.ChargeTime = 0.5f;
        }
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

    private List<Enemy> _hits_dash = new List<Enemy>();
    private void StartDashing(bool reset_hits, bool hit_start, bool hit_end)
    {
        if (reset_hits) _hits_dash.Clear();
        cr_dash = CoroutineController.Instance.Run(SequenceCr(hit_start, hit_end), "dash_" + GetInstanceID());
    }

    private void InterruptDash()
    {
        if (cr_dash != null)
        {
            Dashing = false;
            CoroutineController.Instance.Kill(cr_dash);
            UpdateBody(1);
        }
    }

    private IEnumerator SequenceCr(bool hit_start, bool hit_end)
    {
        Dashing = true;
        Player.Instance.MovementLock.AddLock(nameof(AbilityDash));
        Player.Instance.DragLock.AddLock(nameof(AbilityDash));
        Player.Instance.InvincibilityLock.AddLock(nameof(AbilityDash));

        // Dash
        var direction = Player.MoveDirection;
        var velocity = direction * Speed;
        Player.Character.SetLookDirection(direction);
        //StartDashVisual();
        yield return MoveCr(velocity);

        // Remove counters
        Player.Instance.InvincibilityLock.RemoveLock(nameof(AbilityDash));
        Player.Instance.MovementLock.RemoveLock(nameof(AbilityDash));
        Player.Instance.DragLock.RemoveLock(nameof(AbilityDash));
        Dashing = false;

        StartCooldown();
    }

    private IEnumerator MoveCr(Vector3 velocity)
    {
        PosDashStart = transform.position;
        while (Vector3.Distance(transform.position, PosDashStart) < Distance)
        {
            Player.Rigidbody.velocity = velocity;
            var t = Vector3.Distance(transform.position, PosDashStart) / Distance;
            UpdateBody(t);
            yield return new WaitForFixedUpdate();
        }
        Player.Rigidbody.velocity = velocity.normalized * Player.SPEED_MOVE;
        UpdateBody(1);

        if (HasModifier(Type.CHARGE))
        {
            HitEnemiesArea(transform.position, 2f);
        }
    }

    #region VISUAL
    private void InitializeBody()
    {
        if(clone == null)
        {
            if (HasModifier(Type.SPLIT))
            {
                CreateClone();
                clone.gameObject.SetActive(false);
                clone.transform.localPosition = Player.Character.transform.localPosition;
            }
        }
        else
        {
            clone.gameObject.SetActive(false);
        }
    }

    private void UpdateBody(float t)
    {
        var curve = HasModifier(Type.SPLIT) ? ac_path_split : ac_path_normal;
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

            clone.gameObject.SetActive(t < 1f);
        }
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
            HitEnemiesArea(enemy.transform.position, 2f);

            if (enemy.IsKillable() && HasModifier(Type.CHARGE))
            {
                // Penetrate
            }
            else
            {
                // Stop dashing
                InterruptDash();
                StartCoroutine(HitKnockbackCr());
            }
        }
    }

    private IEnumerator HitKnockbackCr()
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

    private void HitEnemy(Enemy enemy)
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

    private void HitEnemiesArea(Vector3 position, float radius)
    {
        foreach (var hit in Physics2D.OverlapCircleAll(position, radius))
        {
            var enemy = hit.GetComponentInParent<Enemy>();
            HitEnemy(enemy);
        }
    }
    #endregion
    #region CLONE
    private Character clone;
    private Character CreateClone()
    {
        clone = Instantiate(Player.Character.gameObject, Player.Character.transform.parent).GetComponent<Character>();
        return clone;
    }
    #endregion
}
