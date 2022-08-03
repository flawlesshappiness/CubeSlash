using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityAI : MonoBehaviour
{
    protected Enemy Self { get; private set; }
    protected Vector3 Position { get { return Self.transform.position; } }
    protected Vector3 PlayerPosition { get { return Player.Instance.transform.position; } }
    public void Initialize(Enemy enemy)
    {
        Self = enemy;
    }

    public void Kill()
    {
        AITargetController.Instance.ClearArtifacts(Self);
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
        return Player.Instance.Health.Value > Player.Instance.Health.Min;
    }

    protected float AngleTowards(Vector3 position)
    {
        var dir = position - Self.transform.position;
        return Vector3.SignedAngle(Self.MoveDirection, dir, Vector3.forward);
    }

    protected void MoveTowards(Vector3 position, float speed_turn, bool turn = true)
    {
        if (turn)
        {
            TurnTowards(position, speed_turn);
        }
        Self.Move(Self.MoveDirection);
    }

    protected void TurnTowards(Vector3 position, float turn)
    {
        if (Self.MovementLock.IsFree)
        {
            var angle = AngleTowards(position);
            var z = Mathf.Clamp(turn * Time.deltaTime, 0, angle.Abs()) * Mathf.Sign(angle);
            Self.Character.transform.rotation *= Quaternion.Euler(0, 0, z);
            Self.Character.SetLookDirection(Self.MoveDirection);
        }
    }

    protected bool HasPlayerArtifact { get; private set; }
    protected bool RequestPlayerArtifact()
    {
        HasPlayerArtifact = AITargetController.Instance.RequestArtifact(Self, Player.Instance.transform);
        return HasPlayerArtifact;
    }

    protected void RemovePlayerArtifact()
    {
        AITargetController.Instance.RemoveArtifact(Self, Player.Instance.transform);
        HasPlayerArtifact = false;
    }
}