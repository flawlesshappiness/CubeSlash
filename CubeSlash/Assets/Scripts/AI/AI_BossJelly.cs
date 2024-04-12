using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AI_BossJelly : BossAI
{
    [SerializeField] private ParticleSystem ps_tether;
    [SerializeField] private HealthDud prefab_dud;

    private Vector3 target_position;
    private bool moving;

    private List<Tether> tethers = new List<Tether>();

    private const int COUNT_TETHER_MIN_DIFF = 4;
    private const int COUNT_TETHER_MAX_DIFF = 12;
    private int max_tethers;
    private int remaining_tethers;

    private float mul_move_min = 0.5f;
    private float mul_move_max = 1.0f;

    private float T_Health => remaining_tethers / (float)max_tethers;
    private float MulMove => Mathf.Lerp(mul_move_min, mul_move_max, T_Health);
    private BossJellyBody JellyBody => Body as BossJellyBody;

    private class Tether
    {
        public ParticleSystem ps;
        public IKillable target;
    }

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);
        Self.CanReposition = false;

        CreateTethers();
        StartCoroutine(MoveCr());
    }

    private void Update()
    {
        target_position = Player.Instance.transform.position;

        UpdateTethers();
    }

    private void FixedUpdate()
    {
        if (!moving)
        {
            Self.Rigidbody.velocity *= 0.98f;
        }
    }

    private IEnumerator MoveCr()
    {
        while (true)
        {
            Lerp.LocalScale(Self.Body.pivot_sprite, 0.5f * MulMove, new Vector3(0.8f, 1.2f, 1f))
                .Curve(EasingCurves.EaseOutQuad);

            PushAwayEnemies();

            moving = true;
            var time_move = Time.time + 0.5f;
            while (Time.time < time_move)
            {
                MoveTowards(PlayerPosition);
                yield return new WaitForFixedUpdate();
            }
            moving = false;

            Lerp.LocalScale(Self.Body.pivot_sprite, 1f * MulMove, Vector3.one)
                .Curve(EasingCurves.EaseInOutQuad);

            var time_wait = Time.time + 1f * MulMove;
            while (Time.time < time_wait)
            {
                TurnTowards(target_position);
                yield return new WaitForFixedUpdate();
            }
        }
    }

    private void CreateTethers()
    {
        var diff = DifficultyController.Instance.DifficultyValue;
        var count_tether_diff = (int)Mathf.Lerp(COUNT_TETHER_MIN_DIFF, COUNT_TETHER_MAX_DIFF, diff);
        var count_tether_area = AreaController.Instance.CurrentAreaIndex;
        max_tethers = count_tether_diff + count_tether_area;
        remaining_tethers = max_tethers;

        if (DifficultyController.Instance.DifficultyIndex == 0)
        {
            for (int i = 0; i < max_tethers; i++)
            {
                CreateTether();
            }
        }
        else
        {
            for (int i = 0; i < Mathf.Min(max_tethers, 2); i++)
            {
                CreateTether();
            }
        }
    }

    private void CreateTether()
    {
        // Tether
        var tether = new Tether();
        tether.ps = Instantiate(ps_tether);
        tether.ps.Play();
        ObjectController.Instance.Add(tether.ps.gameObject);

        // Enemy
        var enemy = EnemyController.Instance.SpawnEnemy(EnemyType.JellyShield, CameraController.Instance.GetPositionOutsideCamera());
        enemy.OnDeath += () => OnTetherKilled(tether);
        tether.target = enemy;

        tethers.Add(tether);
        UpdateTether(tether);

        // Update body
        remaining_tethers--;
        var t = remaining_tethers / (float)max_tethers;
        JellyBody.T_Health = t;
    }

    private void UpdateTethers()
    {
        tethers.ToList().ForEach(t => UpdateTether(t));
    }

    private void UpdateTether(Tether tether)
    {
        var dir = Self.transform.position - tether.target.GetPosition();
        var distance = dir.magnitude;
        var angle = Vector3.SignedAngle(Vector3.up, dir, Vector3.forward);
        var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        var position = Vector3.Lerp(Self.transform.position, tether.target.GetPosition(), 0.5f);

        tether.ps.transform.SetPositionAndRotation(position, rotation);

        tether.ps.ModifyShape(shape =>
        {
            shape.scale = new Vector3(0.5f, 1f, distance - 2);
        });

        tether.ps.ModifyEmission(e =>
        {
            e.rateOverTime = new ParticleSystem.MinMaxCurve { constant = distance * 5 };
        });
    }

    private void OnTetherKilled(Tether tether)
    {
        tether.ps.SetEmissionEnabled(false);
        Destroy(tether.ps.gameObject, 5);
        tethers.Remove(tether);

        if (remaining_tethers > 0 && DifficultyController.Instance.DifficultyIndex > 0)
        {
            CreateTether();
        }
        else if (tethers.Count == 0)
        {
            Self.Kill();
        }
    }

    private void PushAwayEnemies()
    {
        var enemies = EnemyController.Instance.ActiveEnemies
            .Where(e => e != Self && Vector3.Distance(e.transform.position, transform.position) < 20)
            .ToList();

        var forward = Self.transform.up;
        var right = Self.transform.right;

        foreach (var e in enemies)
        {
            var dir = e.transform.position - transform.position;
            var dist = dir.magnitude;
            var dot_forward = Vector3.Dot(forward, dir);
            var dot_right = Vector3.Dot(right, dir);
            var force = (1f - Mathf.Min(dist / 20, 1)) * 500;

            if (dot_forward > 0)
            {
                var sign = Mathf.Sign(dot_right);
                var velocity = right.normalized * force * sign;
                e.Knockback(velocity, true, false);
            }
        }
    }
}