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
    private float DistanceExtend { get; set; }

    // Upgrades
    public bool HasTrailUpgrade { get; set; }

    [Header("DASH")]
    [SerializeField] private AbilityDashClone template_clone;
    [SerializeField] private AnimationCurve ac_push_enemies;

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();
        template_clone.gameObject.SetActive(false);
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
            Type.SPLIT => CooldownTime + 2.0f,
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
        Dashing = true;
        Player.MovementLock.AddLock(nameof(AbilityDash));
        Player.DragLock.AddLock(nameof(AbilityDash));
        Player.InvincibilityLock.AddLock(nameof(AbilityDash));
        Player.Body.gameObject.SetActive(false);

        var extend = DistanceExtend;
        DistanceExtend = 0;

        var directions = HasModifier(Type.SPLIT) ? AbilitySplit.GetSplitDirections(3, 25, Player.MoveDirection) : new List<Vector3> { Player.MoveDirection };
        for (int i = 0; i < directions.Count; i++)
        {
            StartCoroutine(DashCr(directions[i], i == 0));
        }

        IEnumerator DashCr(Vector3 direction, bool has_player)
        {
            IKillable victim = null;
            var clone = Instantiate(template_clone, GameController.Instance.world);
            clone.transform.position = Player.transform.position;
            clone.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
            clone.Initialize(this, has_player);
            clone.gameObject.SetActive(true);
            clone.onHitKillable += k => victim = k;

            yield return MoveCloneCr();
            EndDash(clone, victim, has_player);

            IEnumerator MoveCloneCr()
            {
                var velocity = direction * Speed;
                var pos_origin = transform.position;
                var dist_target = Distance + extend;

                while (victim == null && Vector3.Distance(clone.transform.position, pos_origin) < dist_target)
                {
                    clone.Rigidbody.velocity = velocity;
                    clone.DashUpdate();
                    yield return new WaitForFixedUpdate();
                }
                clone.DashUpdate();
            }
        }

        void EndDash(AbilityDashClone clone, IKillable victim, bool has_player)
        {
            var hit_anything = victim != null;
            if (!hit_anything)
            {
                hit_anything = HitEnemiesArea(Player.transform.position, 1.0f) > 0; // Try to hit something
            }

            if (has_player)
            {
                Dashing = false;
                Player.InvincibilityLock.RemoveLock(nameof(AbilityDash));
                Player.MovementLock.RemoveLock(nameof(AbilityDash));
                Player.DragLock.RemoveLock(nameof(AbilityDash));
                StartCooldown();

                CameraController.Instance.Target = Player.transform;
                Player.transform.position = clone.transform.position;
                Player.Body.gameObject.SetActive(true);
                Player.Rigidbody.velocity = Player.MoveDirection * Speed * (hit_anything ? -1 : 1);
            }

            if (hit_anything)
            {
                HitEnemiesArea(Player.transform.position, RadiusDamage);
                Player.PushEnemiesInArea(Player.transform.position, RadiusPush, ForcePush, ac_push_enemies);
            }

            clone.Destroy();
        }
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
                DistanceExtend += DistanceExtendPerKill;
                count++;
            });

        return count;
    }
}
