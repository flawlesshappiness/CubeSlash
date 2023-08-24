using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class BoomerangProjectile : Projectile
{
    [SerializeField] private ParticleSystem ps_chain;

    public Vector3 StartPosition { get; set; }
    public Vector3 Velocity { get; set; }
    public float Distance { get; set; }
    public bool HasChain { get; set; }

    public bool Caught { get; private set; }

    private bool _returning;
    private float _time_chain_hit;

    private const float DISTANCE_CATCH = 1.5f;
    private const float TIME_CHAIN_HIT = 0.5f;
    private const float RADIUS_CHAIN = 5f;

    protected override void Start()
    {
        base.Start();

        AnimateRotation();

        if (HasChain)
        {
            ps_chain.Play();
        }
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
            }
        }
    }

    public override void Update()
    {
        base.Update();

        UpdateDistance();
        UpdateReturn();
        UpdateCatch();
        UpdateSize();
        UpdateChain();
    }

    private void UpdateDistance()
    {
        if (_returning) return;

        var current_distance = Vector3.Distance(transform.position, StartPosition);
        if (current_distance < Distance) return;

        _returning = true;
    }

    private void UpdateReturn()
    {
        if (!_returning) return;
        if (GameController.Instance.IsPaused) return;

        var direction = Player.Instance.transform.position - transform.position;
        Rigidbody.AddForce(direction.normalized * 5f);
        Rigidbody.velocity = Vector3.ClampMagnitude(Rigidbody.velocity, Velocity.magnitude);
    }

    private void UpdateCatch()
    {
        if (!_returning) return;

        var distance = Vector3.Distance(transform.position, Player.Instance.transform.position);
        if (distance > DISTANCE_CATCH) return;

        Caught = true;
        Kill();
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