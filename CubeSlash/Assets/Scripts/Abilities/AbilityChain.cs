using Flawliz.Lerp;
using System.Collections;
using System.Linq;
using UnityEngine;

public class AbilityChain : Ability
{
    [Header("CHAIN")]
    [SerializeField] private Transform pivot_preview;
    [SerializeField] private SpriteRenderer spr_preview;
    [SerializeField] private DamageTrail trail;
    [SerializeField] private Projectile projectile_fragment;

    public float Cooldown { get; private set; }
    public float Radius { get; private set; }
    public int Chains { get; private set; }
    public int Strikes { get; private set; }
    public int ChainSplits { get; private set; }
    public bool HitsExplode { get; private set; }
    public bool HitsFragment { get; private set; }
    public bool Trail { get; private set; }

    private float time_attack;

    private const float RADIUS = 6;

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();

        trail.gameObject.SetActive(false);
    }

    public override void OnValuesUpdated()
    {
        base.OnValuesUpdated();

        Cooldown = GetFloatValue(StatID.chain_cooldown_flat) * GetFloatValue(StatID.chain_cooldown_perc);
        Radius = RADIUS * GetFloatValue(StatID.chain_radius_perc);
        Chains = GetIntValue(StatID.chain_chains);
        Strikes = GetIntValue(StatID.chain_strikes);
        ChainSplits = GetIntValue(StatID.chain_chain_strikes);
        HitsExplode = GetBoolValue(StatID.chain_hits_explode);
        HitsFragment = GetBoolValue(StatID.chain_fragments);
        Trail = GetBoolValue(StatID.chain_trail);

        pivot_preview.localScale = Vector3.one * Radius * 2;
        spr_preview.SetAlpha(0);
    }


    public override float GetBaseCooldown() => Cooldown;
    public override bool CanPressWhileOnCooldown() => true;

    public override void Pressed()
    {
        base.Pressed();
        AnimateShowPreview(true);
        InUse = true;
        Player.Instance.AbilityLock.AddLock(nameof(AbilityChain));
    }

    public override void Released()
    {
        base.Released();
        AnimateShowPreview(false);
        InUse = false;
        Player.Instance.AbilityLock.RemoveLock(nameof(AbilityChain));
    }

    private void Update()
    {
        if (!InUse) return;
        if (Time.time < time_attack) return;

        var center = Player.Instance.transform.position;
        var success = TryChainToTarget(center, Radius, Chains, Strikes, ChainSplits, HitTarget);

        var time_success = Time.time + Cooldown * Player.Instance.GlobalCooldownMultiplier;
        var time_fail = Time.time + 0.1f;

        if (success)
        {
            time_attack = time_success;
            StartCooldown();
        }
        else
        {
            time_attack = time_fail;
        }
    }

    private void HitTarget(IKillable k)
    {
        var position = k.GetPosition();

        if (HitsExplode)
        {
            StartCoroutine(ExplodeCr(position));
        }

        if (HitsFragment)
        {
            var distance = 6f;
            var speed = 10f;
            var size = 0.5f;
            var lifetime = Calculator.DST_Time(distance, speed);
            var fragments = AbilityMines.ShootFragments(position, projectile_fragment, 3, speed, size);
            fragments.ForEach(f => f.Lifetime = lifetime);
        }

        if (Trail)
        {
            var radius = 1.5f;
            var lifetime = 1f;
            trail.radius = radius;
            trail.lifetime = lifetime;
            var t = trail.CreateTrail(position);
        }

        IEnumerator ExplodeCr(Vector3 position)
        {
            yield return new WaitForSeconds(0.25f);
            AbilityExplode.Explode(position, 3f, 0);
        }
    }

    public static bool TryChainToTarget(Vector3 center, float radius, int chains_left, int strikes, int chain_strikes, System.Action<IKillable> onHit = null)
    {
        var hits = Physics2D.OverlapCircleAll(center, radius)
            .Select(hit => hit.GetComponentInParent<IKillable>())
            .Where(hit => hit != null && hit.CanKill())
            .Distinct();

        if (hits.Count() == 0) return false;

        var count_hits = 0;
        var order_by_dist = hits.OrderBy(hit => Vector3.Distance(center, hit.GetPosition()));
        foreach(var hit in order_by_dist)
        {
            if (!hit.CanKill()) continue;
            if (count_hits >= strikes) break;
            count_hits++;

            ChainToTarget(hit, center, radius, chains_left, chain_strikes, onHit);
        }

        return true;
    }

    public static void ChainToTarget(IKillable k, Vector3 center, float radius, int chains_left, int strikes, System.Action<IKillable> onHit = null)
    {
        Vector3 target_position = k.GetPosition();

        // Create particle system
        CreateZapPS(center, target_position);
        CreateImpactPS(target_position);

        // Audio
        SoundController.Instance.Play(SoundEffectType.sfx_chain_zap);
        
        // Kill target
        onHit?.Invoke(k);
        Player.Instance.KillEnemy(k);

        // Keep chaining
        if (chains_left <= 1) return;
        CoroutineController.Instance.StartCoroutine(ChainCr());
        IEnumerator ChainCr()
        {
            yield return new WaitForSeconds(0.1f);
            TryChainToTarget(target_position, radius, chains_left - 1, strikes, strikes, onHit);
        }
    }

    public static void CreateZapPS(Vector3 center, Vector3 target)
    {
        var template = Resources.Load<ParticleSystem>("Particles/ps_chain_zap");
        var ps = Instantiate(template, GameController.Instance.world);
        ps.transform.position = center;

        var dir = target - center;
        var angle = Vector3.SignedAngle(Vector3.up, dir, Vector3.forward);
        var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        ps.transform.rotation = rotation;

        var scale = dir.magnitude;
        ps.transform.localScale = Vector3.one * scale;

        ps.Play();
        Destroy(ps.gameObject, 2f);
    }

    public static void CreateImpactPS(Vector3 center)
    {
        var template = Resources.Load<ParticleSystem>("Particles/ps_chain_impact");
        var ps = Instantiate(template, GameController.Instance.world);
        ps.transform.position = center;

        ps.Play();
        Destroy(ps.gameObject, 5f);
    }

    private CustomCoroutine AnimateShowPreview(bool show)
    {
        return this.StartCoroutineWithID(Cr(), "show_preview_" + GetInstanceID());
        IEnumerator Cr()
        {
            var end = show ? 0.05f : 0f;
            yield return LerpEnumerator.Alpha(spr_preview, 0.25f, end);
        }
    }
}