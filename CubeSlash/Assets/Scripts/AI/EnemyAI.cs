using Flawliz.Lerp;
using System.Collections.Generic;
using UnityEngine;

public abstract class EnemyAI : MonoBehaviour
{
    protected Enemy Self { get; private set; }
    protected Vector3 Position { get { return Self.transform.position; } }
    protected Vector3 PlayerPosition { get { return Player.Instance.transform.position; } }
    protected EnemyBody Body { get { return Self.Body as EnemyBody; } }
    public virtual void Initialize(Enemy enemy)
    {
        Self = enemy;
    }

    public void Kill()
    {
        Destroy(gameObject);
    }

    protected Vector3 GetPositionNearPlayer()
    {
        return Player.Instance.transform.position + Random.insideUnitCircle.ToVector3() * Random.Range(1f, 15f);
    }

    protected float DistanceToPlayer()
    {
        return Vector3.Distance(transform.position, Player.Instance.transform.position);
    }

    protected Vector3 DirectionToPlayer()
    {
        return Player.Instance.transform.position - Position;
    }

    protected bool IsPlayerAlive()
    {
        return !Player.Instance.IsDead;
    }

    protected float AngleTowards(Vector3 position)
    {
        var dir = position - Self.transform.position;
        return Vector3.SignedAngle(Self.MoveDirection, dir, Vector3.forward);
    }

    protected void MoveTowards(Vector3 destination)
    {
        var direction = GetOpenDirectionTowards(destination);
        Self.Move(direction);
    }

    protected void MoveToStop(float mul = 1f)
    {
        var v = Self.Rigidbody.velocity;
        if (v.magnitude > 0)
        {
            Self.Rigidbody.AddForce(-v * mul * Self.Rigidbody.mass);
        }
    }

    protected void TurnTowards(Vector3 position, float angle_min = 25)
    {
        if (Self.MovementLock.IsFree)
        {
            var angle = AngleTowards(position);
            var t = angle_min == 0 ? 1 : Mathf.Clamp(angle.Abs() / angle_min, 0, 1);
            var vel_max = Self.Settings.angular_velocity * t;
            Self.AngularVelocity = vel_max;
            Self.Turn(angle < 0);
        }
    }

    protected Vector3 GetOpenDirectionTowards(Vector3 destination)
    {
        var raycast_distance = 5f;
        var forward = (destination - Self.transform.position).normalized;
        var angle = 0;
        var angle_delta = 30;
        while (angle < 180)
        {
            var directions = new List<Vector3>();
            if (angle == 0)
            {
                directions.Add(forward);
            }
            else
            {
                var left = Quaternion.AngleAxis(angle, Vector3.forward) * forward;
                var right = Quaternion.AngleAxis(-angle, Vector3.forward) * forward;
                directions.Add(left);
                directions.Add(right);
            }

            foreach (var dir in directions)
            {
                //var hits = Physics2D.RaycastAll(Self.transform.position, dir, raycast_distance);
                var radius = Self.Settings.size * 0.5f;
                var origin = Self.transform.position + dir * radius;
                var hits = Physics2D.CircleCastAll(origin, radius, dir, raycast_distance);
                if (AnyValidHits(hits))
                {
                    Debug.DrawLine(Self.transform.position, Self.transform.position + dir * raycast_distance, Color.red);
                    continue;
                }
                else
                {
                    Debug.DrawLine(Self.transform.position, Self.transform.position + dir * raycast_distance, Color.green);
                }

                return dir.normalized;
            }

            angle += angle_delta;
        }

        return forward.normalized;

        bool AnyValidHits(RaycastHit2D[] hits)
        {
            foreach (var hit in hits)
            {
                var enemy = hit.collider.GetComponentInParent<Enemy>();
                if (enemy != null && enemy != Self) return true;

                var obstacle = hit.collider.GetComponentInParent<Obstacle>();
                if (obstacle != null && !obstacle.enemy_ai_ignore) return true;
            }

            return false;
        }
    }

    protected void LerpAngularVelocity(float time, float end) => Lerp.Value("angular_velocity_" + Self.GetInstanceID(), time, Self.AngularVelocity, end, f => Self.AngularVelocity = f);
    protected void LerpAngularAcceleration(float time, float end) => Lerp.Value("angular_acceleration_" + Self.GetInstanceID(), time, Self.AngularAcceleration, end, f => Self.AngularAcceleration = f);
    protected void LerpLinearVelocity(float time, float end) => Lerp.Value("linear_velocity_" + Self.GetInstanceID(), time, Self.LinearVelocity, end, f => Self.LinearVelocity = f);
    protected void LerpLinearAcceleration(float time, float end) => Lerp.Value("linear_acceleration_" + Self.GetInstanceID(), time, Self.LinearAcceleration, end, f => Self.LinearAcceleration = f);
}