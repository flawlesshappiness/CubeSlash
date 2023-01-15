using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class FogLayer : MonoBehaviour
{
    public SpriteRenderer spr;
    public Transform shadow;

    public int SortingOrder { set { spr.sortingOrder = value * 10 + 9; } }

    private void Update()
    {
        var w = CameraController.Instance.Width * 1.1f;
        var h = CameraController.Instance.Height * 1.1f;
        spr.transform.localScale = new Vector3(w, h, 1);
        shadow.transform.localScale = new Vector3(w, w, 1);
    }

    public Coroutine Destroy(float time)
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return Lerp.Alpha(spr, time, 0);
            Destroy(gameObject);

        }
    }
}