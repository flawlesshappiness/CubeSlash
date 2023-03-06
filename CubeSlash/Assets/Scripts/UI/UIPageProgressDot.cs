using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class UIPageProgressDot : MonoBehaviour
{
    [SerializeField] private RectTransform pivot_dot;

    private Coroutine cr_animate;

    private void Start()
    {
        pivot_dot.localScale = Vector3.zero;
    }

    public Coroutine AnimateShow()
    {
        StopAnimating();
        cr_animate = StartCoroutine(Cr());
        return cr_animate;
        IEnumerator Cr()
        {
            yield return LerpEnumerator.LocalScale(pivot_dot, 0.2f, Vector3.one);
        }
    }

    public Coroutine AnimateHide()
    {
        StopAnimating();
        cr_animate = StartCoroutine(Cr());
        return cr_animate;
        IEnumerator Cr()
        {
            yield return LerpEnumerator.LocalScale(pivot_dot, 0.2f, Vector3.zero);
        }
    }

    private void StopAnimating()
    {
        if(cr_animate != null)
        {
            StopCoroutine(cr_animate);
        }
        cr_animate = null;
    }
}