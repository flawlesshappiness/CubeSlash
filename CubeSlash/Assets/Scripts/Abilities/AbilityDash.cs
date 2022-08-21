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
    private int Piercing { get; set; }

    // Upgrades
    private bool HasTrailUpgrade { get; set; }

    [Header("DASH")]
    [SerializeField] private BoxCollider2D trigger;
    [SerializeField] private TrailDash trail;
    [SerializeField] private AnimationCurve ac_push_enemies;
    public AnimationCurve ac_path_normal;
    public AnimationCurve ac_path_split;

    private CustomCoroutine cr_dash;

    private int pierces_left;
    private float distance_temp;

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();
        trigger.enabled = false;
        trail.gameObject.SetActive(false);
    }

    public override void ResetValues()
    {
        base.ResetValues();
        CooldownTime = 0.5f;
        Speed = 30f;
        Distance = 5f;
        DistanceExtendPerKill = 0;
        RadiusTrigger = 1;
        RadiusDamage = 1.0f;
        RadiusPush = 10;
        ForcePush = 400;
        Piercing = 0;

        InitializeBody();
    }

    public override void ApplyUpgrade(Upgrade upgrade)
    {
        base.ApplyUpgrade(upgrade);

        if(upgrade.data.type == UpgradeData.Type.DASH_DISTANCE)
        {
            if(upgrade.level >= 1)
            {
                CooldownTime += 0.3f;
                Piercing += 2;
            }

            if(upgrade.level >= 2)
            {
                CooldownTime += 0.3f;
                Distance += 2f;
                Piercing += 4;
            }

            if (upgrade.level >= 3)
            {
                DistanceExtendPerKill += 3f;
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
        }
    }

    private void StartDashing()
    {
        trigger.enabled = true;
        pierces_left = Piercing;
        distance_temp = Distance;
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

        var ps_trail = Instantiate(Resources.Load<ParticleSystem>("Particles/ps_trail_dash"), Player.Instance.transform);
        ps_trail.transform.localPosition = Vector3.zero;

        // Dash
        var velocity = Direction * Speed;
        Player.Character.SetLookDirection(Direction);
        yield return MoveCr(velocity);

        // End
        if(HitEnemiesArea(Player.transform.position, RadiusDamage) > 0)
        {
            PushEnemiesInArea(Player.transform.position, RadiusPush);
        }

        EndDash();
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

        StartCooldown();
    }

    private IEnumerator MoveCr(Vector3 velocity)
    {
        PositionOrigin = transform.position;
        while (Vector3.Distance(transform.position, PositionOrigin) < distance_temp)
        {
            Player.Rigidbody.velocity = velocity;
            var t = Vector3.Distance(transform.position, PositionOrigin) / distance_temp;
            UpdateBody(t);
            UpdateTrail();
            yield return new WaitForFixedUpdate();
        }
        Player.Rigidbody.velocity = velocity.normalized * Player.SpeedMove;
        UpdateBody(1);
        UpdateTrail();

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

        PushEnemiesInArea(Player.transform.position, RadiusPush);
        var count_hits = HitEnemiesArea(Player.transform.position, RadiusDamage);

        pierces_left -= count_hits;
        bool can_pierce = HasModifier(Type.CHARGE) || pierces_left > 0;

        if(killable != null)
        {
            if(killable.CanKill() && can_pierce)
            {
                distance_temp += DistanceExtendPerKill;
            }
            else
            {
                StopDashing();
            }
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
        var time_end = Time.time + 0.25f;
        Player.Rigidbody.velocity = Player.MoveDirection * -15f;
        while (Time.time < time_end)
        {
            Player.Rigidbody.velocity = Player.Rigidbody.velocity * 0.995f;
            yield return null;
        }
        Player.Instance.InvincibilityLock.RemoveLock(slock);
        Player.Instance.MovementLock.RemoveLock(slock);
        Player.Instance.DragLock.RemoveLock(slock);
    }

    private int HitEnemiesArea(Vector3 position, float radius)
    {
        var count = 0;
        foreach (var hit in Physics2D.OverlapCircleAll(position, radius))
        {
            var killable = hit.GetComponentInParent<IKillable>();
            if(killable != null && killable.CanKill())
            {
                killable.Kill();
                count++;
            }
        }
        return count;
    }

    private void PushEnemiesInArea(Vector3 position, float radius)
    {
        var hits = new List<Enemy>();
        foreach (var hit in Physics2D.OverlapCircleAll(position, radius))
        {
            var enemy = hit.GetComponentInParent<Enemy>();
            if(enemy != null && !hits.Contains(enemy))
            {
                hits.Add(enemy);
                var dir = enemy.transform.position - Player.transform.position;
                var dist = Vector3.Distance(enemy.transform.position, position);
                var t_dist = dist / radius;

                var t_force = ac_push_enemies.Evaluate(t_dist);
                var force = ForcePush * t_force;
                enemy.Rigidbody.velocity += dir.normalized.ToVector2() * force;
            }
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
