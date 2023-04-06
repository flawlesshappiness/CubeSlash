using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class ExplodeChargeEffect : MonoBehaviour
{
    [SerializeField] private Transform pivot;
    [SerializeField] private SpriteRenderer spr;

    private Coroutine cr;

    private void Start()
    {
        spr.SetAlpha(0);
    }

    public Coroutine Animate(Vector3 start, Vector3 end, float duration)
    {
        StopAnimating();
        cr = StartCoroutine(Cr());
        return cr;
        IEnumerator Cr()
        {
            var q_start = Quaternion.AngleAxis(0f, Vector3.forward);
            var q_end = Quaternion.AngleAxis(duration * 90f, Vector3.forward);

            //Lerp.Alpha(spr, duration * 0.1f, 0f, 0.5f);
            spr.SetAlpha(1);
            yield return LerpEnumerator.Value(duration, f =>
            {
                pivot.localScale = Vector3.Lerp(start, end, f);
                pivot.rotation = Quaternion.Lerp(q_start, q_end, f);
            });
        }
    }

    public void StopAnimating()
    {
        spr.SetAlpha(0);
        if(cr != null)
        {
            StopCoroutine(cr);
        }
    }
}