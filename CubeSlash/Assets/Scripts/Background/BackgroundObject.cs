using UnityEngine;

public abstract class BackgroundObject : MonoBehaviour
{
    public Vector3 StartPosition { get; set; }
    public int Layer { get; set; }
    public float Scale { get; set; }
    public Vector3 Offset { get; set; }

    protected Vector2 World { get; private set; }
    private Vector2 parallax;

    private Area area;

    public abstract void Destroy();

    public virtual void Initialize(Area area)
    {
        this.area = area;

        var w = CameraController.Instance.Width;
        var h = CameraController.Instance.Height;
        World = new Vector2(w,h);

        var parallax_min = area.parallax_min;
        var parallax_max = area.parallax_max;
        parallax = new Vector2(parallax_min, parallax_max);

        StartPosition = transform.localPosition;
    }

    public virtual void UpdateParallax(Vector3 camera_position)
    {
        var layer_count = GetLayerCount();
        var t = 1f - Mathf.Clamp(Layer / (float)(layer_count - 1), 0, 1);
        var tp = Mathf.Lerp(parallax.x, parallax.y, t.Abs());
        var pp = StartPosition - camera_position * tp;
        transform.localPosition = camera_position + new Vector3(pp.x, pp.y, StartPosition.z) + Offset;
    }

    private int GetLayerCount() => area.background_layers.Count;
}