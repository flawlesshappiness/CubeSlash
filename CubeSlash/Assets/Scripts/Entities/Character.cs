using System.Collections;
using System.Collections.Generic;
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

        public bool inactive_when_parent_parasite;

        public Vector3 Position { get { return transform.position; } }
        public bool Empty { get { return parasite == null; } }

        public void SetParasite(Enemy e)
        {
            parasite = e;
            e.transform.position = transform.position;
            e.transform.parent = transform;
            e.SetParasite(true);
            e.OnDeath += () => 
            {
                parasite = null;
                e.SetParasite(false);
            };
        }
    }

    public void Initialize()
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

        float t_scale_vel = Mathf.Clamp(Rigidbody.velocity.magnitude / 40, 0, 1);
        float x_scale = Mathf.Lerp(1f, 0.5f, t_scale_vel);
        transform.localScale = Vector3.Slerp(transform.localScale, Vector3.one.SetX(x_scale), 10 * Time.deltaTime);
    }
}
