using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Character : MonoBehaviourExtended
{
    public enum Type { CIRCLE, SQUARE }
    [SerializeField] public CircleCollider2D Collider;
    [SerializeField] public CircleCollider2D Trigger;

    [Header("PARASITE")]
    [Min(0)] public int parasite_level;
    [SerializeField] public List<ParasiteSpace> ParasiteSpaces = new List<ParasiteSpace>();
    private Rigidbody2D Rigidbody { get { return GetComponentOnce<Rigidbody2D>(ComponentSearchType.PARENT); } }

    private Quaternion rotation_look;

    [System.Serializable]
    public class ParasiteSpace
    {
        public Transform transform;
        [HideInInspector] public Enemy parasite;
        public Character Character { get; set; }

        public bool inactive_when_parent_parasite;

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
            var blocked = e != null && e.IsParasite && inactive_when_parent_parasite;
            return empty && active && !blocked;
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

        float t_scale_vel = Mathf.Clamp(Rigidbody.velocity.magnitude / 40, 0, 1);
        float x_scale = Mathf.Lerp(1f, 0.5f, t_scale_vel);
        transform.localScale = Vector3.Slerp(transform.localScale, Vector3.one.SetX(x_scale), 10 * Time.deltaTime);
    }
}
