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
    public float TrailRadius { get; private set; }
    public bool TrailChain { get; private set; }
    public bool TrailSplit { get; private set; }
    public bool TrailFragment { get; private set; }

    [Header("DASH")]
    [SerializeField] private DamageTrail trail_gas;
    [SerializeField] private DamageTrail trail_chain;
    [SerializeField] private Projectile projectile_fragment;
    [SerializeField] private ParticleSystem ps_bubbles, ps_trail, ps_impact;
    [SerializeField] private AnimationCurve ac_push_enemies;

    private DamageTrail current_trail;
    private Coroutine cr_dash;
    private Vector3 dir_dash;
    private float distance_dashed;

    private const float DISTANCE = 15;
    private const float SPEED = 12;
    private const float RADIUS_DAMAGE = 1.5f;
    private const float RADIUS_FORCE = 12f;
    private const float FORCE = 200;
    private const float FORCE_SELF = 600;
    private const float TRAIL_DECAY_TIME = 2f;
    private const float TRAIL_RADIUS = 0.5f;

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();
        Player.onTriggerEnter += OnImpact;

        ps_trail.SetEmissionEnabled(false);

        trail_gas.gameObject.SetActive(false);
        trail_chain.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        Player.onTriggerEnter -= OnImpact;
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

        current_trail = TrailChain ? trail_chain : trail_gas;
        current_trail.ResetTrail();
        current_trail.lifetime = TrailDecayTime;
        current_trail.radius = TrailRadius;

        cr_dash = StartCoroutine(DashCr(Player.MoveDirection));
    }

    private IEnumerator DashCr(Vector3 direction)
    {
        IKillable victim = null;
        var velocity = direction * Speed;
        var pos_prev = Player.transform.position;
        distance_dashed = 0f;
        dir_dash = direction;
        while (victim == null && distance_dashed < Distance)
        {
            // Update distance
            var pos_cur = Player.transform.position;
            distance_dashed += Vector3.Distance(pos_prev, pos_cur);
            pos_prev = pos_cur;

            // Update direction
            var input = PlayerInput.MoveDirection;
            if (input.magnitude > 0.5f)
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

            UpdateTrail();

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
            Player.PushEnemiesInArea(Player.transform.position, RADIUS_FORCE, FORCE, false, ac_push_enemies);
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
            .Where(k => k != null);

        foreach (var hit in hits)
        {
            if (Player.TryKillEnemy(hit))
            {
                count++;
            }
        }

        return count;
    }

    private void KnockbackSelf()
    {
        Player.Knockback(-dir_dash.normalized * FORCE_SELF, true, true);
    }

    private void UpdateTrail()
    {
        var t = Mathf.Clamp01(distance_dashed / Distance);
        var trails = current_trail.CreateTrailsFromPreviousPosition();

        // Split
        if (TrailSplit)
        {
            var count = trails.Count;
            var radius = 3f;
            for (int i = 0; i < count; i++)
            {
                var trail = trails[i];
                var copy = trail.CreateTrail(trail.transform.position);
                var position = trail.transform.position;
                var next_position = i < trails.Count - 1 ? trails[i + 1].transform.position : Player.transform.position;
                var forward = next_position - position;
                var right = Vector3.Cross(forward, Vector3.forward).normalized;
                var tsin = Mathf.Lerp(0f, Mathf.PI * 2, t);
                var sine = Mathf.Sin(tsin);

                trail.transform.position = position + right * sine * radius;
                copy.transform.position = position - right * sine * radius;

                trails.Add(copy);
            }
        }

        if (TrailFragment)
        {
            var distance = 10f;
            var speed = 10f;
            var size = 0.5f;
            var lifetime = Calculator.DST_Time(distance, speed);
            foreach (var trail in trails)
            {
                trail.onHit += k =>
                {
                    var fragments = AbilityMines.ShootFragments(k.GetPosition(), projectile_fragment, 5, speed, size);
                    foreach (var frag in fragments)
                    {
                        frag.Lifetime = lifetime;
                    }
                };
            }
        }

        if (TrailChain)
        {
            foreach (var trail in trails)
            {
                AbilityChain.CreateImpactPS(trail.transform.position);

                trail.onHit += k =>
                {
                    AbilityChain.TryChainToTarget(new AbilityChain.ChainInfo
                    {
                        center = k.GetPosition(),
                        radius = 6f,
                        chains_left = 1,
                        initial_strikes = 1
                    });
                };
            }
        }
    }
}
