using System.Linq;
using UnityEngine;

public class Body : MonoBehaviourExtended
{
    [Header("BODY")]
    [SerializeField] public Transform pivot_main;
    [SerializeField] public Transform pivot_sprite;
    [SerializeField] public Animator animator_main;
    [SerializeField] public CircleCollider2D Collider;
    [SerializeField] public CircleCollider2D Trigger;
    [SerializeField] public ParticleSystem ps_death;
    public bool scale_by_velocity;

    public float Size { get; set; } = 1f;

    private Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(ComponentSearchType.PARENT); } }

    private Quaternion rotation_look;

    public virtual void Initialize()
    {
        
    }

    public void SetLookDirection(Vector3 direction)
    {
        var q = Quaternion.LookRotation(Vector3.forward, direction);
        rotation_look = q;
    }

    private void Update()
    {
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation_look, 10 * Time.deltaTime);

        UpdateScaleByVelocity();
    }

    public Transform GetTransform(string name)
    {
        return GetComponentsInChildren<Transform>()
            .FirstOrDefault(t => t.name == name);
    }

    public void SetCollisionEnabled(bool enabled)
    {
        Trigger.enabled = enabled;
        Collider.enabled = enabled;
    }

    private void UpdateScaleByVelocity()
    {
        if (!scale_by_velocity) return;
        var t_velocity = Mathf.Clamp(Rigidbody.velocity.magnitude / 40, 0, 1);
        float x_scale = Mathf.Lerp(1f, 0.5f, t_velocity);
        transform.localScale = Vector3.Slerp(transform.localScale, Size * new Vector3(x_scale, 1), 10 * Time.deltaTime);
    }
}
