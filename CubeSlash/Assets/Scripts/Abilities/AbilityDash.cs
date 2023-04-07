using Flawliz.Lerp;
using System.Collections;
using System.Linq;
using UnityEngine;

public class AbilityDash : Ability
{
    private bool Dashing { get; set; }

    // Values
    public float Cooldown { get; private set; }
    public float Distance { get; private set; }
    public float Speed { get; private set; }
    public float TrailDecayTime { get; private set; }

    [Header("DASH")]
    [SerializeField] private DamageTrail damage_trail;
    [SerializeField] private ParticleSystem ps_bubbles, ps_trail, ps_impact;
    [SerializeField] private AnimationCurve ac_push_enemies;

    private Coroutine cr_dash;
    private Vector3 dir_dash;

    private const float DISTANCE = 15;
    private const float SPEED = 12;
    private const float RADIUS_DAMAGE = 1.5f;
    private const float RADIUS_FORCE = 12f;
    private const float FORCE = 200;
    private const float FORCE_SELF = 600;
    private const float TRAIL_DECAY_TIME = 2f;

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();
        Player.onTriggerEnter += OnImpact;

        ps_trail.SetEmissionEnabled(false);

        damage_trail.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        Player.onTriggerEnter -= OnImpact;
    }

    public override void OnValuesUpdated()
    {
        base.OnValuesUpdated();

        Cooldown = GetFloatValue(StatID.dash_cooldown_flat) * GetFloatValue(StatID.dash_cooldown_perc);
        Distance = DISTANCE * GetFloatValue(StatID.dash_distance_perc);
        Speed = SPEED * GetFloatValue(StatID.dash_speed_perc);
        TrailDecayTime = TRAIL_DECAY_TIME * GetFloatValue(StatID.dash_trail_time_perc);
    }

    public override float GetBaseCooldown() => Cooldown;

    public override void Trigger()
    {
        base.Trigger();
        if (Dashing) return;
        StartDashing();
    }

    private void StartDashing()
    {
        InUse = true;
        Dashing = true;

        Player.InputLock.AddLock(nameof(AbilityDash));
        Player.DragLock.AddLock(nameof(AbilityDash));
        Player.InvincibilityLock.AddLock(nameof(AbilityDash));
        Player.AbilityLock.AddLock(nameof(AbilityDash));

        SoundController.Instance.Play(SoundEffectType.sfx_dash_start);
        ps_trail.SetEmissionEnabled(true);

        damage_trail.ResetTrail();
        damage_trail.lifetime = TrailDecayTime;

        cr_dash = StartCoroutine(DashCr(Player.MoveDirection));
    }

    private IEnumerator DashCr(Vector3 direction)
    {
        IKillable victim = null;
        var velocity = direction * Speed;
        var pos_prev = Player.transform.position;
        var distance = 0f;
        dir_dash = direction;
        while (victim == null && distance < Distance)
        {
            // Update distance
            var pos_cur = Player.transform.position;
            distance += Vector3.Distance(pos_prev, pos_cur);
            pos_prev = pos_cur;

            // Update direction
            var input = PlayerInput.MoveDirection;
            if(input.magnitude > 0.5f)
            {
                var right = Vector3.Cross(direction, Vector3.forward);
                var dot = Vector3.Dot(right, input);
                var sign = Mathf.Sign(dot);
                var angle = -2 * sign;
                direction = Quaternion.AngleAxis(angle, Vector3.forward) * direction;
                velocity = direction * Speed;
            }

            Player.Rigidbody.velocity = velocity;
            Player.Body.SetLookDirection(direction);

            damage_trail.CreateTrailsFromPreviousPosition();

            yield return new WaitForFixedUpdate();
        }

        EndDash(victim);
    }

    private void OnImpact(Collider2D c)
    {
        if (!Dashing) return;

        var k = c.GetComponentInParent<IKillable>();
        if (k == null) return;
        HitEnemiesArea(k.GetPosition(), RADIUS_DAMAGE);
        EndDash(k);

        // Particle System
        var player_size = Player.Body.Size;
        var angle = Vector3.SignedAngle(Vector3.up, Player.Body.transform.up, Vector3.forward);
        var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        ps_impact.Duplicate()
            .Parent(GameController.Instance.world)
            .Position(ps_impact.transform.position + Player.Body.transform.up * player_size * 0.5f)
            .Rotation(rotation)
            .Play()
            .Destroy(1);
    }

    private void EndDash(IKillable victim)
    {
        StopCoroutine(cr_dash);
        cr_dash = null;

        Dashing = false;
        InUse = false;
        Player.InputLock.RemoveLock(nameof(AbilityDash));
        Player.DragLock.RemoveLock(nameof(AbilityDash));
        Player.AbilityLock.RemoveLock(nameof(AbilityDash));

        ps_trail.SetEmissionEnabled(false);

        StartCooldown();

        var hit_anything = victim != null;
        if (!hit_anything)
        {
            var radius = Player.Instance.PlayerBody.Size * 1.5f;
            hit_anything = HitEnemiesArea(Player.transform.position, radius) > 0; // Try to hit something
        }

        if (hit_anything)
        {
            KnockbackSelf();
            SoundController.Instance.Play(SoundEffectType.sfx_dash_impact);
            Player.PushEnemiesInArea(Player.transform.position, RADIUS_FORCE, FORCE, ac_push_enemies);
        }

        StartCoroutine(InvincibleCr());
        IEnumerator InvincibleCr()
        {
            yield return new WaitForSeconds(0.1f);
            Player.InvincibilityLock.RemoveLock(nameof(AbilityDash));
        }
    }

    private int HitEnemiesArea(Vector3 position, float radius)
    {
        var count = 0;
        var hits = Physics2D.OverlapCircleAll(position, radius)
            .Select(hit => hit.GetComponentInParent<IKillable>())
            .Distinct()
            .Where(k => k != null && k.CanKill());

        foreach(var hit in hits)
        {
            if (!hit.CanKill()) continue;
            Player.KillEnemy(hit);
            count++;
        }

        return count;
    }

    private void KnockbackSelf()
    {
        Player.Knockback(-dir_dash.normalized * FORCE_SELF, true, true);
    }
}
