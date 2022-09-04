using UnityEngine;

public class HealthPoint
{
    public enum Type { FULL, EMPTY, TEMPORARY }
    public Type HealthType { get; set; }

    public event System.Action onFull;
    public event System.Action onEmpty;
    public event System.Action onDestroy;

    public void Destroy() => onDestroy?.Invoke();
    public void Empty()
    {
        HealthType = Type.EMPTY;
        onEmpty?.Invoke();
    }
    public void Fill()
    {
        HealthType = Type.FULL;
        onFull?.Invoke();
    }
}