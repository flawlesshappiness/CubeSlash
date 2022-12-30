using UnityEngine;
using PathCreation;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class AI_BossPlant : EntityAI
{
    [SerializeField] private PlantWall template_wall;
    [SerializeField] private PlantPillar template_pillar;
    [SerializeField] private HealthDud template_dud;

    private List<PlantWall> walls = new List<PlantWall>();
    private List<PlantPillar> pillars = new List<PlantPillar>();

    private const int EXP_PER_WALL = 3;
    private const int HITPOINTS = 5;
    private const float SIZE_WALL = 5;
    private const float RADIUS = 25;
    private const float COOLDOWN_PILLAR_MAX = 8f;
    private const float COOLDOWN_PILLAR_MIN = 3f;
    private const float PILLAR_SIZE_MIN = 5;
    private const float PILLAR_SIZE_MAX = 8;
    private const float PILLAR_LIFETIME_MIN = 5f;
    private const float PILLAR_LIFETIME_MAX = 10f;

    private int duds_to_kill;
    private float time_until_pillar;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);
        duds_to_kill = HITPOINTS;
        Self.Body.gameObject.SetActive(false);
        Self.transform.position = Player.Instance.transform.position;

        CreateArena();
        CreateDud();
    }

    private void CreateArena()
    {
        var points = CircleHelper.Points(RADIUS, 10);
        var bezier = new BezierPath(points, true, PathSpace.xy);
        var path = new VertexPath(bezier, GameController.Instance.world);
        var offset = Player.Instance.transform.position;

        var wall_length = SIZE_WALL * 0.95f;
        var wall_length_half = wall_length * 0.5f;
        var distance = 0f;
        while (distance < path.length)
        {
            var d = distance + wall_length_half;
            var point = path.GetPointAtDistance(d);
            var direction = path.GetDirectionAtDistance(d);
            var angle = Vector3.SignedAngle(Vector3.up, direction, Vector3.forward);
            var rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            var wall = Instantiate(template_wall, GameController.Instance.world);
            wall.transform.position = point + offset;
            wall.transform.rotation = rotation;
            walls.Add(wall);

            distance += wall_length;
        }
    }

    private void CreateDud()
    {
        var player_position = Player.Instance.transform.position;
        var min_distance = RADIUS * 0.9f;
        var wall = walls.Where(w => Vector3.Distance(w.transform.position, player_position) > min_distance).ToList().Random();
        var dud = Instantiate(template_dud, wall.transform);
        var tdud = wall.GetDudTransform();
        dud.transform.position = tdud.position;
        dud.transform.rotation = tdud.rotation;
        dud.transform.localScale = tdud.localScale;
        dud.OnKilled += OnDudKilled;
        dud.SetDudActive(false, false);
        dud.SetDudActive(true, true);
        dud.SetGlowEnabled(true);
    }

    private void CreatePillar()
    {
        var pillar = Instantiate(template_pillar, GameController.Instance.world);
        pillar.transform.position = Player.Instance.transform.position;
        pillar.SetHidden();
        pillars.Add(pillar);

        var t = GetHealthPercentage();
        var scale = Vector3.one * Mathf.Lerp(PILLAR_SIZE_MIN, PILLAR_SIZE_MAX, t);
        pillar.transform.localScale = scale;

        var lifetime = Mathf.Lerp(PILLAR_LIFETIME_MIN, PILLAR_LIFETIME_MAX, t);
        pillar.AnimateAppear(3f, lifetime);
    }

    private void OnDudKilled()
    {
        if(duds_to_kill > 0)
        {
            CreateDud();
            duds_to_kill--;
        }
        else
        {
            End();
        }
    }

    private void End()
    {
        foreach(var wall in walls)
        {
            // Spawn exp
            for (int i = 0; i < EXP_PER_WALL; i++)
            {
                var rnd = Random.insideUnitCircle * SIZE_WALL * 0.25f;
                var pos = wall.transform.position + rnd.ToVector3();
                var exp = ItemController.Instance.SpawnExperience(pos);
                exp.SetPlant();
            }

            wall.Kill();
        }

        foreach(var pillar in pillars)
        {
            if(pillar != null)
            {
                pillar.Kill();
            }
        }

        Self.Respawn();
    }

    private float GetHealthPercentage()
    {
        return 1f - ((float)duds_to_kill / HITPOINTS);
    }

    private void Update()
    {
        var t = GetHealthPercentage();
        if(Time.time > time_until_pillar)
        {
            CreatePillar();
            var cooldown = Mathf.Lerp(COOLDOWN_PILLAR_MAX, COOLDOWN_PILLAR_MIN, t);
            time_until_pillar = Time.time + cooldown;
        }
    }
}