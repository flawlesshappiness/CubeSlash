using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.HID;

public class AbilityChain : Ability
{
    public float Radius { get; private set; }
    public float Frequency { get; private set; }
    public float FrequencyPerc { get; private set; }
    public int Chains { get; private set; }
    public int Strikes { get; private set; }
    public int ChainStrikes { get; private set; }
    public bool HitsExplode { get; private set; }

    private float time_attack;

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();
    }

    public override void OnValuesApplied()
    {
        base.OnValuesApplied();

        Radius = GetFloatValue("Radius");
        Frequency = GetFloatValue("Frequency");
        FrequencyPerc = GetFloatValue("FrequencyPerc");
        Chains = GetIntValue("Chains");
        Strikes = GetIntValue("Strikes");
        ChainStrikes = GetIntValue("ChainStrikes");
        HitsExplode = GetBoolValue("HitsExplode");
    }

    public override void Pressed()
    {
        base.Pressed();
        if (HasModifier(Type.CHARGE)) return;
        InUse = true;
        Player.Instance.AbilityLock.AddLock(nameof(AbilityChain));
    }

    public override void Released()
    {
        base.Released();
        if (HasModifier(Type.CHARGE)) return;
        InUse = false;
        Player.Instance.AbilityLock.RemoveLock(nameof(AbilityChain));
    }

    public override void Trigger()
    {
        base.Trigger();
        var center = Player.Instance.transform.position;
        TryChainToTarget(center, Radius, Chains, Strikes, ChainStrikes, HitTarget);
    }

    private void Update()
    {
        if (!InUse) return;
        if (Time.time < time_attack) return;

        var center = Player.Instance.transform.position;
        var success = TryChainToTarget(center, Radius, Chains, Strikes, ChainStrikes, HitTarget);

        if (success)
        {
            time_attack = Time.time + Frequency * FrequencyPerc;
        }
        else
        {
            time_attack = Time.time + 0.1f;
        }
    }

    private void HitTarget(IKillable k)
    {
        var position = k.GetPosition();

        if (HitsExplode)
        {
            StartCoroutine(ExplodeCr(position));
        }

        IEnumerator ExplodeCr(Vector3 position)
        {
            yield return new WaitForSeconds(0.25f);
            AbilityExplode.Explode(position, 2f, 0);
        }
    }

    public static bool TryChainToTarget(Vector3 center, float radius, int chains_left, int strikes, int chain_strikes, System.Action<IKillable> onHit = null)
    {
        var hits = Physics2D.OverlapCircleAll(center, radius)
            .Select(hit => hit.GetComponentInParent<IKillable>())
            .Where(hit => hit != null && hit.CanKill())
            .Distinct();

        if (hits.Count() == 0) return false;

        var order_by_dist = hits.OrderBy(hit => Vector3.Distance(center, hit.GetPosition()));
        var enemies_to_strike = order_by_dist.Take(Mathf.Max(1, strikes));

        foreach(var e in enemies_to_strike)
        {
            ChainToTarget(e, center, radius, chains_left, chain_strikes, onHit);
        }
        return true;
    }

    public static void ChainToTarget(IKillable k, Vector3 center, float radius, int chains_left, int strikes, System.Action<IKillable> onHit = null)
    {
        Vector3 target_position = k.GetPosition();

        // Create particle system
        CreateZapPS(center, target_position);
        CreateImpactPS(target_position);
        
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
}