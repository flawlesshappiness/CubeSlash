using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class BoomerangProjectile : Projectile
{
    [SerializeField] private ParticleSystem ps_chain;

    public Vector3 StartPosition { get; set; }
    public Vector3 Velocity { get; set; }
    public float Distance { get; set; }
    public float LingerDuration { get; set; }
    public bool HasChain { get; set; }

    public bool Caught { get; private set; }

    private bool _stopping;
    private bool _returning;
    private float _time_chain_hit;

    private const float RETURN_ACCELERATION = 20f;
    private const float DISTANCE_CATCH = 1.5f;
    private const float TIME_CHAIN_HIT = 0.5f;
    private const float RADIUS_CHAIN = 5f;

    private const float MAX_STOPPING_VELOCITY = 0.5f;

    protected override void Start()
    {
        base.Start();

        AnimateRotation();

        if (HasChain)
        {
            ps_chain.Play();
        }

        StartCoroutine(StateCr());
    }

    private Coroutine AnimateRotation()
    {
        var start = Vector3.zero;
        var end = new Vector3(0, 0, -360f);
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            while (true)
            {
                yield return LerpEnumerator.LocalEuler(pivot_animation, 0.5f, start, end);

                HitOverlappingEnemies();
            }
        }
    }

    private void HitOverlappingEnemies()
    {
        var results = new Collider2D[10];
        var filter = new ContactFilter2D().NoFilter();
        if (Collider.OverlapCollider(filter, results) > 0)
        {
            foreach (var result in results)
            {
                if (result == null) continue;

                var k = result.GetComponentInParent<IKillable>();
                if (k == null) continue;

                onHitEnemy?.Invoke(k);
                k.TryKill();
            }
        }
    }

    public override void Update()
    {
        base.Update();

        UpdateCatch();
        UpdateSize();
        UpdateChain();
    }

    public void FixedUpdate()
    {
        UpdateLinger();
        UpdateReturn();
    }

    private IEnumerator StateCr()
    {
        // Distance
        while (Vector3.Distance(transform.position, StartPosition) < Distance)
        {
            yield return null;
        }

        // Linger
        _stopping = true;
        while (Rigidbody.velocity.magnitude > MAX_STOPPING_VELOCITY)
        {
            yield return null;
        }

        yield return new WaitForSeconds(LingerDuration);

        // Return
        _stopping = false;
        _returning = true;
    }

    private void UpdateLinger()
    {
        if (!_stopping) return;
        if (GameController.Instance.IsPaused) return;

        Rigidbody.AddForce(-Rigidbody.velocity.normalized * RETURN_ACCELERATION);

        if (Rigidbody.velocity.magnitude < MAX_STOPPING_VELOCITY)
        {
            Rigidbody.velocity = Vector3.zero;
        }
    }

    private void UpdateReturn()
    {
        if (!_returning) return;
        if (GameController.Instance.IsPaused) return;

        var projectile = this;
        var position_return = Player.Instance.transform.position;
        var max_velocity = Velocity.magnitude;
        UpdateReturnProjectile(projectile, position_return, max_velocity);
    }

    public static void UpdateReturnProjectile(Projectile projectile, Vector3 position_return, float max_velocity)
    {
        var rig = projectile.Rigidbody;
        var direction = position_return - projectile.transform.position;
        rig.AddForce(direction.normalized * RETURN_ACCELERATION);
        rig.velocity = Vector3.ClampMagnitude(rig.velocity, max_velocity);
    }

    private void UpdateCatch()
    {
        if (!_returning) return;

        var projectile = this;
        var catch_position = Player.Instance.transform.position;

        if (CanCatch(projectile, catch_position))
        {
            Caught = true;
        }

        TryCatch(projectile, catch_position);
    }

    public static bool TryCatch(Projectile projectile, Vector3 catch_position)
    {
        if (!CanCatch(projectile, catch_position)) return false;

        projectile.Kill();
        return true;
    }

    private static bool CanCatch(Projectile projectile, Vector3 catch_position)
    {
        var distance = Vector3.Distance(projectile.transform.position, catch_position);
        return distance < DISTANCE_CATCH;
    }

    private void UpdateSize()
    {
        var distance = Vector3.Distance(transform.position, Player.Instance.transform.position) - DISTANCE_CATCH;
        var distance_max = 2f + DISTANCE_CATCH;
        var t = Mathf.Clamp01(distance / distance_max);
        var size_min = Vector3.one * 0.25f;
        var size_max = Vector3.one * 1f;
        var size = Vector3.Lerp(size_min, size_max, t);
        pivot_animation.localScale = size;
    }

    private void UpdateChain()
    {
        if (!HasChain) return;
        if (Time.time < _time_chain_hit) return;

        var success = AbilityChain.TryChainToTarget(new AbilityChain.ChainInfo
        {
            center = transform.position,
            radius = RADIUS_CHAIN,
            chains_left = 1,
            initial_strikes = 1,
            onHit = (info, k) =>
            {
                var position = k.GetPosition();
                onHitEnemy?.Invoke(k);
            }
        });

        var time_add = success ? TIME_CHAIN_HIT : 0.1f;
        _time_chain_hit = Time.time + time_add;
    }
}