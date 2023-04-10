using System.Collections;
using UnityEngine;

public class MinesProjectile : Projectile
{
    public bool HasTrail { get; set; }
    public bool HasChain { get; set; }
    public int ChainHits { get; set; }
    public float ChainRadius { get; set; } = 5f;

    [Header(nameof(SplitProjectile))]
    [SerializeField] private Collider2D trigger;
    [SerializeField] private ParticleSystem ps_gas;
    [SerializeField] private ParticleSystem ps_chain;
    [SerializeField] private SpriteRenderer spr;

    private int _chain_hits;

    private const float TIME_CHAIN_HIT = 0.5f;

    protected override void Start()
    {
        spr.enabled = !HasChain;
        trigger.enabled = !HasChain;

        if (HasTrail)
        {
            ps_gas.Play();
        }

        if (HasChain)
        {
            ps_chain.Play();
            _chain_hits = ChainHits;
            Lifetime = TIME_CHAIN_HIT * (_chain_hits + 1);
            StartCoroutine(ChainLightningCr());
        }

        base.Start();
    }

    IEnumerator ChainLightningCr()
    {
        var time_zap = Time.time + TIME_CHAIN_HIT;
        while (true)
        {
            if (Time.time > time_zap)
            {
                var success = AbilityChain.TryChainToTarget(transform.position, ChainRadius, 1, 1, 1);
                if (success)
                {
                    time_zap = Time.time + TIME_CHAIN_HIT;
                    _chain_hits--;

                    if (_chain_hits <= 0)
                    {
                        Kill();
                    }
                }
            }
            yield return null;
        }
    }
}