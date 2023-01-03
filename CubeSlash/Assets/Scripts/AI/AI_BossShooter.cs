using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class AI_BossShooter : EnemyAI
{
    [SerializeField] private Projectile prefab_projectile;
    [SerializeField] private FMODEventReference sfx_shoot;

    private enum MoveState { WATCH, MOVE_TO_PLAYER }
    private MoveState state = MoveState.WATCH;

    private Vector3 destination;

    private bool ignore_state;
    private bool attacking;
    private float time_attack;
    private float time_circle_attack;

    private BossShooterBody body_shooter;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);
        ResetSpeed();

        body_shooter = enemy.Body.GetComponent<BossShooterBody>();

        Self.EnemyBody.OnDudKilled += dud => HideAndShowDuds(5);
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
                    var close_to_player = DistanceToPlayer() < 20f;
                    if (close_to_player)
                    {
                        SelectAttack();
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

    private void SelectAttack()
    {
        if(Time.time > time_circle_attack + 20)
        {
            this.StartCoroutineWithID(AttackShootCircle(), "attack_" + GetInstanceID());
            time_circle_attack = Time.time;
        }
        else if (AngleTowards(PlayerPosition).Abs() > 60)
        {
            this.StartCoroutineWithID(AttackSpinShoot(), "attack_"+GetInstanceID());
        }
        else
        {
            var rnd = Random.Range(0f, 1f);
            if (rnd < 0.2f)
            {
                this.StartCoroutineWithID(AttackSpinShoot(), "attack_" + GetInstanceID());
            }
            else
            {
                this.StartCoroutineWithID(AttackShootArc(), "attack_" + GetInstanceID());
            }
        }
    }

    private IEnumerator AttackShootArc()
    {
        attacking = true;
        ignore_state = true;

        var count_shots = 5;
        var arc = 45;
        var radius = 5f;

        body_shooter.SetEyesArc();

        yield return new WaitForSeconds(1.0f);

        TelegraphShortMany(count_shots, arc);

        yield return new WaitForSeconds(1f);

        ShootMany(count_shots, arc, radius);
        body_shooter.SetEyesSingle();
        Self.Knockback(-transform.up * 300, true, true);

        time_attack = Time.time + Random.Range(4f, 5f);
        attacking = false;
        ignore_state = false;
    }

    private IEnumerator AttackSpinShoot()
    {
        attacking = true;
        ignore_state = true;
        var angle_player = AngleTowards(PlayerPosition);
        var sign = Mathf.Sign(angle_player);
        var dir = -DirectionToPlayer();

        var linearMax = 6;
        var angular_max = 200;
        var count_shots = 8;
        var arc = 60;
        var radius = 5f;

        body_shooter.SetEyesArc();

        yield return new WaitForSeconds(1f);

        TelegraphShortMany(count_shots, arc);

        while (AngleTowards(PlayerPosition).Abs() < 175f)
        {
            Self.LinearVelocity = linearMax;
            Self.AngularVelocity = angular_max;
            Self.Turn(sign < 0);
            Self.Move(dir);
            yield return null;
        }

        TelegraphShortMany(count_shots, arc);

        while (AngleTowards(PlayerPosition).Abs() > 5)
        {
            var angle = AngleTowards(PlayerPosition);
            var t_angle = angle.Abs() / 30;
            Self.LinearVelocity = Mathf.Lerp(0, linearMax, t_angle);
            Self.AngularVelocity = Mathf.Lerp(25, angular_max, t_angle);
            Self.Turn(sign < 0);
            Self.Move(dir);
            yield return null;
        }

        ShootMany(count_shots, arc, radius);
        body_shooter.SetEyesSingle();
        Self.Knockback(-transform.up * 300, true, true);

        ResetSpeed();
        time_attack = Time.time + Random.Range(5f, 7f);
        attacking = false;
    }

    IEnumerator AttackShootCircle()
    {
        attacking = true;
        ignore_state = true;

        body_shooter.SetEyesCircle();

        var duration = 7f;
        Self.AngularVelocity = 25;
        Self.AngularAcceleration = 10;
        this.StartCoroutineWithID(TurnCr(duration), "turn_" + GetInstanceID());
        HideAndShowDuds(duration);

        yield return new WaitForSeconds(1.0f);

        TelegraphCircle();

        yield return new WaitForSeconds(1.0f);

        for (int i = 0; i < 5; i++)
        {
            ShootCircle();
            yield return new WaitForSeconds(1.0f);
        }

        body_shooter.SetEyesSingle();

        ResetSpeed();

        time_attack = Time.time + Random.Range(4f, 5f);
        attacking = false;
        ignore_state = false;

        IEnumerator TurnCr(float duration)
        {
            var right = Random.Range(0, 2) == 0;
            var time_end = Time.time + duration;
            while(Time.time < time_end)
            {
                Self.Turn(right);
                yield return null;
            }
        }
    }

    private void Shoot(Vector3 position, Vector3 direction)
    {
        var velocity_projectile = 10;
        var p = Instantiate(prefab_projectile.gameObject).GetComponent<Projectile>();
        p.transform.position = position;
        p.Rigidbody.velocity = direction.normalized * velocity_projectile;
        p.SetDirection(direction);
        p.Lifetime = 999f;
        //Self.Rigidbody.AddForce(-direction * 50 * Self.Rigidbody.mass);
    }

    private void Shoot(float angle, float radius)
    {
        var dir = Quaternion.AngleAxis(angle, Vector3.forward) * transform.up;
        var pos_front = body_shooter.GetFrontPosition();
        var position = pos_front - (transform.up * radius) + (dir * radius);
        Shoot(position, dir);
    }

    private void ShootMany(int count_shots, float arc, float radius)
    {
        var arc_min = -arc * 0.5f;
        var arc_max = arc * 0.5f;
        for (int i = 0; i < count_shots; i++)
        {
            var t = (float)i / (count_shots - 1);
            var angle = Mathf.Lerp(arc_min, arc_max, t);
            Shoot(angle, radius);
        }

        sfx_shoot.PlayWithPitch(-2);
    }

    private void ShootCircle()
    {
        var ts = body_shooter.GetCircleTransforms();
        foreach(var t in ts)
        {
            Shoot(t.position, t.up);
        }

        sfx_shoot.PlayWithPitch(-2);
    }

    private void TelegraphShortMany(int count, float arc)
    {
        var arc_min = -arc * 0.5f;
        var arc_max = arc * 0.5f;
        for (int i = 0; i < count; i++)
        {
            var t = (float)i / (count - 1);
            var angle = Mathf.Lerp(arc_min, arc_max, t);
            TelegraphShootShort(angle);
        }
    }

    private void TelegraphShootLong(float angle)
    {
        Resources.Load<ParticleSystem>("particles/ps_telegraph_shoot_long")
            .Duplicate()
            .Parent(transform)
            .Scale(Vector3.one)
            .Position(body_shooter.GetFrontPosition())
            .Rotation(transform.rotation * Quaternion.AngleAxis(angle, Vector3.forward))
            .Destroy(3f);
    }

    private void TelegraphShootShort(float angle)
    {
        Resources.Load<ParticleSystem>("particles/ps_telegraph_shoot_short")
            .Duplicate()
            .Parent(transform)
            .Scale(Vector3.one)
            .Position(body_shooter.GetFrontPosition())
            .Rotation(transform.rotation * Quaternion.AngleAxis(angle, Vector3.forward))
            .Destroy(3f);
    }

    private void TelegraphCircle()
    {
        var eye_transforms = body_shooter.GetCircleTransforms();

        foreach(var t in eye_transforms)
        {
            var angle = Vector3.SignedAngle(Vector3.up, t.up, Vector3.forward);
            var psd = Resources.Load<ParticleSystem>("particles/ps_telegraph_shoot_short")
            .Duplicate()
            .Parent(transform)
            .Scale(Vector3.one)
            .Position(t.position)
            .Rotation(transform.rotation * Quaternion.AngleAxis(angle, Vector3.forward))
            .Destroy(3f);
        }
    }
}