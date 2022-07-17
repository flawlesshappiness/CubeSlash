using UnityEngine;

public class BackgroundSprite : MonoBehaviour
{
    public SpriteRenderer spr;

    public int Layer;
    public Vector3 StartPosition { get; set; }
    public Sprite Sprite { set { spr.sprite = value; } }
    public int SortingOrder { set { spr.sortingOrder = value * 10 + 1; } }

    public void Destroy(float time)
    {
        Lerp.Color(spr, time, spr.color.SetA(0))
            .Delay(Random.Range(0, time * 0.5f))
            .OnEnd(() =>
            {
                Destroy(gameObject);
            });
    }
}