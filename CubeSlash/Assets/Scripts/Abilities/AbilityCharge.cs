using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityCharge : Ability
{
    [Header("CHARGE")]
    [SerializeField] private ChargeBeam template_beam;
    [SerializeField] private DamageTrail template_trail;
    [SerializeField] private Projectile template_fragment_projectile;
    [SerializeField] private ParticleSystem ps_charge;
    [SerializeField] private ParticleSystem ps_charge_end;
    [SerializeField] private ParticleSystem ps_charged;
    [SerializeField] private ParticleSystem ps_beam_dust;
    [SerializeField] private AnimationCurve ac_charge_emission;
    [SerializeField] private AnimationCurve ac_charge_suck_falloff;

    private class BeamInfo
    {
        public ChargeBeam graphic;
        public Vector3 localDirection;
        public float delay;
    }

    private const float DISTANCE_MAX = 50f;
    private const int COUNT_EMISSION_PS_MIN = 10;
    private const int COUNT_EMISSION_PS_MAX = 50;

    private const float WIDTH = 1f;
    private const float CHARGE_TIME = 1.5f;
    private const float FORCE = 200f;
    private const float FORCE_SELF = 500f;

    private float time_charge_start;
    private float time_charge_end;

    private List<BeamInfo> beam_infos = new List<BeamInfo>();

    public bool Charging { get; private set; }
    public bool ChargeEnded { get; private set; }
    public int Kills { get; private set; }

    // Values
    private int BeamCount { get; set; }
    private float BeamArc { get; set; }
    private float Width { get; set; }
    public float ChargeTime { get; set; }
    private float Knockback { get; set; }
    private float KnockbackSelf { get; set; }
    private float ChargeTimeOnKill { get; set; }
    private bool ChargeSucksExp { get; set; }
    private bool BeamBack { get; set; }
    private bool ChainLightning { get; set; }
    private bool EnemyFragments { get; set; }

    private FMODEventInstance sfx_charge_start;
    private FMODEventInstance sfx_charge_idle;

    private void Start()
    {
        ps_charge.Play();
        ps_charge.ModifyEmission(e =>
        {
            e.enabled = false;
        });
        ps_charged.Play();
        ps_charged.ModifyEmission(e =>
        {
            e.enabled = false;
        });
    }

    private void OnDisable()
    {
        sfx_charge_idle?.Stop();
        sfx_charge_start?.Stop();
    }

    public override void InitializeFirstTime()
    {
        base.InitializeFirstTime();

        template_trail.gameObject.SetActive(false);
    }

    public override void OnValuesApplied()
    {
        base.OnValuesApplied();
        Width = WIDTH * GetFloatValue("Width");
        ChargeTime = CHARGE_TIME * GetFloatValue("ChargeTime");
        KnockbackSelf = FORCE_SELF * GetFloatValue("KnockbackSelf");
        Knockback = FORCE * GetFloatValue("Knockback");
        BeamCount = GetIntValue("BeamCount");
        BeamArc = GetFloatValue("BeamArc");
        BeamBack = GetBoolValue("BeamBack");
        ChargeSucksExp = GetBoolValue("ChargeSucksExp");
        ChargeTimeOnKill = GetFloatValue("ChargeTimeOnKill");
        ChainLightning = GetBoolValue("ChainLightning");
        EnemyFragments = GetBoolValue("EnemyFragments");

        Charging = false;
        ChargeEnded = false;
        Kills = 0;

        InitializeBeams();
    }

    public override void Pressed()
    {
        base.Pressed();
        BeginCharge();
        ShowBeamPreviews();
    }

    public override void Released()
    {
        base.Released();
        if (EndCharge() && IsActive)
        {
            Trigger();
        }
        else
        {
            SoundController.Instance.Play(SoundEffectType.sfx_ability_cooldown);
            HideBeamPreviews();
        }
    }

    public override void Trigger()
    {
        base.Trigger();
        Shoot(DISTANCE_MAX);
    }

    private Coroutine _cr_charge;
    public void BeginCharge()
    {
        Player.Instance.AbilityLock.AddLock(nameof(AbilityCharge));

        InUse = true;
        Charging = true;
        ChargeEnded = false;

        sfx_charge_start = SoundController.Instance.Play(SoundEffectType.sfx_charge_start);

        if (_cr_charge != null) StopCoroutine(_cr_charge);
        _cr_charge = StartCoroutine(ChargeCr());
        IEnumerator ChargeCr()
        {
            var time = GetChargeTime();
            time_charge_start = Time.time;
            time_charge_end = time_charge_start + time;
            yield return new WaitForSeconds(time);
        }
    }

    public bool EndCharge()
    {
        InUse = false;
        Charging = false;
        ChargeEnded = false;

        sfx_charge_start?.Stop();
        sfx_charge_idle?.Stop();

        Player.Instance.AbilityLock.RemoveLock(nameof(AbilityCharge));
        return IsFullyCharged();
    }

    private void Update()
    {
        ChargeUpdate();
        SuckUpdate();
        VisualUpdate();
    }

    private void ChargeUpdate()
    {
        var t = GetCharge();
        ps_charge.ModifyEmission(e =>
        {
            e.enabled = Charging && !ChargeEnded;
            var t_rate = ac_charge_emission.Evaluate(t);
            e.rateOverTime = Mathf.Lerp(COUNT_EMISSION_PS_MIN, COUNT_EMISSION_PS_MAX, t_rate);
        });
        ps_charged.ModifyEmission(e =>
        {
            e.enabled = Charging && ChargeEnded;
        });

        if (Charging && !ChargeEnded && t >= 1)
        {
            ChargeEnded = true;
            ps_charge_end.Play();
            sfx_charge_start?.Stop();
            sfx_charge_idle = SoundController.Instance.Play(SoundEffectType.sfx_charge_idle);
        }
    }

    private void VisualUpdate()
    {
        UpdateBeamPositions();
    }
    
    private void SuckUpdate()
    {
        if(Charging && !ChargeEnded && ChargeSucksExp)
        {
            var speed_max = 8f;
            var dist_max = CameraController.Instance.Width;
            var items = ItemController.Instance.GetActiveExperiences();
            foreach(var item in items)
            {
                if (item.IsBeingCollected) continue;
                var dir = Player.transform.position - item.transform.position;
                if (dir.magnitude > dist_max) continue;
                var t = dir.magnitude / dist_max;
                var t_speed = ac_charge_suck_falloff.Evaluate(1f - t);
                item.transform.position += dir.normalized * (speed_max * Time.deltaTime * t_speed);
            }
        }
    }

    private void StartShoot()
    {

    }

    private void Shoot(float distance)
    {
        Kills = 0;

        StartCoroutine(ShootCr());
        IEnumerator ShootCr()
        {
            for (int i = 0; i < beam_infos.Count; i++)
            {
                var info = beam_infos[i];
                var rotation = Player.Body.transform.rotation;
                var dir = rotation * info.localDirection;

                // Damage
                Physics2D.CircleCastAll(Player.transform.position, Width * 0.5f, dir, distance)
                    .Select(hit => hit.collider.GetComponentInParent<IKillable>())
                    .Distinct()
                    .Where(k => k != null && k.CanKill())
                    .ToList().ForEach(k =>
                    {
                        if (k.CanKill())
                        {
                            var position = k.GetPosition();
                            Player.KillEnemy(k);
                            Kills++;

                            if (ChainLightning)
                            {
                                StartCoroutine(ChainLightningCr(position));
                            }

                            if (HasModifier(Type.EXPLODE))
                            {
                                StartCoroutine(ExplodeCr(position));
                            }

                            if (EnemyFragments)
                            {
                                var fragments = AbilityMines.ShootFragments(position, template_fragment_projectile, 3, 20, 1.0f);
                                foreach(var fragment in fragments)
                                {
                                    fragment.Lifetime = 0.25f;
                                }
                            }
                        }
                    });

                // Enemy knockback
                var radius_knockback = Width * 3f;
                Physics2D.CircleCastAll(Player.transform.position + dir.normalized * radius_knockback, radius_knockback, dir, distance)
                    .Select(hit => hit.collider.GetComponentInParent<Enemy>())
                    .Where(e => e != null)
                    .ToList().ForEach(e =>
                    {
                        e.Knockback(Player.MoveDirection * Knockback, true, false);
                    });

                // Self knockback
                Player.Knockback(-dir.normalized * KnockbackSelf, true, true);

                // Visual
                info.graphic.AnimateFire();

                // Trail
                if (HasModifier(Type.DASH))
                {
                    var trail = Instantiate(template_trail, template_trail.transform.parent);
                    trail.gameObject.SetActive(true);
                    trail.transform.localPosition = Vector3.zero;
                    trail.ResetTrail();
                    trail.transform.position = Player.transform.position + dir * distance;
                    trail.UpdateTrail();
                }

                // Sound
                SoundController.Instance.Play(SoundEffectType.sfx_charge_shoot);

                // Delay
                if(info.delay > 0)
                {
                    yield return new WaitForSeconds(info.delay);
                }
            }

            StartCooldown();
        }

        IEnumerator ChainLightningCr(Vector3 position)
        {
            yield return new WaitForSeconds(0.25f);
            AbilityChain.TryChainToTarget(position, 6f, 1, 1, 0);
        }

        IEnumerator ExplodeCr(Vector3 position)
        {
            yield return new WaitForSeconds(0.25f);
            AbilityExplode.Explode(position, 2f, 50);
        }
    }

    public bool IsFullyCharged()
    {
        return GetCharge() >= 1f;
    }

    public float GetCharge()
    {
        var p = (Time.time - time_charge_start) / (time_charge_end - time_charge_start);
        var t = Mathf.Clamp(p, 0f, 1f);
        return t;
    }

    private float GetChargeTime()
    {
        if (IsActive)
        {
            var time_per_kill = Mathf.Clamp(ChargeTime * Kills * ChargeTimeOnKill, -ChargeTime, 0);
            return ChargeTime + time_per_kill;
        }
        else if(ModifierParent != null)
        {
            return ModifierParent.Info.type switch
            {
                Type.CHARGE => ((AbilityCharge)ModifierParent).GetChargeTime(),
                _ => 1f,
            };
        }

        return 0;
    }

    private void InitializeBeams()
    {
        // Clear beams
        beam_infos.ForEach(beam => Destroy(beam.graphic.gameObject));
        beam_infos.Clear();

        // Create beams
        var directions = AbilitySplit.GetSplitDirections(BeamCount, BeamArc, Vector3.up);
        for (int i = 0; i < directions.Count; i++)
        {
            var info = new BeamInfo();
            info.graphic = CreateBeamLocal();
            info.graphic.SetAlpha(0);
            info.localDirection = directions[i];
            info.delay = 0.1f;
            beam_infos.Add(info);
        }

        // Create back beam
        if (BeamBack)
        {
            var info = new BeamInfo();
            info.graphic = CreateBeamLocal();
            info.graphic.SetAlpha(0);
            info.localDirection = Vector3.down;
            info.delay = 0;
            beam_infos.Insert(0, info);
        }
    }

    private ChargeBeam CreateBeamLocal()
    {
        var beam = CreateBeam();
        beam.transform.parent = transform;
        beam.SetWidth(Width);
        beam.SetLength(DISTANCE_MAX);
        return beam;
    }

    public static ChargeBeam CreateBeam()
    {
        var template = Resources.Load<ChargeBeam>("Prefabs/Abilities/Objects/Beam");
        var beam = Instantiate(template);
        beam.gameObject.SetActive(true);
        return beam;
    }

    private void ShowBeamPreviews()
    {
        beam_infos.ForEach(beam => beam.graphic.AnimateShowPreview(true, ChargeTime));
    }

    private void HideBeamPreviews()
    {
        beam_infos.ForEach(beam => beam.graphic.AnimateShowPreview(false));
    }

    private void UpdateBeamPositions()
    {
        for (int i = 0; i < beam_infos.Count; i++)
        {
            var info = beam_infos[i];
            var beam = info.graphic;
            var rotation = Player.Body.transform.rotation;
            beam.SetPosition(Player.Body.transform.position);
            beam.SetDirection(rotation * info.localDirection);
        }
    }
}
