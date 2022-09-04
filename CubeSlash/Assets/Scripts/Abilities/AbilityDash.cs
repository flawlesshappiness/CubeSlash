using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityDash : Ability
{
    private bool Dashing { get; set; }
    private Vector3 PositionOrigin { get; set; }
    private Vector3 Direction { get; set; }
    private float Distance { get; set; }
    private float DistanceExtendPerKill { get; set; }
    private float Speed { get; set; }
    private float RadiusTrigger { get; set; }
    private float RadiusDamage { get; set; }
    private float RadiusPush { get; set; }
    private float ForcePush { get; set; }

    // Upgrades
    private bool HasTrailUpgrade { get; set; }

    [Header("DASH")]
    [SerializeField] private BoxCollider2D trigger;
    [SerializeField] private TrailDash trail;
    [SerializeField] private ParticleSystem ps_dash;
    [SerializeField] private AnimationCurve ac_push_enemies;
    public AnimationCurve ac_path_normal;
    public AnimationCurve ac_path_split;

    private CustomCoroutine cr_dash;

    private float distance_target;
    private float distance_extend;

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();
        trigger.enabled = false;
        trail.gameObject.SetActive(false);
        SetEffectEnabled(false);
    }

    public override void ResetValues()
    {
        base.ResetValues();
        CooldownTime = 0.75f;
        Speed = 30f;
        Distance = 5f;
        DistanceExtendPerKill = 0;
        RadiusTrigger = 1;
        RadiusDamage = 1.0f;
        RadiusPush = 12;
        ForcePush = 300;

        InitializeBody();
    }

    public override void ApplyUpgrade(Upgrade upgrade)
    {
        base.ApplyUpgrade(upgrade);

        if(upgrade.data.type == UpgradeData.Type.DASH_DISTANCE)
        {
            if(upgrade.level >= 1)
            {
                RadiusDamage += 1.0f;
            }

            if(upgrade.level >= 2)
            {
                RadiusDamage += 1.0f;
                Distance += 2f;
            }

            if (upgrade.level >= 3)
            {
                RadiusDamage += 1.0f;
                DistanceExtendPerKill += 1f;
            }
        }

        if (upgrade.data.type == UpgradeData.Type.DASH_TRAIL)
        {
            if (upgrade.level >= 1)
            {
                Speed += 3.0f;
            }

            if (upgrade.level >= 2)
            {
                Speed += 3.0f;
                Distance += 2f;
            }

            if(upgrade.level >= 3)
            {
                Speed += 3.0f;
            }

            HasTrailUpgrade = upgrade.level >= 3;
        }
    }

    public override void ApplyModifier(Ability modifier)
    {
        base.ApplyModifier(modifier);

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

        RadiusTrigger = modifier.type switch
        {
            Type.DASH => RadiusTrigger + 0.5f,
            Type.CHARGE => RadiusTrigger + 0.5f,
            Type.SPLIT => RadiusTrigger + 2,
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
            StartDashing();
        }

        InUse = true;
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
                StartDashing();
            }
            else
            {
                InUse = false;
            }
        }
    }

    private void StartDashing()
    {
        trigger.enabled = true;
        distance_target = Distance + distance_extend;
        distance_extend = 0;
        Direction = Player.MoveDirection;
        trail.ResetTrail();
        cr_dash = CoroutineController.Instance.Run(SequenceCr(), "dash_" + GetInstanceID());
    }

    private IEnumerator SequenceCr()
    {
        Dashing = true;
        Player.Instance.MovementLock.AddLock(nameof(AbilityDash));
        Player.Instance.DragLock.AddLock(nameof(AbilityDash));
        Player.Instance.InvincibilityLock.AddLock(nameof(AbilityDash));

        SetEffectEnabled(true);

        // Dash
        var velocity = Direction * Speed;
        Player.Body.SetLookDirection(Direction);
        yield return MoveCr(velocity);

        // End
        if(HitEnemiesArea(Player.transform.position, 1.0f) > 0) // Default radius value
        {
            Player.PushEnemiesInArea(Player.transform.position, RadiusPush, ForcePush, ac_push_enemies);
            InterruptDash();
            StartCoroutine(KnockbackSelfCr());
        }
        else
        {
            EndDash();
        }
    }

    private void InterruptDash()
    {
        if (cr_dash != null)
        {
            Dashing = false;
            CoroutineController.Instance.Kill(cr_dash);
            UpdateBody(1);
            EndDash();
        }
    }

    private void EndDash()
    {
        Player.Instance.InvincibilityLock.RemoveLock(nameof(AbilityDash));
        Player.Instance.MovementLock.RemoveLock(nameof(AbilityDash));
        Player.Instance.DragLock.RemoveLock(nameof(AbilityDash));
        trigger.enabled = false;
        Dashing = false;

        SetEffectEnabled(false);

        StartCooldown();
    }

    private IEnumerator MoveCr(Vector3 velocity)
    {
        PositionOrigin = transform.position;
        while (Vector3.Distance(transform.position, PositionOrigin) < distance_target)
        {
            Player.Rigidbody.velocity = velocity;
            var t = Vector3.Distance(transform.position, PositionOrigin) / distance_target;
            UpdateBody(t);
            UpdateTrail();
            yield return new WaitForFixedUpdate();
        }
        Player.Rigidbody.velocity = velocity.normalized * Player.LinearVelocity;
        UpdateBody(1);
        UpdateTrail();

        if (HasModifier(Type.CHARGE))
        {
            HitEnemiesArea(transform.position, 2f);
        }
    }

    private void SetEffectEnabled(bool enabled)
    {
        ps_dash.transform.rotation = Player.Body.transform.rotation;
        foreach (var ps in ps_dash.GetComponentsInChildren<ParticleSystem>())
        {
            ps.ModifyEmission(e => e.enabled = enabled);
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
                clone.transform.localPosition = Player.Body.transform.localPosition;
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
        var pos_prev = Player.Body.transform.localPosition;
        var pos_next = right * tval;

        Player.Body.transform.localPosition = pos_next;

        var dir_delta = pos_next - pos_prev;
        Player.Body.SetLookDirection(dir + dir_delta);

        var ptrigger = Player.Instance.Body.Trigger;
        trigger.size = new Vector2(Mathf.Clamp(tval.Abs() * 2 + ptrigger.radius * 2, ptrigger.radius, float.MaxValue), ptrigger.radius * 2);
        trigger.transform.rotation = Player.Instance.Body.transform.rotation;

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
    #region TRAIL
    private void UpdateTrail()
    {
        if (HasTrailUpgrade)
        {
            trail.UpdateTrail();
        }
    }
    #endregion
    #region ENEMY
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponentInParent<Player>()) return;
        if (!Dashing) return;
        
        var killable = collision.GetComponentInParent<IKillable>();

        Player.PushEnemiesInArea(Player.transform.position, RadiusPush, ForcePush, ac_push_enemies);
        var count_hits = HitEnemiesArea(Player.transform.position, RadiusDamage);

        if(killable != null)
        {
            StopDashing();
        }

        void StopDashing()
        {
            InterruptDash();
            StartCoroutine(KnockbackSelfCr());
        }
    }

    private IEnumerator KnockbackSelfCr()
    {
        string slock = nameof(AbilityDash) + "Knockback";
        Player.Instance.InvincibilityLock.AddLock(slock);
        Player.Instance.MovementLock.AddLock(slock);
        Player.Instance.DragLock.AddLock(slock);
        Player.Rigidbody.velocity = Player.MoveDirection * -15f;
        yield return new WaitForSeconds(0.25f);
        Player.Instance.InvincibilityLock.RemoveLock(slock);
        Player.Instance.MovementLock.RemoveLock(slock);
        Player.Instance.DragLock.RemoveLock(slock);
    }

    private int HitEnemiesArea(Vector3 position, float radius)
    {
        var count = 0;
        Physics2D.OverlapCircleAll(position, radius)
            .Select(hit => hit.GetComponentInParent<IKillable>())
            .Where(k => k != null && k.CanKill())
            .ToList().ForEach(k =>
            {
                k.Kill();
                distance_extend += DistanceExtendPerKill;
                count++;
            });

        return count;
    }
    #endregion
    #region CLONE
    private Body clone;
    private Body CreateClone()
    {
        clone = Instantiate(Player.Body.gameObject, Player.Body.transform.parent).GetComponent<Body>();
        return clone;
    }
    #endregion
}
