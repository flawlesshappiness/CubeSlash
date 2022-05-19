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

    private const float TIME_CHARGE_MAX = 1f;
    private const float DISTANCE_MAX = 20f;
    private const float ANGLE_MIN = 10;
    private const float ANGLE_MAX = 25;
    private const float WIDTH_MAX = 2f;
    private const int DAMAGE_MAX = 3;

    private const int COUNT_EMISSION_PS_MIN = 10;
    private const int COUNT_EMISSION_PS_MAX = 50;

    private float time_charge_start;
    private float time_charge_end;
    public bool Charging { get; private set; }
    public bool ChargeEnded { get; private set; }

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

    public override void Pressed()
    {
        base.Pressed();
        time_charge_start = Time.time;
        time_charge_end = time_charge_start + TIME_CHARGE_MAX;
        Charging = true;
        ChargeEnded = false;
        Player.Instance.AbilityLock.AddLock(nameof(AbilityCharge));
    }

    public override void Released()
    {
        base.Released();
        Charging = false;
        ChargeEnded = false;

        if (IsActive)
        {
            if (HasModifier(Type.SPLIT))
            {
                var t = GetCharge();
                var count = t == 1 ? 15 : (int)Mathf.Clamp(Mathf.Lerp(3, 8, t), 3, 7);
                var angle = t == 1 ? 170 : Mathf.Lerp(40, 30, t);
                ShootTargets(Player.transform.position, Player.MoveDirection, count, angle, true);
                Player.Instance.MovementLock.AddLock(nameof(AbilityCharge));
            }
            else
            {
                TryShoot(Player.transform.position, Player.MoveDirection, ANGLE_MAX, true);
            }
        }
        else
        {
            Player.Instance.AbilityLock.RemoveLock(nameof(AbilityCharge));
        }
    }

    public override float GetCooldown()
    {
        var t = GetCharge();
        return
            (HasModifier(Type.DASH) ? (t == 1 ? 5 : 2) : 0) +
            (HasModifier(Type.SPLIT) ? (t == 1 ? 5 : 1) : 0) +
            0;
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

    private void TryShoot(Vector3 start, Vector3 dir, float angle_max, bool first_beam)
    {
        var target = GetTarget(start, dir.normalized, DISTANCE_MAX, angle_max);
        if (target)
        {
            Shoot(start, target, first_beam);

            if (HasModifier(Type.DASH)) // Penetrate
            {
                // Check if some enemies are way too close
                var hits = Physics2D.OverlapCircleAll(target.transform.position, 1f);
                foreach(var hit in hits)
                {
                    var e = hit.GetComponentInParent<Enemy>();
                    if (e)
                    {
                        DamageEnemy(e);
                    }
                }

                // Penetration shot
                TryShoot(target.transform.position, dir, ANGLE_MIN, false);
            }
        }
        else
        {
            Shoot(start, start + dir.normalized * DISTANCE_MAX, first_beam);
        }

        Player.Instance.AbilityLock.RemoveLock(nameof(AbilityCharge));
        StartCooldown();
    }

    private void Shoot(Vector3 start, Enemy target, bool first_beam)
    {
        DamageEnemy(target);
        Shoot(start, target.transform.position, first_beam);
    }

    private void Shoot(Vector3 start, Vector3 end, bool first_beam)
    {
        var width = WIDTH_MAX * GetCharge();
        StartVisual(start, end, first_beam ? 0.1f : width, width);
    }

    private void ShootTargets(Vector3 start, Vector3 dir, int count, float angle_max, bool first_beam)
    {
        var split = GetModifier<AbilitySplit>(Type.SPLIT);
        if (split)
        {
            var directions = split.GetSplitDirections(count, angle_max, dir);
            StartCoroutine(ShootTargetsCr(start, directions, first_beam));
        }
    }

    private void DamageEnemy(Enemy e)
    {
        var t = GetCharge();
        var damage = (int)Mathf.Clamp((DAMAGE_MAX * t) + 1, 1, DAMAGE_MAX);
        Player.Instance.DamageEnemy(e, damage);
    }

    private IEnumerator ShootTargetsCr(Vector3 start, List<Vector3> directions, bool first_beam)
    {
        for (int i = 0; i < directions.Count; i++)
        {
            var dir = directions[i];
            TryShoot(start, dir, ANGLE_MIN, false);

            for (int i_frames = 0; i_frames < 10; i_frames++)
            {
                yield return null;
            }
        }

        Player.Instance.AbilityLock.RemoveLock(nameof(AbilityCharge));
        Player.Instance.MovementLock.RemoveLock(nameof(AbilityCharge));
        StartCooldown();
    }

    public float GetCharge()
    {
        var t = Mathf.Clamp((Time.time - time_charge_start) / (time_charge_end - time_charge_start), 0f, 1f);
        return t;
    }

    private void StartVisual(Vector3 start, Vector3 end, float w_start, float w_end)
    {
        CoroutineController.Instance.Run(BeamVisualCr(start, end, w_start, w_end), "beam_visual_"+GetInstanceID());

        // Particle System
        var dir = end - start;
        var angle = Vector3.SignedAngle(Vector3.up, dir, Vector3.back);
        ps_beam_dust.Duplicate()
            .Position(start)
            .Euler(angle-90, 90, -90)
            .Play()
            .Destroy(5);
    }

    private IEnumerator BeamVisualCr(Vector3 start, Vector3 end, float w_start, float w_end)
    {
        prefab_line.gameObject.SetActive(true);
        var line = Instantiate(prefab_line.gameObject, prefab_line.transform.parent).GetComponent<LineRenderer>();
        prefab_line.gameObject.SetActive(false);
        line.SetPosition(0, start);
        line.SetPosition(1, end);
        yield return Lerp.Value(0.25f, 0f, 1f, f =>
        {
            line.startWidth = Mathf.Lerp(w_start, 0f, f);
            line.endWidth = Mathf.Lerp(w_end, 0f, f);
        }, line.gameObject, "width_" + line.GetInstanceID())
            .GetEnumerator();
        line.gameObject.SetActive(false);
        Destroy(line.gameObject);
    }

    private class FindTargetMap
    {
        public Enemy Enemy { get; set; }
        public float Value { get; set; }
    }

    private Enemy GetTarget(Vector3 start, Vector3 forward, float distance_max, float angle_max)
    {
        var targets = new List<FindTargetMap>();
        var hits = Physics2D.OverlapCircleAll(start, distance_max);
        foreach(var hit in hits)
        {
            var e = hit.gameObject.GetComponentInParent<Enemy>();
            if (e == null) continue;

            var dir = e.transform.position - start;
            var angle = Vector3.Angle(forward.normalized, dir.normalized);
            var v_angle = 1f - (angle / 180f);
            if (angle > angle_max) continue;

            var dist = dir.magnitude;
            var v_dist = (1f - (dist / distance_max)) * 1.5f;

            var target = new FindTargetMap();
            targets.Add(target);
            target.Enemy = e;
            target.Value = v_angle + v_dist;
        }

        targets = targets.OrderByDescending(target => target.Value).ToList();
        return targets.Count == 0 ? null : targets[0].Enemy;
    }
}
