using System.Collections;
using System.Linq;
using UnityEngine;

public class AbilityExplode : Ability
{
    [Header("EXPLODE")]
    [SerializeField] private ExplodeChargeEffect fx_charge;

    public event System.Action onChargeStart, onChargeEnd, onExplode;

    // Values
    public float Cooldown { get; private set; }
    public float Radius { get; private set; }
    public float ChargeTime { get; private set; }
    public float Knockback { get; private set; }
    public float SlowRadius { get; private set; }
    public float SlowPerc { get; private set; }
    public bool ChainExplode { get; private set; }
    public bool HasFragments { get; private set; }
    public bool SlowLinger { get; private set; }
    public int MiniExplosions { get; private set; }

    private const float RADIUS = 4f;
    private const float RADIUS_MUL_START = 0f;
    private const float CHARGE_TIME = 1f;
    private const float FORCE = 200f;

    private float time_charge_start;

    private SlowArea _slow_area;

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();
    }

    public override void OnValuesUpdated()
    {
        base.OnValuesUpdated();
        Cooldown = GetFloatValue(StatID.explode_cooldown_flat) * GetFloatValue(StatID.explode_cooldown_perc);
        Radius = RADIUS * GetFloatValue(StatID.explode_radius_perc);
        Knockback = FORCE;
        ChargeTime = CHARGE_TIME * GetFloatValue(StatID.explode_charge_time);
        SlowPerc = 1f - GetFloatValue(StatID.explode_slow_perc);
        SlowRadius = RADIUS * GetFloatValue(StatID.explode_slow_area_perc);
        ChainExplode = GetBoolValue(StatID.explode_chain);
        HasFragments = GetBoolValue(StatID.explode_fragments);
        SlowLinger = GetBoolValue(StatID.explode_slow_linger);
        MiniExplosions = GetIntValue(StatID.explode_minis);
    }

    public override float GetBaseCooldown() => Cooldown;

    public override void Pressed()
    {
        InUse = true;
        base.Pressed();
        time_charge_start = Time.time;

        onChargeStart?.Invoke();

        var parent = HasFragments ? GameController.Instance.world : transform;
        var position = transform.position;

        fx_charge.transform.parent = parent;
        fx_charge.transform.position = position;
        fx_charge.Animate(Vector3.one * Radius * RADIUS_MUL_START, Vector3.one * Radius, ChargeTime);

        _slow_area = SlowArea.Create();
        _slow_area.transform.parent = parent;
        _slow_area.transform.position = position;
        _slow_area.SetSlowPercentage(SlowPerc);
    }

    public override void Released()
    {
        base.Released();
        if (!InUse) return;

        InUse = false;
        onChargeEnd?.Invoke();

        fx_charge.StopAnimating();

        if (SlowLinger)
        {
            _slow_area.transform.parent = GameController.Instance.world;
            _slow_area.Destroy(4f);
        }
        else
        {
            Destroy(_slow_area.gameObject);
        }

        Trigger();
    }

    private void ReleaseOverTime()
    {
        if (!InUse) return;
        
        var t = (time_charge_start + ChargeTime + 1);
        if (Time.time < t) return;
        
        Released();
    }

    private void Update()
    {
        ReleaseOverTime();
        SlowEnemiesInArea();
    }

    private float GetChargeValue() => (Time.time - time_charge_start) / ChargeTime;

    private void SlowEnemiesInArea()
    {
        if (!InUse) return;
        if (_slow_area == null) return;

        var t = Mathf.Clamp01(GetChargeValue());
        var r_end = (Radius + SlowRadius);
        var r_start = r_end * RADIUS_MUL_START;
        var r = Mathf.Lerp(r_start, r_end, t);
        _slow_area.SetRadius(r);
    }

    public override void Trigger()
    {
        if (InUse) return;
        base.Trigger();
        var t = Mathf.Clamp01(GetChargeValue());
        var r = Radius * t;
        var f = Knockback * t;
        var c = (Cooldown * 0.5f) + (Cooldown * 0.5f * t);
        Explode(_slow_area.transform.position, r, f, OnHit);
        OnExplode(_slow_area.transform.position, t);
        StartCooldown(c);
    }

    private void OnExplode(Vector3 position, float t = 1)
    {
        InUse = false;
        StartCooldown();
        Player.Instance.AbilityLock.RemoveLock(nameof(AbilityExplode));

        for (int i = 0; i < MiniExplosions; i++)
        {
            var radius = Radius * Random.Range(0.3f, 0.75f);
            var dir = Random.insideUnitCircle.ToVector3().normalized;
            var pos = position + dir * (Radius + radius);
            var delay = Random.Range(0.3f, 0.6f);

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

        onExplode?.Invoke();
    }

    private void OnHit(IKillable k)
    {
        var position = k.GetPosition();

        if (ChainExplode)
        {
            AbilityChain.CreateImpactPS(position);
        }

        if (ChainExplode)
        {
            StartCoroutine(ExplodeCr(new ChargeInfo
            {
                parent = GameController.Instance.world,
                radius = Random.Range(Radius * 0.25f, Radius * 0.5f),
                delay = Random.Range(0.3f, 0.6f),
                getPosition = () => position,
                play_charge_sfx = false,
                onHit = OnHit,
            }));
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

        CreateChargeEffect(parent, getPosition(), radius, delay);

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
        foreach(var hit in hits)
        {
            var k = hit.GetComponentInParent<IKillable>();
            if (k == null) continue;

            if (k.CanKill())
            {
                onHit?.Invoke(k);
                Player.Instance.KillEnemy(k);
            }
        }

        // Knockback
        if(force > 0)
        {
            Player.PushEnemiesInArea(position, radius * 3, force);
        }

        // Fx
        CreateExplodeEffect(position, radius);
    }

    public static void CreateChargeEffect(Transform parent, Vector3 position, float radius, float duration)
    {
        var ps = Resources.Load<ParticleSystem>("Particles/ps_explode_charge");
        var psd = ps.Duplicate()
            .Parent(parent)
            .Position(position)
            .Scale(Vector3.one * radius * 2);

        psd.ps.ModifyMain(m =>
        {
            m.startLifetime = new ParticleSystem.MinMaxCurve { constant = duration };
        });
        psd.Play();
    }

    public static void CreateExplodeEffect(Vector3 position, float radius, Color? color = null)
    {
        Color actual_color = color ?? Color.white;

        var template_explosion = Resources.Load<ExplosionEffect>("Particles/ExplosionEffect");
        var explosion = Instantiate(template_explosion, GameController.Instance.world);
        explosion.transform.position = position;
        explosion.transform.localScale = Vector3.one * radius * 2;
        explosion.SetColor(actual_color);
        Destroy(explosion, 2f);

        var ps_explode = Resources.Load<ParticleSystem>("Particles/ps_explode");
        ps_explode.Duplicate()
            .Scale(Vector3.one * radius * 2)
            .Position(position)
            .Play();

        // Sfx
        SoundController.Instance.PlayGroup(SoundEffectType.sfx_explode_release);
    }
}