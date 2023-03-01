using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyBody : Body
{
    [Header("DUDS (OPTIONAL)")]
    [SerializeField] public HealthDud template_dud;
    [SerializeField] private List<DudsTransformList> duds_map = new List<DudsTransformList>();
    
    public List<HealthDud> Duds { get { return health_duds.ToList(); } }

    private List<HealthDud> health_duds = new List<HealthDud>();

    public System.Action<HealthDud> OnDudKilled;

    public bool HasDuds { get; private set; }

    [System.Serializable]
    private class DudsTransformList
    {
        public List<Transform> duds = new List<Transform>();
    }

    private void OnDrawGizmos()
    {
        var colors = new Color[] { Color.green, Color.yellow, Color.red };
        for (int i_map = 0; i_map < duds_map.Count; i_map++)
        {
            var color = colors[i_map];
            var map = duds_map[i_map];
            for (int i_t = 0; i_t < map.duds.Count; i_t++)
            {
                var t = map.duds[i_t];
                Gizmos.color = color;
                Gizmos.DrawSphere(t.position, 0.05f);
                Gizmos.DrawLine(t.position, t.position + t.up * 0.1f);
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
        for (int i_map = 0; i_map < duds_map.Count; i_map++)
        {
            if (i_map > DifficultyController.Instance.DifficultyIndex) continue;
            var map = duds_map[i_map];
            for (int i_t = 0; i_t < map.duds.Count; i_t++)
            {
                var t = map.duds[i_t];
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

    public bool HasLivingDuds() => health_duds.Any(dud => dud.IsAlive());
}