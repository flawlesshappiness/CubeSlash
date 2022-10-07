using UnityEngine;

public interface IKillable
{
    public void Kill();
    public bool CanKill();
    public Vector3 GetPosition();
}