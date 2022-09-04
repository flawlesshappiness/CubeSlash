using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class UIHealthPoint : MonoBehaviour
{
    [SerializeField] private GameObject g_full;
    [SerializeField] private GameObject g_temp;
    [SerializeField] private GameObject g_empty;

    private UIHealth parent;

    public HealthPoint Target { get; private set; }
    public HealthPoint.Type Type { get { return Target.HealthType; } }

    public void Initialize(UIHealth parent, HealthPoint target)
    {
        this.parent = parent;

        Target = target;
        SetType(Type);

        Target.onFull += _OnFull;
        Target.onEmpty += _OnEmpty;
        Target.onDestroy += _OnDestroy;
    }

    private void OnDisable()
    {
        if(Target != null)
        {
            Target.onFull -= _OnFull;
            Target.onEmpty -= _OnEmpty;
            Target.onDestroy -= _OnDestroy;
        }
    }

    public void SetType(HealthPoint.Type type)
    {
        g_full.SetActive(type == HealthPoint.Type.FULL);
        g_empty.SetActive(type == HealthPoint.Type.EMPTY);
        g_temp.SetActive(type == HealthPoint.Type.TEMPORARY);
    }

    private void _OnFull()
    {
        SetType(HealthPoint.Type.FULL);
    }

    private void _OnEmpty()
    {
        SetType(HealthPoint.Type.EMPTY);
    }

    private void _OnDestroy()
    {
        parent.RemoveHealthPoint(this);
        Destroy(gameObject);
    }
}
