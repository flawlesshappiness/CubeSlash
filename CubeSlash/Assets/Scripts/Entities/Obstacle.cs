using UnityEngine;

public class Obstacle : MonoBehaviour, IKillable, IHurt
{
    public bool hurts;

    public bool CanKill() => false;

    public Vector3 GetPosition() => transform.position;

    public virtual void Kill() { }
    public bool CanHurt() => hurts;

    protected virtual void OnEnable()
    {
        ObjectController.Instance.Add(gameObject);
    }

    protected virtual void OnDestroy()
    {
        ObjectController.Instance.Remove(gameObject);
    }
}