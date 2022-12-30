using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    public float SelfKnockback { get; private set; }
    public float CooldownOnHit { get; private set; }
    public bool TrailEnabled { get; private set; }

    [Header("DASH")]
    [SerializeField] private AbilityDashClone template_clone;
    [SerializeField] private AnimationCurve ac_push_enemies;
    [SerializeField] private FMODEventReference event_dash_start;
    [SerializeField] private FMODEventReference event_dash_impact;

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
        SelfKnockback = GetFloatValue("SelfKnockback");
        CooldownOnHit = GetFloatValue("CooldownOnHit");
        TrailEnabled = GetBoolValue("TrailEnabled");
    }

    public override void Trigger()
    {
        base.Trigger();
        if (Dashing) return;
        StartDashing();
    }

    private void StartDashing()
    {
        Dashing = true;
        Player.MovementLock.AddLock(nameof(AbilityDash));
        Player.DragLock.AddLock(nameof(AbilityDash));
        Player.InvincibilityLock.AddLock(nameof(AbilityDash));
        //Player.Body.gameObject.SetActive(false);
        Player.Body.SetCollisionEnabled(false);

        event_dash_start.Play();

        var directions = HasModifier(Type.SPLIT) ? AbilitySplit.GetSplitDirections(3, 25, Player.MoveDirection) : new List<Vector3> { Player.MoveDirection };
        for (int i = 0; i < directions.Count; i++)
        {
            StartCoroutine(DashCr(directions[i], i == 0));
        }

        if (HasModifier(Type.EXPLODE))
        {
            AbilityExplode.Explode(Player.transform.position, 3f, 2f, 200);
        }

        IEnumerator DashCr(Vector3 direction, bool has_player)
        {
            IKillable victim = null;
            var clone = CreateClone();
            yield return MoveCloneCr(clone);
            EndDash(clone, victim, has_player);

            IEnumerator MoveCloneCr(AbilityDashClone clone)
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

                if(victim != null)
                {
                    event_dash_impact.PlayWithTimeLimit(0.1f);
                }
            }
            
            AbilityDashClone CreateClone()
            {
                var clone = Instantiate(template_clone, GameController.Instance.world);
                clone.transform.position = Player.transform.position;
                clone.transform.rotation = Quaternion.LookRotation(Vector3.forward, direction);
                clone.Initialize(this, has_player);
                clone.gameObject.SetActive(true);
                clone.onHitKillable += k => {
                    if (HasModifier(Type.CHARGE) && k.CanKill())
                    {
                        HitEnemiesArea(clone.transform.position, RadiusDamage);
                    }
                    else
                    {
                        victim = k;
                    }
                };
                return clone;
            }
        }

        void EndDash(AbilityDashClone clone, IKillable victim, bool has_player)
        {
            var hit_anything = victim != null;
            if (!hit_anything)
            {
                hit_anything = HitEnemiesArea(clone.transform.position, 1.0f) > 0; // Try to hit something
            }

            if (has_player)
            {
                Dashing = false;
                Player.MovementLock.RemoveLock(nameof(AbilityDash));
                Player.DragLock.RemoveLock(nameof(AbilityDash));

                var cd = hit_anything ? Cooldown * CooldownOnHit : Cooldown;
                StartCooldown(cd);

                CameraController.Instance.Target = Player.transform;
                Player.transform.position = clone.transform.position;
                //Player.Body.gameObject.SetActive(true);
                Player.Body.SetCollisionEnabled(true);
                Player.Rigidbody.velocity = Player.MoveDirection * Speed * (hit_anything ? -1 : 1);

                if (hit_anything)
                {
                    KnockbackSelf();
                }
            }

            if (hit_anything || HasModifier(Type.CHARGE))
            {
                HitEnemiesArea(clone.transform.position, RadiusDamage);
                Player.PushEnemiesInArea(clone.transform.position, RadiusKnockback, ForceKnockback, ac_push_enemies);
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
            .Distinct()
            .Where(k => k != null && k.CanKill())
            .ToList().ForEach(k =>
            {
                Player.KillEnemy(k);
                count++;
            });
        return count;
    }

    private void KnockbackSelf()
    {
        Player.Knockback(-Player.MoveDirection.normalized * SelfKnockback, true, true);
    }
}
