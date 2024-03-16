using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplitProjectile : Projectile
{
    public bool HasChain { get; set; } = false;
    public float ChainRadius { get; set; } = 7f;
    public float Velocity { get; set; }
    public Vector3 StartPosition { get; set; }
    public bool IsBoomerang { get; set; }
    public float BoomerangDistance { get; set; }

    [Header(nameof(SplitProjectile))]
    [SerializeField] private ParticleSystem ps_chain;
    [SerializeField] private SpriteRenderer spr;

    private bool _returning;

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

    public override void Update()
    {
        base.Update();
        UpdateDistance();
        UpdateCatch();
    }

    private void FixedUpdate()
    {
        UpdateReturn();
    }

    private void UpdateDistance()
    {
        if (!IsBoomerang) return;
        if (_returning) return;

        var current_distance = Vector3.Distance(transform.position, StartPosition);
        if (current_distance < BoomerangDistance) return;

        _returning = true;
    }

    private void UpdateReturn()
    {
        if (!IsBoomerang) return;
        if (!_returning) return;

        BoomerangProjectile.UpdateReturnProjectile(this, Player.Instance.transform.position, Velocity);
    }

    private void UpdateCatch()
    {
        if (!IsBoomerang) return;
        if (!_returning) return;

        BoomerangProjectile.TryCatch(this, Player.Instance.transform.position);
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