using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour, IInitializable
{
    public static CameraController Instance { get; private set; }
    public Camera Camera { get; private set; }
    public Transform Target { get; set; }
    public float Height { get { return Camera.orthographicSize * 2f; } }
    public float Width { get { return Height * Camera.aspect; } }

    public void Initialize()
    {
        Instance = this;
        Camera = Camera.main;
    }

    private void Update()
    {
        if (Target == null) return;

        var z = Camera.transform.position.z;
        Camera.transform.position = Vector3.Slerp(Camera.transform.position, Target.position.SetZ(z), 5 * Time.deltaTime);
    }
}
