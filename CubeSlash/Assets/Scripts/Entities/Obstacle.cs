using UnityEngine;

public class Obstacle : MonoBehaviour, IKillable, IHurt
{
    public bool hurts;

    public bool CanHit() => false;
    public bool CanKill() => false;

    public Vector3 GetPosition() => transform.position;

    public virtual bool TryKill() => false;
    public bool CanHurt() => hurts;
}