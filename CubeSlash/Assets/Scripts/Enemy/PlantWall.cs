using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class PlantWall : Obstacle
{
    [SerializeField] private Transform pivot_animation;
    [SerializeField] private Transform pivot_dud;
    [SerializeField] private ParticleSystem ps_death;

    public Vector3 GetDudPosition() => pivot_dud.position;
    public Vector3 GetDudScale() => pivot_dud.localScale;
    public Quaternion GetDudRotation() => pivot_dud.rotation;

    public CustomCoroutine AnimateAppear()
    {
        return this.StartCoroutineWithID(Cr(), "scale_"+GetInstanceID());
        IEnumerator Cr()
        {
            yield return LerpEnumerator.LocalScale(pivot_animation, 1f, Vector3.zero, Vector3.one);
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

    public void Destroy()
    {
        ps_death.Duplicate()
            .Position(transform.position)
            .Play()
            .Destroy(10);

        Destroy(gameObject);
    }
}