using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class PlantWall : Obstacle
{
    [SerializeField] private Transform pivot_animation;
    [SerializeField] private Transform pivot_dud;
    [SerializeField] private SpriteRenderer spr;
    [SerializeField] private ParticleSystem ps_death;

    public Transform GetDudTransform() => pivot_dud;

    public void SetHidden()
    {
        pivot_animation.localScale = Vector3.zero;
    }

    public CustomCoroutine AnimateAppear(float duration = 1f)
    {
        return this.StartCoroutineWithID(Cr(), "scale_"+GetInstanceID());
        IEnumerator Cr()
        {
            yield return LerpEnumerator.LocalScale(pivot_animation, duration, Vector3.zero, Vector3.one);
        }
    }

    public CustomCoroutine AnimateDisappear()
    {
        return this.StartCoroutineWithID(Cr(), "scale_" + GetInstanceID());
        IEnumerator Cr()
        {
            yield return LerpEnumerator.LocalScale(pivot_animation, 1f, Vector3.zero);
        }
    }

    public void SetSortingOrder(int order)
    {
        spr.sortingOrder = order;
    }

    public override void Kill()
    {
        base.Kill();

        ps_death.Duplicate()
            .Parent(GameController.Instance.world)
            .Play()
            .Destroy(10);

        Destroy(gameObject);
    }
}