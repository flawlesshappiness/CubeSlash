using UnityEngine;

public interface IHurt
{
    public bool CanHurt();
    public Vector3 GetPosition();
}