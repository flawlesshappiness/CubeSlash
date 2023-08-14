using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitProjectile : Projectile
{
    public bool HasChain { get; set; } = false;
    public float ChainRadius { get; set; } = 7f;

    [Header(nameof(SplitProjectile))]
    [SerializeField] private ParticleSystem ps_chain;
    [SerializeField] private SpriteRenderer spr;

    protected override void Start()
    {
        spr.enabled = !HasChain;

        if (HasChain)
        {
            ps_chain.Play();
            onHitEnemy += OnHitEnemyChain;
        }

        base.Start();
    }

    private void OnHitEnemyChain(IKillable k)
    {
        AbilityChain.CreateImpactPS(k.GetPosition());

        CoroutineController.Instance.StartCoroutine(Cr());
        IEnumerator Cr()
        {
            var info = new AbilityChain.ChainInfo
            {
                center = transform.position,
                radius = ChainRadius,
                chains_left = 2,
                chain_strikes = 1,
                initial_strikes = 1,
                hits = new List<IKillable> { k }
            };

            yield return new WaitForSeconds(0.2f);

            var success = AbilityChain.TryChainToTarget(info);
        }
    }
}