using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviourExtended
{
    public enum Type { CIRCLE, SQUARE }
    private Collider Collider { get { return GetComponentOnce<Collider>(ComponentSearchType.CHILDREN); } }
    private Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(ComponentSearchType.PARENT); } }

    private Quaternion rotation_look;

    public void SetLookDirection(Vector3 direction)
    {
        var q = Quaternion.LookRotation(Vector3.forward, direction);
        rotation_look = q;
    }

    private void Update()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation_look, 10 * Time.deltaTime);

        float t_scale_vel = Mathf.Clamp(Rigidbody.velocity.magnitude / 40, 0, 1);
        float x_scale = Mathf.Lerp(1f, 0.5f, t_scale_vel);
        transform.localScale = Vector3.Slerp(transform.localScale, Vector3.one.SetX(x_scale), 10 * Time.deltaTime);
    }
}
