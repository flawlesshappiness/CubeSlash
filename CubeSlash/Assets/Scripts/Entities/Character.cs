using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviourExtended
{
    public enum Type { CIRCLE, SQUARE }
    [SerializeField] public CircleCollider2D Collider;
    [SerializeField] public CircleCollider2D Trigger;
    private Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(ComponentSearchType.PARENT); } }

    private Quaternion rotation_look;

    private void Start()
    {
        SetDesiredTriggerSize("default", Trigger.radius);
    }

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

    #region TRIGGER
    private Dictionary<string, float> desired_trigger_sizes = new Dictionary<string, float>();

    public void SetDesiredTriggerSize(string id, float f)
    {
        if (desired_trigger_sizes.ContainsKey(id))
        {
            desired_trigger_sizes[id] = f;
        }
        else
        {
            desired_trigger_sizes.Add(id, f);
        }

        UpdateTriggerSize();
    }

    public void RemoveDesiredTriggerSize(string id)
    {
        if (desired_trigger_sizes.ContainsKey(id))
        {
            desired_trigger_sizes.Remove(id);
            UpdateTriggerSize();
        }
    }

    private void UpdateTriggerSize()
    {
        float biggest = 0f;
        foreach(var kvp in desired_trigger_sizes)
        {
            if(kvp.Value > biggest)
            {
                biggest = kvp.Value;
            }
        }

        Trigger.radius = biggest;
    }
    #endregion
}
