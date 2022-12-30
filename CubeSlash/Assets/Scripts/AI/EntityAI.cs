using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Flawliz.Lerp;
using System.Linq;

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
        var dir = position - transform.position;
        Self.Move(dir);
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

    protected void LerpAngularVelocity(float time, float end) => Lerp.Value("angular_velocity_" + Self.GetInstanceID(), time, Self.AngularVelocity, end, f => Self.AngularVelocity = f);
    protected void LerpAngularAcceleration(float time, float end) => Lerp.Value("angular_acceleration_" + Self.GetInstanceID(), time, Self.AngularAcceleration, end, f => Self.AngularAcceleration = f);
    protected void LerpLinearVelocity(float time, float end) => Lerp.Value("linear_velocity_" + Self.GetInstanceID(), time, Self.LinearVelocity, end, f => Self.LinearVelocity = f);
    protected void LerpLinearAcceleration(float time, float end) => Lerp.Value("linear_acceleration_" + Self.GetInstanceID(), time, Self.LinearAcceleration, end, f => Self.LinearAcceleration = f);

    protected void ShieldDuds(float duration)
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            Self.EnemyBody.Duds.Where(d => d.IsActive()).ToList().ForEach(d => d.SetArmorActive(true));
            yield return new WaitForSeconds(duration);
            Self.EnemyBody.Duds.Where(d => d.IsActive()).ToList().ForEach(d => d.SetArmorActive(false));
        }
    }

    protected void HideDuds(float duration)
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            Self.EnemyBody.Duds.Where(d => d.IsActive()).ToList().ForEach(d => d.SetDudActive(false));
            yield return new WaitForSeconds(duration);
            Self.EnemyBody.Duds.Where(d => d.IsActive()).ToList().ForEach(d => d.SetDudActive(true));
        }
    }
}