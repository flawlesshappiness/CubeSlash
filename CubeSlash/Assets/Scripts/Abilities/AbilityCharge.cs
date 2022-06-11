using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityCharge : Ability
{
    [SerializeField] private LineRenderer prefab_line;
    [SerializeField] private ParticleSystem ps_charge;
    [SerializeField] private ParticleSystem ps_charge_end;
    [SerializeField] private ParticleSystem ps_charged;
    [SerializeField] private ParticleSystem ps_beam_dust;
    [SerializeField] private AnimationCurve ac_charge_emission;

    private const float DISTANCE_MAX = 50f;

    private const int COUNT_EMISSION_PS_MIN = 10;
    private const int COUNT_EMISSION_PS_MAX = 50;

    private float time_charge_start;
    private float time_charge_end;
    public bool Charging { get; private set; }
    public bool ChargeEnded { get; private set; }

    private float Width { get; set; }
    private float ChargeTime { get; set; }
    private float Knockback { get; set; }

    private void Start()
    {
        prefab_line.gameObject.SetActive(false);
    }

    private void OnEnable()
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

    public override void InitializeValues()
    {
        base.InitializeValues();
        Width = 1;
        ChargeTime = 0.75f;
        Knockback = -10;
    }

    public override void InitializeModifier(Ability modifier)
    {
        Width = modifier.type switch
        {
            Type.DASH => Width + 2,
            Type.CHARGE => Width + 5,
            Type.SPLIT => Width * 0.5f,
        };

        ChargeTime = modifier.type switch
        {
            Type.DASH => ChargeTime + 0.25f,
            Type.CHARGE => ChargeTime + 0.5f,
            Type.SPLIT => ChargeTime + 0.5f,
        };

        Knockback = modifier.type switch
        {
            Type.DASH => Knockback - 75,
            Type.CHARGE => Knockback - 25,
            Type.SPLIT => Knockback - 10,
        };
    }

    public override void Pressed()
    {
        base.Pressed();
        time_charge_start = Time.time;
        time_charge_end = time_charge_start + ChargeTime;
        Charging = true;
        ChargeEnded = false;
        Player.Instance.AbilityLock.AddLock(nameof(AbilityCharge));
    }

    public override void Released()
    {
        base.Released();
        Charging = false;
        ChargeEnded = false;

        if (IsActive && IsFullyCharged())
        {
            Shoot(Player.MoveDirection, DISTANCE_MAX);
        }

        Player.Instance.AbilityLock.RemoveLock(nameof(AbilityCharge));
    }

    public override float GetCooldown()
    {
        return
            (HasModifier(Type.DASH) ? 0 : 0) +
            (HasModifier(Type.CHARGE) ? 5 : 0) +
            (HasModifier(Type.SPLIT) ? 3 : 0) +
            0.5f;
    }

    private void Update()
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
        }
    }

    private void Shoot(Vector3 dir, float distance)
    {
        var directions =
            HasModifier(Type.SPLIT) ? GetModifier<AbilitySplit>(Type.SPLIT).GetSplitDirections(3, 15, dir) :
            new List<Vector3> { dir };
        StartCoroutine(ShootCr(directions));

        IEnumerator ShootCr(List<Vector3> directions)
        {
            foreach(var dir in directions)
            {
                var hits = Physics2D.CircleCastAll(Player.transform.position, Width * 0.5f, dir, distance);
                foreach (var hit in hits)
                {
                    var e = hit.collider.GetComponentInParent<Enemy>();
                    if (e)
                    {
                        Player.Instance.KillEnemy(e);
                    }
                }

                StartCoroutine(KnockbackCr());
                StartVisual(Player.transform.position, Player.transform.position + dir * distance, 20);
                yield return new WaitForSeconds(0.1f);
            }
            StartCooldown();
        }

        IEnumerator KnockbackCr()
        {
            Player.Rigidbody.velocity = Player.MoveDirection * Knockback;
            Player.MovementLock.AddLock(nameof(AbilityCharge));
            Player.DragLock.AddLock(nameof(AbilityCharge));

            // Drag
            var time_end = Time.time + 0.2f;
            while(Time.time < time_end)
            {
                Player.Rigidbody.velocity *= 0.98f;
                yield return null;
            }

            Player.MovementLock.RemoveLock(nameof(AbilityCharge));
            Player.DragLock.RemoveLock(nameof(AbilityCharge));
        }
    }

    public bool IsFullyCharged()
    {
        return GetCharge() == 1f;
    }

    private float GetCharge()
    {
        var t = Mathf.Clamp((Time.time - time_charge_start) / (time_charge_end - time_charge_start), 0f, 1f);
        return t;
    }

    private void StartVisual(Vector3 start, Vector3 end, int segments)
    {
        StartCoroutine(BeamVisualCr(start, end, segments));

        // Particle System
        var dir = end - start;
        var angle = Vector3.SignedAngle(Vector3.up, dir, Vector3.back);
        ps_beam_dust.Duplicate()
            .Position(start)
            .Euler(angle-90, 90, -90)
            .Play()
            .Destroy(5);
    }

    private IEnumerator BeamVisualCr(Vector3 start, Vector3 end, int segments)
    {
        var line = Instantiate(prefab_line.gameObject, prefab_line.transform.parent).GetComponent<LineRenderer>();
        line.gameObject.SetActive(true);

        // Segments
        line.positionCount = segments + 1;
        line.SetPosition(0, start);
        for (int i = 0; i < segments; i++)
        {
            line.SetPosition(i, Vector3.Lerp(start, end, (float)i / segments));
        }
        line.SetPosition(segments, end);

        // Width
        yield return Lerp.Value(0.25f, "width_" + line.GetInstanceID(), f => line.widthMultiplier = (1f - f) * Width)
            .Connect(line.gameObject)
            .Curve(Lerp.Curve.EASE_END)
            .GetCoroutine();
        line.gameObject.SetActive(false);
        Destroy(line.gameObject);
    }
}
