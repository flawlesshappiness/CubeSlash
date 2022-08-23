using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Character : MonoBehaviourExtended
{
    public enum Type { CIRCLE, SQUARE }
    [SerializeField] public CircleCollider2D Collider;
    [SerializeField] public CircleCollider2D Trigger;
    [SerializeField] public Transform parent_health_duds;
    [SerializeField] public ParticleSystem ps_death;
    private Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(ComponentSearchType.PARENT); } }
    private Quaternion rotation_look;
    private List<HealthDud> health_duds = new List<HealthDud>();

    public System.Action<HealthDud> OnDudKilled;

    public List<HealthDud> Duds { get { return health_duds.ToList(); } }

    public void Initialize()
    {
        if(parent_health_duds != null)
        {
            foreach(var t in parent_health_duds.GetComponentsInChildren<Transform>())
            {
                if(t != parent_health_duds)
                {
                    var dud = Instantiate(Resources.Load<HealthDud>("Prefabs/Entities/HealthDud"), t);
                    dud.transform.SetGlobalScale(t.localScale);
                    dud.transform.localPosition = Vector3.zero;
                    dud.transform.localRotation = Quaternion.identity;
                    dud.Initialize();
                    dud.OnKilled += () => OnDudKilled?.Invoke(dud);
                    health_duds.Add(dud);
                }
            }
        }
    }

    public bool HasActiveHealthDuds() => parent_health_duds != null && health_duds.Any(dud => dud.IsActive());

    public void SetLookDirection(Vector3 direction)
    {
        var q = Quaternion.LookRotation(Vector3.forward, direction);
        rotation_look = q;
    }

    private void Update()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation_look, 10 * Time.deltaTime);

        var t_velocity = Mathf.Clamp(Rigidbody.velocity.magnitude / 40, 0, 1);
        float x_scale = Mathf.Lerp(1f, 0.5f, t_velocity);
        transform.localScale = Vector3.Slerp(transform.localScale, Vector3.one.SetX(x_scale), 10 * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        if(parent_health_duds != null)
        {
            foreach (var t in parent_health_duds.GetComponentsInChildren<Transform>())
            {
                if (t != parent_health_duds)
                {
                    Gizmos.color = Color.yellow;
                    Gizmos.DrawSphere(t.position, 0.05f);
                    Gizmos.DrawLine(t.position, t.position + t.up * 0.1f);
                }
            }
        }
    }

    public Transform GetTransform(string name)
    {
        return GetComponentsInChildren<Transform>()
            .FirstOrDefault(t => t.name == name);
    }
}
