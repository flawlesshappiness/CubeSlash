using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class BackgroundSprite : MonoBehaviour
{
    public SpriteRenderer spr;

    public int Layer;
    public Vector3 StartPosition { get; set; }
    public Sprite Sprite { set { spr.sprite = value; } }
    public int SortingOrder { set { spr.sortingOrder = value * 10 + 1; } }

    public Coroutine AnimateAppear(float time)
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(Random.Range(0f, time * 0.5f));
            yield return Lerp.Alpha(spr, time, 1);
        }
    }

    public Coroutine Destroy(float time)
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(Random.Range(0, time * 0.5f));
            yield return Lerp.Alpha(spr, time, 0);
            Destroy(gameObject);
        }
    }
}