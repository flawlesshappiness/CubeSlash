using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityExplode : Ability
{
    public float Cooldown { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.explode_cooldown).ModifiedValue.float_value; } }
    public float Radius { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.explode_radius).ModifiedValue.float_value; } }
    public float ChargeTime { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.explode_charge_time).ModifiedValue.float_value; } }
    public float Slow { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.explode_slow).ModifiedValue.float_value; } }
    public bool ChainExplode { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.explode_chain).ModifiedValue.bool_value; } }
    public bool SplitExplode { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.explode_split).ModifiedValue.bool_value; } }
    public int Fragments { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.explode_fragments).ModifiedValue.int_value; } }
    public int MiniExplosions { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.explode_minis).ModifiedValue.int_value; } }

    private const float FORCE = 250f;

    private float time_charge_start;

    private List<ActiveCharge> _active_charges = new List<ActiveCharge>();

    private class ActiveCharge
    {
        public ExplodeChargeEffect fx;
        public float radius;
        public Vector3 dir;
    }

    public override float GetBaseCooldown() => Cooldown;

    public override void Pressed()
    {
        InUse = true;
        base.Pressed();
        time_charge_start = Time.time;

        if (SplitExplode)
        {
            var count = 3;
            var delta_angle = 360f / count;
            for (int i = 0; i < count; i++)
            {
                var angle = delta_angle * i;
                var dir = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.up;
                var fx = CreateChargeEffect(Vector3.zero, Vector3.one * Radius, ChargeTime);
                fx.transform.parent = Player.Body.transform;
                fx.transform.localPosition = Vector3.zero;

                var charge = new ActiveCharge
                {
                    fx = fx,
                    radius = Radius,
                    dir = dir,
                };
                _active_charges.Add(charge);
            }
        }
        else
        {
            var fx = CreateChargeEffect(Vector3.zero, Vector3.one * Radius, ChargeTime);
            fx.transform.parent = transform;
            fx.transform.localPosition = Vector3.zero;

            var charge = new ActiveCharge
            {
                fx = fx,
                radius = Radius
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

        var t = (time_charge_start + ChargeTime + 1);
        if (Time.time < t) return;

        Released();
    }

    private void Update()
    {
        ReleaseOverTime();
        UpdateChargePosition();
    }

    private float GetChargeValue() => (Time.time - time_charge_start) / ChargeTime;

    private void UpdateChargePosition()
    {
        if (_active_charges.Count == 0) return;

        var t = Mathf.Clamp01(GetChargeValue());
        foreach (var charge in _active_charges)
        {
            charge.fx.transform.localPosition = charge.dir * charge.radius * t * 1.25f;
        }
    }

    public override void Trigger()
    {
        if (InUse) return;
        base.Trigger();
        var t = Mathf.Clamp01(GetChargeValue());
        var f = FORCE * t / _active_charges.Count;
        var c = (Cooldown * 0.5f) + (Cooldown * 0.5f * t);

        foreach (var charge in _active_charges)
        {
            var r = charge.radius * t;

            var position = charge.fx.transform.position;
            Explode(position, r, f, OnHit);
            OnExplode(position, t);

            charge.fx.StopAnimating();
            Destroy(charge.fx.gameObject, 1);
        }
        _active_charges.Clear();

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
    }

    private void OnHit(IKillable k)
    {
        var position = k.GetPosition();

        if (ChainExplode)
        {
            AbilityChain.CreateImpactPS(position);

            StartCoroutine(ExplodeCr(new ChargeInfo
            {
                parent = GameController.Instance.world,
                radius = Random.Range(Radius * 0.25f, Radius * 0.5f),
                delay = Random.Range(0.3f, 0.6f),
                getPosition = () => position,
                play_charge_sfx = false,
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

        var fx_start = Vector3.zero;
        var fx_end = Vector3.one * info.radius;
        var fx = CreateChargeEffect(fx_start, fx_end, delay);
        fx.transform.parent = parent;
        fx.transform.position = getPosition();

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