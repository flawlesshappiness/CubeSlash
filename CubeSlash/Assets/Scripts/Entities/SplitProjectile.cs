using System.Collections;
using UnityEngine;

public class SplitProjectile : Projectile
{
    public bool HasTrail { get; set; } = false;
    public bool HasChain { get; set; } = false;
    public float ChainRadius { get; set; } = 5f;

    [Header(nameof(SplitProjectile))]
    [SerializeField] private DamageTrail trail;
    [SerializeField] private ParticleSystem ps_gas, ps_chain;
    [SerializeField] private SpriteRenderer spr;

    private int _chain_hits;

    private const float TIME_CHAIN_HIT = 0.5f;

    protected override void Start()
    {
        spr.enabled = !HasChain && !HasTrail;

        trail.gameObject.SetActive(false);
        if(HasTrail)
        {
            trail.ResetTrail();
            ps_gas.Play();
        }

        if (HasChain)
        {
            ps_chain.Play();
            Drag = 0.96f;
            _chain_hits = 3;
            Lifetime = TIME_CHAIN_HIT * (_chain_hits + 1);
            StartCoroutine(ChainLightningCr());
        }

        base.Start();
    }

    public override void Update()
    {
        base.Update();

        if(HasTrail)
        {
            trail.CreateTrailsFromPreviousPosition();
        }
    }

    IEnumerator ChainLightningCr()
    {
        var time_zap = Time.time + TIME_CHAIN_HIT;
        while (true)
        {
            if (Time.time > time_zap)
            {
                var success = AbilityChain.TryChainToTarget(new AbilityChain.ChainInfo
                {
                    center = transform.position,
                    radius = ChainRadius,
                    chains_left = 1,
                    initial_strikes = 1,
                });

                if (success)
                {
                    time_zap = Time.time + TIME_CHAIN_HIT;
                    _chain_hits--;

                    if(_chain_hits <= 0)
                    {
                        Kill();
                    }
                }
            }
            yield return null;
        }
    }
}