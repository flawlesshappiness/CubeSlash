using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityCharge : Ability
{
    [SerializeField] private LineRenderer line;

    private const float TIME_CHARGE_MAX = 1f;
    private const float DISTANCE_MAX = 20f;
    private const float ANGLE_MAX = 25f;
    private const float WIDTH_MAX = 2f;
    private const int DAMAGE_MAX = 3;

    private float time_charge_start;
    private float time_charge_end;
    private bool charging;

    private void Start()
    {
        line.gameObject.SetActive(false);
    }

    public override void Pressed()
    {
        base.Pressed();
        time_charge_start = Time.time;
        time_charge_end = time_charge_start + TIME_CHARGE_MAX;
        charging = true;
    }

    public override void Released()
    {
        base.Released();
        charging = false;
        Shoot();
    }

    private void Shoot()
    {
        var t = Mathf.Clamp((Time.time - time_charge_start) / (time_charge_end - time_charge_start), 0f, 1f);
        var damage = (int)Mathf.Clamp((DAMAGE_MAX * t) + 1, 1, TIME_CHARGE_MAX);
        var target = GetTarget(DISTANCE_MAX, ANGLE_MAX);
        if(target != null)
        {
            var width = WIDTH_MAX * t;
            Player.Instance.DamageEnemy(target, damage);
            StartVisual(target.transform.position, width);
        }
    }

    private void StartVisual(Vector3 position, float width)
    {
        StopVisual();
        _cr_visual = StartCoroutine(BeamVisualCr(position, width));
    }

    private void StopVisual()
    {
        if(_cr_visual != null)
        {
            StopCoroutine(_cr_visual);
            _cr_visual = null;
        }
    }

    private Coroutine _cr_visual;
    private IEnumerator BeamVisualCr(Vector3 position, float width)
    {
        line.gameObject.SetActive(true);
        line.SetPosition(0, Player.transform.position);
        line.SetPosition(1, position);
        yield return Lerp.Value(0.25f, 0f, 1f, f =>
        {
            line.startWidth = Mathf.Lerp(0.1f, 0f, f);
            line.endWidth = Mathf.Lerp(width, 0f, f);
        }, line.gameObject, "width_" + line.GetInstanceID())
            .GetEnumerator();
        line.gameObject.SetActive(false);
    }

    private class FindTargetMap
    {
        public Enemy Enemy { get; set; }
        public float Value { get; set; }
    }

    private Enemy GetTarget(float distance_max, float angle_max)
    {
        var targets = new List<FindTargetMap>();
        var hits = Physics2D.OverlapCircleAll(Player.transform.position, distance_max);
        foreach(var hit in hits)
        {
            var e = hit.gameObject.GetComponentInParent<Enemy>();
            if (e == null) continue;

            var dir = e.transform.position - Player.transform.position;
            var angle = Vector3.Angle(Player.MoveDirection.normalized, dir.normalized);
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
