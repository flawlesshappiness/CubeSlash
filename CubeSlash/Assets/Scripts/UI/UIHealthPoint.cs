using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class UIHealthPoint : MonoBehaviour
{
    [SerializeField] private Animator animator_img;

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
        animator_img.SetInteger("type", (int)type);
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
        animator_img.SetTrigger("destroy");
        parent.RemoveHealthPoint(this);
        StartCoroutine(Cr());

        IEnumerator Cr()
        {
            yield return new WaitForSeconds(0.5f);
            Destroy(gameObject);
        }
    }
}
