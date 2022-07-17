using UnityEngine;

public class FogLayer : MonoBehaviour
{
    public SpriteRenderer spr;

    public int SortingOrder { set { spr.sortingOrder = value * 10 + 9; } }

    private void Start()
    {
        var w = CameraController.Instance.Width * 1.1f;
        var h = CameraController.Instance.Height * 1.1f;
        spr.transform.localScale = new Vector3(w, h, 1);
    }

    public void Destroy(float time)
    {
        Lerp.Color(spr, time, spr.color.SetA(0))
            .OnEnd(() =>
            {
                Destroy(gameObject);
            });
    }
}