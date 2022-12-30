using UnityEngine;

public class Obstacle : MonoBehaviour, IKillable, IHurt
{
    public bool hurts;

    public bool CanKill() => false;

    public Vector3 GetPosition() => transform.position;

    public void Kill()
    {
        // No
    }

    public bool CanHurt() => hurts;
}