using System.Collections;
using System.Linq;
using UnityEngine;

public class AI_BossHost : EntityAI
{
    [SerializeField] private Color color_beam;
    [SerializeField] private Vector2 speed_rotate_min_max;
    [SerializeField] private FMODEventReference sfx_charge, sfx_shoot;

    private Vector3 destination;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);
        this.StartCoroutineWithID(BeamCooldownCr(), "BeamCooldown_" + GetInstanceID());
        Self.EnemyBody.OnDudKilled += dud => HideDuds(4);
        Self.OnDeath += OnDeath;
    }

    private void FixedUpdate()
    {
        if(IsPlayerAlive())
        {
            destination = PlayerPosition;
        }

        var dir = (destination - Position).normalized;
        var offset = dir * Self.Settings.size * 0.7f;
        var t = (Vector3.Distance(Position + offset, destination)) / (CameraController.Instance.Width * 0.5f);
        Self.AngularAcceleration = Mathf.Lerp(Self.Settings.angular_acceleration * 0.25f, Self.Settings.angular_acceleration, t);
        Self.AngularVelocity = Mathf.Lerp(Self.Settings.angular_velocity * 0.25f, Self.Settings.angular_velocity, t);
        MoveTowards(PlayerPosition);
    }

    private void LateUpdate()
    {
        RotateBodyUpdate();
    }

    private void OnDeath()
    {
        sfx_charge.Stop();
    }

    private void RotateBodyUpdate()
    {
        var duds = Self.EnemyBody.Duds;
        var duds_dead = duds.Where(d => d.Dead);
        var t_duds = (float)duds_dead.Count() / duds.Count();
        var speed = Mathf.Lerp(speed_rotate_min_max.x, speed_rotate_min_max.y, t_duds);
        var body = Self.Body;
        var angle = body.transform.eulerAngles.z + speed;
        var angle_mod = angle % 360f;
        var q = Quaternion.AngleAxis(angle_mod, Vector3.forward);
        body.transform.rotation = Quaternion.Lerp(body.transform.rotation, q, Time.deltaTime * 10f);

    }

    IEnumerator BeamCooldownCr()
    {
        while (true)
        {
            ShootBeams();
            yield return new WaitForSeconds(5f);
        }
    }

    private void ShootBeams()
    {
        foreach(var dud in Self.EnemyBody.Duds)
        {
            if (!dud.Dead) continue;
            dud.StartCoroutineWithID(ShootBeam(dud), "ShootBeam_" + dud.GetInstanceID());
        }

        IEnumerator ShootBeam(HealthDud dud)
        {
            var dir_to_player = DirectionToPlayer();
            var dir = dud.transform.up;
            var beam = AbilityCharge.CreateBeam();
            var width = 5f;
            var length = dir_to_player.magnitude + CameraController.Instance.Width;

            beam.transform.parent = dud.transform;
            beam.transform.localPosition = Vector3.zero - Vector3.up * width * 0.5f;
            beam.transform.localRotation = Quaternion.identity;
            beam.SetColor(color_beam);
            beam.SetWidth(width);
            beam.SetLength(length);
            beam.UpdateVisual();
            beam.AnimateShowPreview(true);

            sfx_charge.PlayWithTimeLimit(0.2f);

            yield return new WaitForSeconds(2f);

            sfx_charge.Stop();
            sfx_shoot.Play();

            dir = dud.transform.up;
            Physics2D.CircleCastAll(dud.transform.position, width * 0.25f, dir, length)
                    .Select(hit => hit.collider.GetComponentInParent<Player>())
                    .Distinct()
                    .Where(p => p != null && !p.IsDead)
                    .ToList().ForEach(p =>
                    {
                        p.Damage(dud.transform.position);
                    });

            Self.Knockback(-dir * 200f, true, false);

            yield return beam.AnimateFire();

            Destroy(beam.gameObject);
        }
    }
}