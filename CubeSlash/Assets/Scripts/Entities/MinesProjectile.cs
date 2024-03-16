using UnityEngine;

public class MinesProjectile : Projectile
{
    public bool HasTrail { get; set; }
    public bool HasChain { get; set; }

    [Header(nameof(SplitProjectile))]
    [SerializeField] private Collider2D trigger;
    [SerializeField] private ParticleSystem ps_gas;
    [SerializeField] private ParticleSystem ps_chain;
    [SerializeField] private SpriteRenderer spr;

    protected override void Start()
    {
        spr.enabled = true;
        trigger.enabled = true;

        if (HasTrail)
        {
            ps_gas.Play();
        }

        if (HasChain)
        {
            ps_chain.Play();
        }

        base.Start();
    }
}