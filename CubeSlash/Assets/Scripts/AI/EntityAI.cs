using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityAI : MonoBehaviour
{
    protected Enemy Self { get; private set; }
    protected Vector3 Position { get { return Self.transform.position; } }
    protected Vector3 PlayerPosition { get { return Player.Instance.transform.position; } }
    public virtual void Initialize(Enemy enemy)
    {
        Self = enemy;
    }

    public void Kill()
    {
        Destroy(gameObject);
    }

    public virtual void Knockback(float time, Vector3 velocity, float drag)
    {
        StartCoroutine(Cr());

        IEnumerator Cr()
        {
            var time_end = Time.time + time;
            Self.MovementLock.AddLock("Knockback");
            Self.DragLock.AddLock("Knockback");
            Self.Rigidbody.velocity = velocity;
            while (Time.time < time_end)
            {
                Self.Rigidbody.velocity = Self.Rigidbody.velocity * drag;
                yield return null;
            }
            Self.MovementLock.RemoveLock("Knockback");
            Self.DragLock.RemoveLock("Knockback");
        }
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

    protected void MoveTowards(Vector3 position)
    {
        Self.Move(Self.MoveDirection);
    }

    protected void MoveToStop(float mul = 1f)
    {
        var v = Self.Rigidbody.velocity;
        if(v.magnitude > 0)
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

    protected void LerpAngularVelocity(float time, float end) => Lerp.Value(time, Self.AngularVelocity, end, "angular_velocity_" + Self.GetInstanceID(), f => Self.AngularVelocity = f);
    protected void LerpAngularAcceleration(float time, float end) => Lerp.Value(time, Self.AngularAcceleration, end, "angular_acceleration_" + Self.GetInstanceID(), f => Self.AngularAcceleration = f);
    protected void LerpLinearVelocity(float time, float end) => Lerp.Value(time, Self.LinearVelocity, end, "linear_velocity_" + Self.GetInstanceID(), f => Self.LinearVelocity = f);
    protected void LerpLinearAcceleration(float time, float end) => Lerp.Value(time, Self.LinearAcceleration, end, "linear_acceleration_" + Self.GetInstanceID(), f => Self.LinearAcceleration = f);
}