using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : Singleton
{
    public static CameraController Instance { get { return Instance<CameraController>(); } }
    private Camera _cam;
    public Camera Camera { get { return _cam ?? FindCamera(); } }
    public Transform Target { get; set; }
    public float Height { get { return Camera.orthographicSize * 2f; } }
    public float Width { get { return Height * Camera.aspect; } }

    protected override void Initialize()
    {
        base.Initialize();
    }

    private void LateUpdate()
    {
        if (Target == null) return;

        var z = -10;
        Camera.transform.position = Vector3.Lerp(Camera.transform.position, Target.position.SetZ(z), 5 * Time.deltaTime);
    }

    public Vector3 GetPositionOutsideCamera()
    {
        var dir = Random.insideUnitCircle.normalized.ToVector3();
        var dist = Width * 0.5f * Random.Range(1.3f, 2.0f);
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
        CoroutineController.Instance.Kill("AnimateSize_" + GetInstanceID());
    }

    public CustomCoroutine AnimateSize(float duration, float size, AnimationCurve curve = null)
    {
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
}
