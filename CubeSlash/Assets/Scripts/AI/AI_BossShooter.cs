using System.Collections;
using System.Linq;
using UnityEngine;

public class AI_BossShooter : EntityAI
{
    [SerializeField] private Projectile prefab_projectile;

    private enum MoveState { WATCH, MOVE_TO_PLAYER }
    private MoveState state = MoveState.WATCH;

    private Vector3 destination;

    private bool ignore_state;
    private bool attacking;
    private float time_attack;

    private Transform t_eye;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);
        ResetSpeed();

        t_eye = Self.Body.GetTransform("eye");

        Self.Body.OnDudKilled += dud => ShieldDuds();
    }

    private void ResetSpeed()
    {
        ignore_state = false;
        LerpLinearVelocity(0.25f, Self.Settings.linear_velocity);
        LerpLinearAcceleration(0.25f, Self.Settings.linear_acceleration);
        LerpAngularVelocity(0.25f, Self.Settings.angular_velocity);
        LerpAngularAcceleration(0.25f, Self.Settings.angular_acceleration);
    }

    private void FixedUpdate()
    {
        if (ignore_state)
        {
            return;
        }

        if(state == MoveState.WATCH)
        {
            if(Vector3.Distance(Position, PlayerPosition) > CameraController.Instance.Width * 0.45f)
            {
                state = MoveState.MOVE_TO_PLAYER;
            }
            else
            {
                MoveToStop(1f);
                TurnTowards(PlayerPosition);

                if (!attacking && Time.time > time_attack)
                {
                    if(AngleTowards(PlayerPosition).Abs() > 60)
                    {
                        StartCoroutine(AttackSpinShoot());
                    }
                    else
                    {
                        StartCoroutine(AttackShootFreq());
                    }
                }
            }
        }
        else if(state == MoveState.MOVE_TO_PLAYER)
        {
            if (Vector3.Distance(Position, PlayerPosition) > CameraController.Instance.Width * 0.25f)
            {
                destination = IsPlayerAlive() ? PlayerPosition : Position;
                MoveTowards(destination);
                TurnTowards(destination);
            }
            else
            {
                state = MoveState.WATCH;
            }
        }
    }

    private IEnumerator AttackShootFreq()
    {
        attacking = true;
        ignore_state = true;

        // Telegraph
        TelegraphShootLong(0);

        var timestampA = Time.time;
        while(Time.time < timestampA + 1.5f)
        {
            Self.AngularVelocity = 200;
            TurnTowards(PlayerPosition, 0);
            yield return null;
        }

        var time_start = Time.time;
        var time_end = Time.time + 5f;

        // Shoot
        while(Time.time < time_end)
        {
            var t = Mathf.Clamp01((Time.time - time_start) / (time_end - time_start));
            Shoot(Random.Range(-15, 15));
            Self.LinearVelocity = 7;
            Self.AngularVelocity = Mathf.Lerp(50, 200, t);
            TurnTowards(PlayerPosition, 0);
            yield return new WaitForSeconds(0.1f + 0.7f * t);
        }

        ResetSpeed();
        time_attack = Time.time + Random.Range(5f, 7f);
        attacking = false;
    }

    private IEnumerator AttackSpinShoot()
    {
        attacking = true;
        ignore_state = true;
        var angle_player = AngleTowards(PlayerPosition);
        var sign = Mathf.Sign(angle_player);
        var dir = -DirectionToPlayer();

        var linearMax = 6;
        var angularMax = 200;
        var count_shots = 5;
        var arc = 45;

        Telegraph(count_shots, arc);

        while (AngleTowards(PlayerPosition).Abs() < 175f)
        {
            Self.LinearVelocity = linearMax;
            Self.AngularVelocity = angularMax;
            Self.Turn(sign < 0);
            Self.Move(dir);
            yield return null;
        }

        Telegraph(count_shots, arc);

        while (AngleTowards(PlayerPosition).Abs() > 5)
        {
            var angle = AngleTowards(PlayerPosition);
            var t_angle = angle.Abs() / 30;
            Self.LinearVelocity = Mathf.Lerp(0, linearMax, t_angle);
            Self.AngularVelocity = Mathf.Lerp(25, angularMax, t_angle);
            Self.Turn(sign < 0);
            Self.Move(dir);
            yield return null;
        }

        var arc_per = arc / count_shots;
        for (int i = -count_shots; i < count_shots + 1; i++)
        {
            Shoot(i * arc_per);
        }

        ResetSpeed();
        time_attack = Time.time + Random.Range(5f, 7f);
        attacking = false;

        void Telegraph(int count, float arc)
        {
            var arc_per = arc / count;
            for (int i = -count; i < count + 1; i++)
            {
                TelegraphShootShort(i * arc_per);
            }
        }
    }

    private void ShieldDuds()
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            Self.Body.Duds.Where(d => d.IsActive()).ToList().ForEach(d => d.SetArmorActive(true));
            yield return new WaitForSeconds(5f);
            Self.Body.Duds.Where(d => d.IsActive()).ToList().ForEach(d => d.SetArmorActive(false));
        }
    }

    private void Shoot(float angle)
    {
        var velocity_projectile = 10;
        var dir = Quaternion.AngleAxis(angle, Vector3.forward) * transform.up;
        var p = Instantiate(prefab_projectile.gameObject).GetComponent<Projectile>();
        p.transform.position = t_eye.position;
        p.Rigidbody.velocity = dir.normalized * velocity_projectile;
        p.SetDirection(dir);
        p.Lifetime = 999f;
        Self.Rigidbody.AddForce(-dir * 50 * Self.Rigidbody.mass);
    }

    private void TelegraphShootLong(float angle)
    {
        Resources.Load<ParticleSystem>("particles/ps_telegraph_shoot_long")
            .Duplicate()
            .Parent(transform)
            .Scale(Vector3.one)
            .Position(t_eye.position)
            .Rotation(transform.rotation * Quaternion.AngleAxis(angle, Vector3.forward))
            .Destroy(3f);
    }

    private void TelegraphShootShort(float angle)
    {
        Resources.Load<ParticleSystem>("particles/ps_telegraph_shoot_short")
            .Duplicate()
            .Parent(transform)
            .Scale(Vector3.one)
            .Position(t_eye.position)
            .Rotation(transform.rotation * Quaternion.AngleAxis(angle, Vector3.forward))
            .Destroy(3f);
    }
}