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

    private class Tether
    {
        public ParticleSystem ps;
        public HealthDud dud;
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
            Lerp.LocalScale(Self.Body.pivot_sprite, 0.5f, new Vector3(0.8f, 1.2f, 1f))
            .Curve(EasingCurves.EaseOutQuad);

            moving = true;
            var time_move = Time.time + 0.5f;
            while (Time.time < time_move)
            {
                Self.Move(Self.MoveDirection);
                yield return new WaitForFixedUpdate();
            }
            moving = false;

            Lerp.LocalScale(Self.Body.pivot_sprite, 1f, Vector3.one)
                .Curve(EasingCurves.EaseInOutQuad);

            var time_wait = Time.time + 1f;
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
        var count = Mathf.Lerp(4, 8, diff);
        for (int i = 0; i < count; i++)
        {
            CreateTether();
        }
    }

    private void CreateTether()
    {
        var tether = new Tether();
        tether.ps = Instantiate(ps_tether);
        tether.ps.Play();
        ObjectController.Instance.Add(tether.ps.gameObject);

        tether.dud = Instantiate(prefab_dud);
        tether.dud.Initialize();
        tether.dud.transform.position = Player.Instance.transform.position + Random.insideUnitCircle.normalized.ToVector3() * Random.Range(40f, 50f);
        tether.dud.transform.localScale = Vector3.one * 2f;
        tether.dud.OnKilled += () => OnDudKilled(tether);
        ObjectController.Instance.Add(tether.dud.gameObject);

        tethers.Add(tether);
        UpdateTether(tether);
    }

    private void UpdateTethers()
    {
        tethers.ToList().ForEach(t => UpdateTether(t));
    }

    private void UpdateTether(Tether tether)
    {
        var dir = Self.transform.position - tether.dud.transform.position;
        var distance = dir.magnitude;
        var angle = Vector3.SignedAngle(Vector3.up, dir, Vector3.forward);
        var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        var position = Vector3.Lerp(Self.transform.position, tether.dud.transform.position, 0.5f);

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

    private void OnDudKilled(Tether tether)
    {
        tether.ps.SetEmissionEnabled(false);
        Destroy(tether.ps.gameObject, 5);
        Destroy(tether.dud.gameObject);
        tethers.Remove(tether);

        if (tethers.Count == 0)
        {
            Self.Kill();
        }
    }
}