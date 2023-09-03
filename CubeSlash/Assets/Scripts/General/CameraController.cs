using Flawliz.Lerp;
using System.Collections;
using UnityEngine;

public class CameraController : Singleton
{
    public static CameraController Instance { get { return Instance<CameraController>(); } }
    private Camera _cam;
    public Camera Camera { get { return _cam ?? FindCamera(); } }
    public Transform Target { get; set; }
    public float Height { get { return Camera.orthographicSize * 2f; } }
    public float Width { get { return Height * Camera.aspect; } }
    public Vector3 Offset { get; set; }
    private float TargetSize { get; set; }

    protected override void Initialize()
    {
        base.Initialize();
        AreaController.Instance.onNextArea += OnNextArea;
        GameController.Instance.onMainMenu += OnMainMenu;

        Camera.orthographicSize = 15;
        TargetSize = Camera.orthographicSize;
    }

    private void LateUpdate()
    {
        if (Target == null) return;

        var z = -10;
        Camera.transform.position = Vector3.Lerp(Camera.transform.position, Target.position.SetZ(z), 5f * Time.unscaledDeltaTime);
    }

    public Vector3 GetPositionOutsideCamera()
    {
        var w = TargetSize * 2f * Camera.aspect;
        var dir = Random.insideUnitCircle.normalized.ToVector3();
        var dist = w * 0.5f * Random.Range(1.3f, 2.0f);
        return Camera.transform.position.SetZ(0) + dir * dist;
    }

    private Camera FindCamera()
    {
        _cam = Camera.main;
        return _cam;
    }

    public void SetSize(float size)
    {
        Camera.orthographicSize = size;
        TargetSize = size;
        CoroutineController.Instance.Kill("AnimateSize_" + GetInstanceID());
    }

    public CustomCoroutine AnimateSize(float duration, float size, AnimationCurve curve = null)
    {
        TargetSize = size;
        var start = Camera.orthographicSize;
        return this.StartCoroutineWithID(Cr(), "AnimateSize_" + GetInstanceID());
        IEnumerator Cr()
        {
            yield return LerpEnumerator.Value(duration, f =>
            {
                var t = curve != null ? curve.Evaluate(f) : f;
                Camera.orthographicSize = Mathf.Lerp(start, size, t);
            });
        }
    }

    private void OnNextArea(Area area)
    {
        var settings = GameSettings.Instance;
        var diff = DifficultyController.Instance.DifficultyValue;
        var max_area_count = settings.area_count_difficulty.Evaluate(diff);
        var t = (float)AreaController.Instance.AreaIndex / (max_area_count - 1);
        var size = settings.camera_size_start + settings.camera_size_game.Evaluate(t);
        AnimateSize(15f, size, EasingCurves.EaseInOutQuad);
    }

    private void OnMainMenu()
    {
        //AnimateSize(5f, 15f, EasingCurves.EaseInOutQuad);
    }
}
