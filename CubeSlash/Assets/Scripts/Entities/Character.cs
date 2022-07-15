using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Character : MonoBehaviourExtended
{
    public enum Type { CIRCLE, SQUARE }
    [SerializeField] public CircleCollider2D Collider;
    [SerializeField] public CircleCollider2D Trigger;
    [SerializeField] private ParticleSystem ps_trail;
    [SerializeField] private AnimationCurve ac_trail_ratio;

    [Header("PARASITE")]
    [SerializeField] public List<ParasiteSpace> ParasiteSpaces = new List<ParasiteSpace>();
    private Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(ComponentSearchType.PARENT); } }

    private Quaternion rotation_look;

    [System.Serializable]
    public class ParasiteSpace
    {
        public Transform transform;
        [HideInInspector] public Enemy parasite;
        public Character Character { get; set; }

        public Vector3 Position { get { return transform.position; } }
        public bool Available { get { return IsAvailable(); } }

        public void SetParasite(Enemy e)
        {
            var host = Character.GetComponentInParent<Enemy>();
            parasite = e;
            e.transform.position = transform.position;
            e.transform.parent = transform;
            e.SetParasiteHost(host);
            e.OnDeath += () => 
            {
                parasite = null;
                e.transform.parent = GameController.Instance.world;
                e.RemoveParasiteHost();
            };
        }

        private bool IsAvailable()
        {
            var empty = parasite == null;
            var active = Character.gameObject.activeInHierarchy;
            var e = Character.GetComponentInParent<Enemy>();
            return empty && active;
        }
    }

    public ParasiteSpace GetParasiteSpace(Transform t)
    {
        return ParasiteSpaces.FirstOrDefault(space => space.transform == t);
    }

    public void Initialize()
    {
        foreach(var space in ParasiteSpaces)
        {
            space.Character = this;
            AITargetController.Instance.SetArtifactOwnerCount(space.transform, 3);
        }
    }

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

        if(ps_trail != null)
        {
            var trails = ps_trail.trails;
            trails.ratio = ac_trail_ratio.Evaluate(t_velocity);
        }
    }

    private void OnDrawGizmos()
    {
        foreach(var space in ParasiteSpaces)
        {
            if(space.transform != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(space.transform.position, 0.05f);
            }
        }
    }
}
