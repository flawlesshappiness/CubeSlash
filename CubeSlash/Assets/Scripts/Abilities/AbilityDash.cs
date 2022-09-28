using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class AbilityDash : Ability
{
    private bool Dashing { get; set; }
    private Vector3 PositionOrigin { get; set; }
    private Vector3 Direction { get; set; }

    // Values
    public float Distance { get; private set; }
    public float Speed { get; private set; }
    public float RadiusDamage { get; private set; }
    public float RadiusKnockback { get; private set; }
    public float ForceKnockback { get; private set; }
    public float CooldownOnHit { get; private set; }
    public int Charges { get; private set; }
    public bool TrailEnabled { get; private set; }

    [Header("DASH")]
    [SerializeField] private AbilityDashClone template_clone;
    [SerializeField] private AnimationCurve ac_push_enemies;

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();
        template_clone.gameObject.SetActive(false);
    }

    public override void OnValuesApplied()
    {
        base.OnValuesApplied();

        Distance = GetFloatValue("Distance");
        Speed = GetFloatValue("Speed");
        RadiusDamage = GetFloatValue("RadiusDamage");
        RadiusKnockback = GetFloatValue("RadiusKnockback");
        ForceKnockback = GetFloatValue("ForceKnockback");
        CooldownOnHit = GetFloatValue("CooldownOnHit");
        Charges = GetIntValue("Charges");
        TrailEnabled = GetBoolValue("TrailEnabled");
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
            clone.onHitKillable += k => {
                if (HasModifier(Type.CHARGE))
                {
                    HitEnemiesArea(clone.transform.position, RadiusDamage);
                }
                else
                {
                    victim = k;
                }
            };

            yield return MoveCloneCr();
            EndDash(clone, victim, has_player);

            IEnumerator MoveCloneCr()
            {
                var velocity = direction * Speed;
                var pos_origin = transform.position;

                while (victim == null && Vector3.Distance(clone.transform.position, pos_origin) < Distance)
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
                Player.MovementLock.RemoveLock(nameof(AbilityDash));
                Player.DragLock.RemoveLock(nameof(AbilityDash));
                StartCooldown();

                CameraController.Instance.Target = Player.transform;
                Player.transform.position = clone.transform.position;
                Player.Body.gameObject.SetActive(true);
                Player.Rigidbody.velocity = Player.MoveDirection * Speed * (hit_anything ? -1 : 1);
            }

            if (hit_anything || HasModifier(Type.CHARGE))
            {
                HitEnemiesArea(Player.transform.position, RadiusDamage);
                Player.PushEnemiesInArea(Player.transform.position, RadiusKnockback, ForceKnockback, ac_push_enemies);
                Player.Knockback(-Player.MoveDirection.normalized * 500, true, true);
            }

            clone.Destroy();

            StartCoroutine(EndInvincibilityCr());
            IEnumerator EndInvincibilityCr()
            {
                yield return new WaitForSeconds(0.2f);
                Player.InvincibilityLock.RemoveLock(nameof(AbilityDash));
            }
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
                count++;
            });

        return count;
    }
}
