using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityExplode : Ability
{
    [SerializeField] private ParticleSystem ps_charge_reduc;

    public float Cooldown { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.explode_cooldown).ModifiedValue.float_value; } }
    public float Radius { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.explode_radius).ModifiedValue.float_value; } }
    public float ChargeTime { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.explode_charge_time).ModifiedValue.float_value; } }
    public float ChargeTimePerc { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.explode_charge_time_perc).ModifiedValue.float_value; } }
    public float ChargeTimeReduc { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.explode_charge_time_reduc).ModifiedValue.float_value; } }
    public float Slow { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.explode_slow).ModifiedValue.float_value; } }
    public bool ChainExplode { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.explode_chain).ModifiedValue.bool_value; } }
    public bool SplitExplode { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.explode_split).ModifiedValue.bool_value; } }
    public int Fragments { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.explode_fragments).ModifiedValue.int_value; } }
    public int MiniExplosions { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.explode_minis).ModifiedValue.int_value; } }
    public bool DelayedExplosion { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.explode_delayed).ModifiedValue.bool_value; } }

    private const float FORCE = 250f;
    private const float CHARGE_TIME_PER_SIZE = 0.27f;

    private float time_charge_start;
    private float time_charge_current;
    private float time_charge_reduced;
    private int count_killed;

    private Coroutine cr_charge_reduc_ps;

    private List<ActiveCharge> _active_charges = new List<ActiveCharge>();

    private class ActiveCharge
    {
        public ExplodeChargeEffect fx;
        public Vector3 dir;
    }

    public override float GetBaseCooldown() => Cooldown;

    public override Dictionary<string, string> GetStats()
    {
        var stats = base.GetStats();

        stats.Add("Charge time", CalculateChargeTime().ToString("0.00"));
        stats.Add("Radius", Radius.ToString("0.00"));

        return stats;
    }

    private float CalculateChargeTime()
    {
        return Radius * CHARGE_TIME_PER_SIZE * ChargeTimePerc * att_cooldown_multiplier.ModifiedValue.float_value;
    }

    public override void Pressed()
    {
        InUse = true;
        base.Pressed();
        time_charge_start = Time.time;

        time_charge_current = CalculateChargeTime();
        time_charge_reduced = 0;
        count_killed = 0;

        SoundController.Instance.Play(SoundEffectType.sfx_explode_charge);

        if (SplitExplode)
        {
            var count = 3;
            var max_angle = 180f + Radius * 5f;
            var delta_angle = max_angle / count;
            var start_angle = -delta_angle;
            for (int i = 0; i < count; i++)
            {
                var angle = start_angle + delta_angle * i;
                var dir = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.up;
                var fx = CreateChargeEffect(Vector3.zero, Vector3.one * Radius, time_charge_current);
                fx.transform.parent = Player.Body.transform;
                fx.transform.localPosition = Vector3.zero;

                var charge = new ActiveCharge
                {
                    fx = fx,
                    dir = dir,
                };
                _active_charges.Add(charge);
            }
        }
        else
        {
            var fx = CreateChargeEffect(Vector3.zero, Vector3.one * Radius, time_charge_current);
            fx.transform.parent = transform;
            fx.transform.localPosition = Vector3.zero;

            var charge = new ActiveCharge
            {
                fx = fx,
                dir = Vector3.zero,
            };
            _active_charges.Add(charge);
        }
    }

    public override void Released()
    {
        base.Released();
        if (!InUse) return;
        InUse = false;
        Trigger();
    }

    private void ReleaseOverTime()
    {
        if (!InUse) return;

        var t = (time_charge_start + time_charge_current + 1);
        if (Time.time < t) return;

        Released();
    }

    protected override void Update()
    {
        base.Update();
        ReleaseOverTime();
        UpdateChargePosition();
    }

    private float GetChargeValue() => time_charge_current <= 0 ? 1 : (Time.time - time_charge_start) / time_charge_current;

    private void UpdateChargePosition()
    {
        if (_active_charges.Count == 0) return;

        var t = Mathf.Clamp01(GetChargeValue());
        foreach (var charge in _active_charges)
        {
            charge.fx.transform.localPosition = charge.dir * Radius * t * 1.05f;
        }
    }

    private IEnumerator ChargeReducCr()
    {
        while (count_killed > 0 && time_charge_reduced > 0)
        {
            count_killed--;
            ps_charge_reduc.ModifyMain(m => m.startSize = new ParticleSystem.MinMaxCurve { constant = Radius }, affectChildren: false);
            ps_charge_reduc.Play();
            yield return new WaitForSeconds(0.1f);
        }
        cr_charge_reduc_ps = null;
    }

    public override void Trigger()
    {
        if (InUse) return;
        base.Trigger();
        var t = Mathf.Clamp01(GetChargeValue());
        var f = FORCE * t / _active_charges.Count;
        var c = (Cooldown * 0.5f) + (Cooldown * 0.5f * t);
        var r = Radius * t;

        foreach (var charge in _active_charges)
        {
            DoExplosion(charge);

            if (DelayedExplosion)
            {
                StartCoroutine(DelayedExplosionCr(charge.fx.transform.position, 2f));
            }
        }
        _active_charges.Clear();

        InUse = false;
        StartCooldown(c);
        Player.Instance.AbilityLock.RemoveLock(nameof(AbilityExplode));

        void DoExplosion(ActiveCharge charge)
        {
            var position = charge.fx.transform.position;
            Explode(position, r, f, OnHit);
            OnExplode(position, t);

            charge.fx.StopAnimating();
            Destroy(charge.fx.gameObject, 1);
        }

        IEnumerator DelayedExplosionCr(Vector3 position, float delay)
        {
            yield return new WaitForSeconds(delay);
            Explode(position, r, f, OnHit);
            OnExplode(position, t);
        }
    }

    private void OnExplode(Vector3 position, float t = 1)
    {
        // Mini explosions
        var mini_max_radius = Radius * 2f;
        var mini_targets = EnemyController.Instance.ActiveEnemies
            .Where(e => Vector3.Distance(e.transform.position, Player.transform.position) < mini_max_radius)
            .OrderBy(e => Vector3.Distance(e.transform.position, Player.transform.position))
            .ToList();

        for (int i = 0; i < (int)(MiniExplosions * t); i++)
        {
            var target = i < mini_targets.Count ? mini_targets[i] : null;

            var radius = Radius * Random.Range(0.3f, 0.5f);
            var dir = Random.insideUnitCircle.ToVector3().normalized;
            var rnd_pos = position + dir * (Radius + radius);
            var pos = target == null ? rnd_pos : target.transform.position;
            var delay = Random.Range(0.2f, 1.0f);

            StartCoroutine(ExplodeCr(new ChargeInfo
            {
                parent = GameController.Instance.world,
                radius = radius,
                delay = delay,
                getPosition = () => pos,
                play_charge_sfx = false,
                onHit = OnHit,
            }));
        }
    }

    private void OnHit(IKillable k)
    {
        var position = k.GetPosition();

        if (time_charge_reduced < ChargeTimeReduc * 10)
        {
            time_charge_reduced += ChargeTimeReduc;

            count_killed += count_killed < 3 ? 1 : 0;
            cr_charge_reduc_ps ??= StartCoroutine(ChargeReducCr());
        }

        if (ChainExplode)
        {
            AbilityChain.CreateImpactPS(position);
            StartCoroutine(ChainCr());
        }

        IEnumerator ChainCr()
        {
            yield return new WaitForSeconds(Random.Range(0.5f, 1.0f));

            AbilityChain.TryChainToTarget(new AbilityChain.ChainInfo
            {
                center = position,
                chain_strikes = 1,
                initial_strikes = 1,
                radius = 6f,
                onHit = (info, k) =>
                {
                    Explode(k.GetPosition(), Radius * 0.25f, 0f);
                }
            });
        }
    }

    public class ChargeInfo
    {
        public Transform parent;
        public float radius;
        public float delay;
        public float force;
        public bool play_charge_sfx;
        public System.Func<Vector3> getPosition;
        public System.Action<IKillable> onHit;
        public System.Action<Vector3> onExplode;
    }

    public static IEnumerator ExplodeCr(ChargeInfo info)
    {
        var parent = info.parent;
        var radius = info.radius;
        var delay = info.delay;
        var force = info.force;
        var onHit = info.onHit;
        var getPosition = info.getPosition;

        /*
        var fx_start = Vector3.zero;
        var fx_end = Vector3.one * info.radius;
        var fx = CreateChargeEffect(fx_start, fx_end, delay);
        fx.transform.parent = parent;
        fx.transform.position = getPosition();
        */

        var sfx_charge = SoundController.Instance.CreateInstance(SoundEffectType.sfx_explode_charge);
        if (info.play_charge_sfx) sfx_charge.Play();

        yield return new WaitForSeconds(delay);

        sfx_charge.Stop();

        var position = getPosition();
        Explode(position, radius, force, onHit);
        info.onExplode?.Invoke(position);
    }

    public static void Explode(Vector3 position, float radius, float force, System.Action<IKillable> onHit = null)
    {
        var hits = Physics2D.OverlapCircleAll(position, radius);
        foreach (var hit in hits)
        {
            var k = hit.GetComponentInParent<IKillable>();
            if (k == null) continue;

            if (Player.Instance.TryKillEnemy(k))
            {
                onHit?.Invoke(k);
            }
        }

        // Knockback
        if (force > 0)
        {
            Player.PushEnemiesInArea(position, radius * 3, force);
        }

        // Fx
        CreateExplodeEffect(position, radius);
    }

    public static ExplodeChargeEffect CreateChargeEffect(Vector3 start, Vector3 end, float duration)
    {
        var prefab = Resources.Load<ExplodeChargeEffect>("Particles/ExplodeChargeEffect");
        var fx = Instantiate(prefab);
        fx.Animate(start, end, duration);
        return fx;
    }

    public static void CreateExplodeEffect(Vector3 position, float radius, Color? color = null)
    {
        Color actual_color = color ?? Color.white;

        var template_explosion = Resources.Load<ExplosionEffect>("Particles/ExplosionEffect");
        var explosion = Instantiate(template_explosion, GameController.Instance.world);
        explosion.transform.position = position;
        explosion.transform.localScale = Vector3.one * radius * 2;
        explosion.SetColor(actual_color);
        Destroy(explosion.gameObject, 2f);

        var ps_explode = Resources.Load<ParticleSystem>("Particles/ps_explode");
        ps_explode.Duplicate()
            .Scale(Vector3.one * radius * 2)
            .Position(position)
            .Play()
            .Destroy(2);

        // Sfx
        SoundController.Instance.SetGroupVolumeByPosition(SoundEffectType.sfx_explode_release, position);
        SoundController.Instance.PlayGroup(SoundEffectType.sfx_explode_release);
    }
}