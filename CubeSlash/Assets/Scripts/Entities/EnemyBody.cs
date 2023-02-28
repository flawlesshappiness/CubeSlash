using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyBody : Body
{
    [Header("DUDS (OPTIONAL)")]
    [SerializeField] public Transform parent_health_duds;
    [SerializeField] public HealthDud template_dud;
    
    public List<HealthDud> Duds { get { return health_duds.ToList(); } }

    private List<HealthDud> health_duds = new List<HealthDud>();

    public System.Action<HealthDud> OnDudKilled;

    public bool HasDuds { get; private set; }

    private void OnDrawGizmos()
    {
        if (parent_health_duds != null)
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

    public override void Initialize()
    {
        base.Initialize();
        InitializeHealthDuds();
    }

    private void InitializeHealthDuds()
    {
        if (parent_health_duds != null)
        {
            foreach (var t in parent_health_duds.GetComponentsInChildren<Transform>())
            {
                if (t != parent_health_duds)
                {
                    var dud = Instantiate(template_dud, t);
                    dud.transform.SetGlobalScale(t.localScale);
                    dud.transform.localPosition = Vector3.zero;
                    dud.transform.localRotation = Quaternion.identity;
                    dud.Initialize();
                    dud.OnKilled += () => OnDudKilled?.Invoke(dud);
                    health_duds.Add(dud);
                }
            }

            HasDuds = health_duds.Count > 0;
        }
    }

    public bool HasActiveHealthDuds() => parent_health_duds != null && health_duds.Any(dud => dud.IsActive());
}