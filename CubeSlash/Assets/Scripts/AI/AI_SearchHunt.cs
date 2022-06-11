using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AI_SearchHunt : EntityAI
{
    public List<Stats> stats = new List<Stats>();

    public enum State { SEARCH, TARGET, PARASITE }
    private State state;

    private Transform Target { get; set; }
    private Character.ParasiteSpace ParasiteSpace { get; set; }
    private Vector3 Destination { get; set; }

    private const float DIST_TARGET_MAX = 15f;

    [System.Serializable]
    public class Stats
    {
        [HideInInspector] public string name;
        public State state;
        public float speed_move;
        public float speed_turn;
        public float angle_min;
        public float angle_max;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(Position, Destination);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(Position, Position + Self.MoveDirection * 2);
    }

    private void OnValidate()
    {
        foreach(var stat in stats)
        {
            stat.name = stat.state.ToString();
        }
    }

    private void Start()
    {
        SetState(State.SEARCH);
    }

    private void Update()
    {
        RepositionUpdate();
        StateUpdate();
    }

    private void MoveUpdate()
    {
        var stats = GetStats();
        var angle = Mathf.Clamp(AngleTowards(Destination).Abs(), stats.angle_min, stats.angle_max);
        var t_angle = (angle - stats.angle_min) / (stats.angle_max - stats.angle_min);
        var t_angle_clamp = 1f - Mathf.Clamp(t_angle, 0, 1);
        var speed_move = Mathf.Clamp(stats.speed_move * t_angle_clamp, stats.speed_move * 0.1f, stats.speed_move);
        MoveTowards(Destination, speed_move, stats.speed_turn);
    }

    private void RepositionUpdate()
    {
        var dist_to_player = DistanceToPlayer();
        if (dist_to_player > CameraController.Instance.Width * 1.5f)
        {
            Self.Reposition();
            return;
        }
    }

    private int idx_state = 0;
    private void SetState(State state)
    {
        this.state = state;
        idx_state = 0;
    }

    private void StateUpdate()
    {
        switch (state)
        {
            case State.SEARCH: SearchUpdate(); break;
            case State.TARGET: TargetUpdate(); break;
            case State.PARASITE: ParasiteUpdate(); break;
        }
    }

    public override void Knockback(float time, Vector3 velocity, float drag)
    {
        base.Knockback(time, velocity, drag);
        StartCoroutine(Cr());
        AITargetController.Instance.ClearArtifacts(Self);

        IEnumerator Cr()
        {
            yield return new WaitForSeconds(time);
            SetState(State.SEARCH);
        }
    }

    private Stats GetStats()
    {
        return stats.FirstOrDefault(stat => stat.state == state);
    }

    private Character.ParasiteSpace GetParasiteTarget()
    {
        var targets = new List<Character.ParasiteSpace>();
        var hits = Physics2D.OverlapCircleAll(Position, CameraController.Instance.Width * 0.5f);
        hits.Select(hit => hit.gameObject.GetComponentInParent<Enemy>())
            .Where(e => e != null && e != Self && e.Character.parasite_level > Self.Character.parasite_level)
            .Select(e => e.Character.ParasiteSpaces)
            .ToList().ForEach(spaces =>
                spaces.Where(space => space.Available)
                .ToList().ForEach(space => targets.Add(space))
            );

        targets = targets.OrderBy(space => Vector3.Distance(transform.position, space.transform.position)).ToList();
        return targets.Count == 0 ? null : targets[0];
    }

    private void SearchUpdate()
    {
        var stats = GetStats();
        if (DistanceToPlayer() < DIST_TARGET_MAX && RequestPlayerArtifact())
        {
            Target = Player.Instance.transform;
            SetState(State.TARGET);
        }

        var parasite = GetParasiteTarget();
        if (parasite != null && AITargetController.Instance.RequestArtifact(Self, parasite.transform))
        {
            ParasiteSpace = parasite;
            Target = parasite.transform;
            SetState(State.PARASITE);
        }

        if (Vector3.Distance(transform.position, Destination) < 2f)
        {
            Destination = Player.Instance != null ?
                GetPositionNearPlayer() : transform.position + Random.insideUnitCircle.ToVector3() * Screen.width * Random.Range(0.25f, 0.5f);
        }

        MoveTowards(Destination, stats.speed_move, stats.speed_turn);
    }

    private void TargetUpdate()
    {
        var stats = GetStats();

        if(idx_state == 0)
        {
            if (AngleTowards(Target.position).Abs() > stats.angle_min)
            {
                MoveTowards(Target.position, 0.1f, 100);
            }
            else
            {
                idx_state = 1;
            }
        }
        else if(idx_state == 1)
        {
            var time_stop = Time.time + 5f;
            if (Time.time > time_stop || Vector3.Distance(Position, Target.position) > DIST_TARGET_MAX * 1.5f)
            {
                if (HasPlayerArtifact)
                    RemovePlayerArtifact();
                Target = null;
                SetState(State.SEARCH);
            }
            else
            {
                MoveTowards(Target.position, stats.speed_move, stats.speed_turn);
            }
        }
    }

    private void ParasiteUpdate()
    {
        var stats = GetStats();
        if (ParasiteSpace == null || !ParasiteSpace.Available)
        {
            Target = null;
            ParasiteSpace = null;
            SetState(State.SEARCH);
        }
        else if (Vector3.Distance(Position, Target.position) < 1)
        {
            ParasiteSpace.SetParasite(Self);
            AITargetController.Instance.RemoveArtifact(Self, Target);
        }
        else
        {
            MoveTowards(Target.position, stats.speed_move, stats.speed_turn);
        }
    }
}
