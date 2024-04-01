using System.Collections.Generic;
using UnityEngine;

public class OrbitProjectile : Projectile
{
    [Header("ORBIT")]
    public ParticleSystem ps_chain;
    public OrbitProjectile prefab_mini_projectile;

    private bool chain_enabled;
    private float cd_chain;

    public bool mini_orbit_enabled;
    public AbilityOrbit.OrbitRing mini_orbit;
    public int mini_orbit_direction_override = 1;

    private void Start()
    {
        onDestroy += OnDestroyed;
    }

    private void OnDestroyed()
    {
        mini_orbit?.Clear();
    }

    public void SetChainEnabled(bool enabled)
    {
        chain_enabled = enabled;
        ps_chain.SetEmissionEnabled(enabled);
    }

    public void SetMiniOrbitEnabled(bool enabled)
    {
        if (enabled)
        {
            if (mini_orbit != null) return;
            mini_orbit = new AbilityOrbit.OrbitRing(this, transform)
            {
                OrbitTime = 1f,
                ProjectileCount = 2,
            };
        }
        else
        {
            mini_orbit?.Clear();
            mini_orbit = null;
        }
    }

    private void Update()
    {
        UpdateChain();
        UpdateMiniOrbit();
    }

    private void UpdateMiniOrbit()
    {
        if (mini_orbit != null)
        {
            mini_orbit.ProjectileSize = transform.localScale.x * 0.5f;
            mini_orbit.Radius = transform.localScale.x * 2.0f;
            mini_orbit.OverrideDirectionMul = mini_orbit_direction_override;
            mini_orbit.Update();
        }
    }

    private void UpdateChain()
    {
        if (!chain_enabled) return;
        if (Time.time < cd_chain) return;

        var info = new AbilityChain.ChainInfo
        {
            center = transform.position,
            radius = 6f,
            chains_left = 1,
            chain_strikes = 1,
            initial_strikes = 1,
            hits = new List<IKillable> { }
        };

        var success = AbilityChain.TryChainToTarget(info);
        if (success)
        {
            cd_chain = Time.time + 1.5f;
        }
    }
}