using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class EntityAI : MonoBehaviour
{
    protected Enemy Self { get; private set; }
    public void Initialize(Enemy enemy)
    {
        Self = enemy;
    }

    protected Vector3 GetPositionNearPlayer()
    {
        return Player.Instance.transform.position + Random.insideUnitCircle.ToVector3() * Random.Range(2f, 5f);
    }

    protected float DistanceToPlayer()
    {
        return Vector3.Distance(transform.position, Player.Instance.transform.position);
    }

    protected bool PlayerIsAlive()
    {
        return Player.Instance.Health.Value > Player.Instance.Health.Min;
    }
}