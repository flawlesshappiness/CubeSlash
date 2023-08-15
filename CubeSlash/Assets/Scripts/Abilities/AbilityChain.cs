using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityChain : Ability
{
    [Header("CHAIN")]
    [SerializeField] private Transform pivot_preview;
    [SerializeField] private SpriteRenderer spr_preview;
    [SerializeField] private DamageTrail trail;
    [SerializeField] private Projectile projectile_fragment;

    public GameAttribute Cooldown { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.chain_cooldown); } }
    public GameAttribute Radius { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.chain_radius); } }
    public GameAttribute Chains { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.chain_chains); } }
    public GameAttribute Strikes { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.chain_strikes); } }
    public GameAttribute ExplosionRadius { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.chain_explosion_radius); } }
    public GameAttribute Fragments { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.chain_fragments); } }

    private float time_attack;

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();

        trail.radius = 0.5f;
        trail.lifetime = 1.5f;
        trail.gameObject.SetActive(false);

        Radius.OnValueModified += () => UpdatePreviewRadius();
    }

    protected override void OnResume()
    {
        base.OnResume();

        var a = IsModifier() ? 0f : 0.05f;
        spr_preview.SetAlpha(a);
        UpdatePreviewRadius();
    }

    public override float GetBaseCooldown() => Cooldown.ModifiedValue.float_value;

    private void UpdatePreviewRadius() => pivot_preview.localScale = Vector3.one * Radius.ModifiedValue.float_value * 2;

    protected override void Update()
    {
        base.Update();
        if (IsModifier()) return;
        if (!IsEquipped()) return;
        if (Time.time < time_attack) return;

        var center = Player.Instance.transform.position;
        var success = TryChainToTarget(new ChainInfo
        {
            center = center,
            radius = Radius.ModifiedValue.float_value,
            chains_left = Chains.ModifiedValue.int_value,
            initial_strikes = Strikes.ModifiedValue.int_value,
            chain_strikes = 1,
            onHit = HitTarget
        });

        var time_success = Time.time + Cooldown.ModifiedValue.float_value * att_cooldown_multiplier.ModifiedValue.float_value;
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

    private void HitTarget(ChainInfo info, IKillable k)
    {
        var position_prev = info.center;
        var position = k.GetPosition();

        if (ExplosionRadius.ModifiedValue.float_value > 0)
        {
            StartCoroutine(ExplodeCr(position));
        }

        if (Fragments.ModifiedValue.bool_value)
        {
            trail.transform.position = position_prev;
            trail.ResetTrail();
            trail.transform.position = position;
            trail.CreateTrailsFromPreviousPosition();
        }

        if (false)
        {
            trail.radius = 1.5f;
            trail.lifetime = 1f;
            var t = trail.CreateTrail(position);
        }

        IEnumerator ExplodeCr(Vector3 position)
        {
            yield return new WaitForSeconds(0.25f);
            AbilityExplode.Explode(position, ExplosionRadius.ModifiedValue.float_value, 0);
        }
    }

    public class ChainInfo
    {
        public Vector3 center;
        public float radius;
        public int chains_left;
        public int initial_strikes;
        public int chain_strikes;
        public System.Action<ChainInfo, IKillable> onHit;
        public List<IKillable> hits = new List<IKillable>();

        public int debug_chain_hits;
    }

    public static bool TryChainToTarget(ChainInfo info)
    {
        var hits = Physics2D.OverlapCircleAll(info.center, info.radius)
            .Select(hit => hit.GetComponentInParent<IKillable>())
            .Where(hit => hit != null && hit.CanHit() && !info.hits.Contains(hit))
            .Distinct();

        if (hits.Count() == 0) return false;

        var count_hits = 0;
        var order_by_dist = hits.OrderBy(hit => Vector3.Distance(info.center, hit.GetPosition()));
        foreach (var hit in order_by_dist)
        {
            if (count_hits >= info.initial_strikes) break;
            count_hits++;

            ChainToTarget(hit, info);
        }

        return true;
    }

    public static void ChainToTarget(IKillable k, ChainInfo info)
    {
        Vector3 target_position = k.GetPosition();

        // Create particle system
        CreateZapPS(info.center, target_position);
        CreateImpactPS(target_position);

        // Audio
        SoundController.Instance.SetGroupVolumeByPosition(SoundEffectType.sfx_chain_zap, info.center);
        SoundController.Instance.PlayGroup(SoundEffectType.sfx_chain_zap);

        // Kill target
        info.onHit?.Invoke(info, k);
        Player.Instance.TryKillEnemy(k);
        info.debug_chain_hits++;

        // Keep chaining
        if (info.chains_left <= 1) return;
        CoroutineController.Instance.StartCoroutine(ChainCr());
        IEnumerator ChainCr()
        {
            yield return new WaitForSeconds(0.2f);

            info.center = target_position;
            info.chains_left--;
            info.initial_strikes = info.chain_strikes;
            info.hits.Add(k);
            TryChainToTarget(info);
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