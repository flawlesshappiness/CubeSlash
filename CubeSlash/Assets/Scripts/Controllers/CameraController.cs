using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : Singleton
{
    public static CameraController Instance { get { return Instance<CameraController>(); } }
    public Camera Camera { get; private set; }
    public Transform Target { get; set; }
    public float Height { get { return Camera.orthographicSize * 2f; } }
    public float Width { get { return Height * Camera.aspect; } }

    public override void Initialize()
    {
        base.Initialize();
        Camera = Camera.main;
    }

    private void Update()
    {
        if (Target == null) return;

        var z = Camera.transform.position.z;
        Camera.transform.position = Vector3.Slerp(Camera.transform.position, Target.position.SetZ(z), 5 * Time.deltaTime);
    }
}
