using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class BackgroundSprite : BackgroundObject
{
    public SpriteRenderer spr;

    public Sprite Sprite { set { spr.sprite = value; } }
    public int SortingOrder { set { spr.sortingOrder = value; } }

    public override void Initialize(Area area)
    {
        base.Initialize(area);
        Scale = transform.localScale.x;
    }

    public override void UpdateParallax(Vector3 camera_position)
    {
        base.UpdateParallax(camera_position);

        var dir_from_cam = transform.position - camera_position;
        var w_max = Scale + World.x;
        var h_max = Scale + World.y;
        if (dir_from_cam.x < -w_max) Offset += new Vector3(w_max * 2, 0);
        if (dir_from_cam.x > w_max) Offset -= new Vector3(w_max * 2, 0);
        if (dir_from_cam.y < -h_max) Offset += new Vector3(0, h_max * 2);
        if (dir_from_cam.y > h_max) Offset -= new Vector3(0, h_max * 2);
    }

    public override void Destroy()
    {
        AnimateDestroy(BackgroundController.OBJECT_FADE_TIME);
    }

    public override void DestroyImmediate()
    {
        Destroy(gameObject);
    }

    public Coroutine AnimateAppear(float time)
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(Random.Range(0f, time * 0.5f));
            yield return LerpEnumerator.Alpha(spr, time, 1);
        }
    }

    public Coroutine AnimateDestroy(float time)
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(Random.Range(0, time * 0.5f));
            yield return LerpEnumerator.Alpha(spr, time, 0);
            Destroy(gameObject);
        }
    }
}