using UnityEngine;

public interface IKillable
{
    public bool CanHit();
    public bool TryKill();
    public bool CanKill();
    public Vector3 GetPosition();
}