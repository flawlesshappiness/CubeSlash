using System.Collections;
using UnityEngine;

public class AI_Beam : EntityAI
{
    [SerializeField] private float dist_max_mul_shoot;
    [SerializeField] private float cooldown_shoot;
    [SerializeField] private float beam_width;
    [SerializeField] private Color beam_color;
    [SerializeField] private ChargeBeam template_beam;

    private Vector3 pos_player_prev;
    private float cd_shoot;

    private ChargeBeam beam;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);
        beam = Instantiate(template_beam, transform);
        beam.transform.localPosition = Vector3.zero;
        beam.SetColor(beam_color);
        beam.SetWidth(beam_width);
        beam.SetAlpha(0);
    }

    private void FixedUpdate()
    {
        pos_player_prev = IsPlayerAlive() ? Player.Instance.transform.position : pos_player_prev;
        MoveUpdate();
        BeamUpdate();
        ShootUpdate();
    }

    private void BeamUpdate()
    {
        if (beam == null) return;
        var dir_to_player = DirectionToPlayer();
        var screen_width = CameraController.Instance.Width;
        beam.SetLength(dir_to_player.magnitude + screen_width);
        beam.SetDirection(Self.Body.transform.up);
    }

    private void MoveUpdate()
    {
        var dist = DistanceToPlayer();
        var dist_max_shoot = CameraController.Instance.Width * dist_max_mul_shoot;
        if (dist < dist_max_shoot)
        {
            MoveToStop(0.5f);
            TurnTowards(pos_player_prev);
        }
        else
        {
            MoveTowards(pos_player_prev);
            TurnTowards(pos_player_prev);
        }
    }

    private void ShootUpdate()
    {
        var dist = DistanceToPlayer();
        var dist_max = CameraController.Instance.Width * dist_max_mul_shoot;
        if (Time.time < cd_shoot) return;
        if (dist < dist_max)
        {
            StartCoroutine(Cr());
        }

        IEnumerator Cr()
        {
            cd_shoot = Time.time + 999;

            var duration = 2.0f;
            var time_start = Time.time;
            var time_end = time_start + duration;
            beam.AnimateShowPreview(true, duration);
            while(Time.time < time_end)
            {
                var t = (Time.time - time_start) / duration;
                var curve = EasingCurves.EaseOutQuad;
                Self.AngularVelocity = Mathf.Lerp(Self.Settings.angular_velocity, Self.Settings.angular_velocity * 0.25f, curve.Evaluate(t));
                Self.AngularAcceleration = Mathf.Lerp(Self.Settings.angular_acceleration, Self.Settings.angular_acceleration * 0.25f, curve.Evaluate(t));
                yield return null;
            }

            beam.AnimateFire();
            Shoot();
            cd_shoot = Time.time + cooldown_shoot;
            Self.Knockback(-Self.Body.transform.up * 600f, true, true);
            Self.AngularVelocity = Self.Settings.angular_velocity;
            Self.AngularAcceleration = Self.Settings.angular_acceleration;
        }
    }

    private void Shoot()
    {
        var hits = Physics2D.CircleCastAll(transform.position, beam_width * 0.5f, Self.Body.transform.up);
        foreach(var hit in hits)
        {
            var player = hit.collider.GetComponentInParent<Player>();
            if (player == null) continue;
            player.Damage(transform.position);
        }
    }
}