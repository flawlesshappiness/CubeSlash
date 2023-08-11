using PathCreation;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AI_BossPlant : BossAI
{
    [SerializeField] private PlantWall template_wall;
    [SerializeField] private PlantPillar template_pillar;
    [SerializeField] private HealthDud template_dud;

    private List<PlantWall> walls = new List<PlantWall>();
    private List<PlantPillar> pillars = new List<PlantPillar>();

    private const int EXP_PER_WALL = 2;
    private static readonly int[] HITPOINTS = new int[] { 4, 5, 6 };
    private const float SIZE_WALL = 5;
    private const float RADIUS = 20;
    private const float RADIUS_PER_INDEX = 3;
    private const float RADIUS_MAX = 35;
    private const float COOLDOWN_PILLAR_MAX = 8f;
    private const float COOLDOWN_PILLAR_MIN = 3f;
    private const float PILLAR_SIZE_MIN = 5;
    private const float PILLAR_SIZE_MAX = 8;
    private const float PILLAR_LIFETIME_MIN = 5f;
    private const float PILLAR_LIFETIME_MAX = 10f;

    private int duds_max;
    private int duds_to_kill;
    private float time_until_pillar;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);
        var i_diff = DifficultyController.Instance.DifficultyIndex;
        duds_max = HITPOINTS[Mathf.Clamp(i_diff, 0, HITPOINTS.Length - 1)];
        duds_to_kill = duds_max;
        Self.Body.gameObject.SetActive(false);
        Self.transform.position = Player.Instance.transform.position;

        CreateArena();

        if (DifficultyController.Instance.DifficultyValue > 0.1f)
        {
            CreateDud();
        }
        else
        {
            CreateDuds();
        }
    }

    private void CreateArena()
    {
        var area_number = AreaController.Instance.AreaIndex + 1;
        var radius = Mathf.Clamp(RADIUS + RADIUS_PER_INDEX * area_number, 0, RADIUS_MAX);
        var points = CircleHelper.Points(radius, 10);
        var bezier = new BezierPath(points, true, PathSpace.xy);
        var path = new VertexPath(bezier, GameController.Instance.world);
        var offset = Player.Instance.transform.position;

        var i_wall = 0;
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
            wall.SetSortingOrder(i_wall);
            wall.transform.position = point + offset;
            wall.transform.rotation = rotation;
            walls.Add(wall);
            wall.AnimateAppear();
            ObjectController.Instance.Add(wall.gameObject);

            distance += wall_length;
            i_wall++;
        }
    }

    private void CreateDuds()
    {
        var dud_walls = walls.TakeRandom(duds_max);
        foreach (var wall in dud_walls)
        {
            CreateDud(wall);
        }
    }

    private void CreateDud()
    {
        var player_position = Player.Instance.transform.position;
        var min_distance = RADIUS * 0.9f;
        var wall = walls.Where(w => Vector3.Distance(w.transform.position, player_position) > min_distance).ToList().Random();
        CreateDud(wall);
    }

    private void CreateDud(PlantWall wall)
    {
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
        ObjectController.Instance.Add(pillar.gameObject);

        var t = GetHealthPercentage();
        var scale = Vector3.one * Mathf.Lerp(PILLAR_SIZE_MIN, PILLAR_SIZE_MAX, t);
        pillar.transform.localScale = scale;

        var lifetime = Mathf.Lerp(PILLAR_LIFETIME_MIN, PILLAR_LIFETIME_MAX, t);
        pillar.AnimateAppear(3f, lifetime);
    }

    private void OnDudKilled()
    {
        duds_to_kill--;

        if (duds_to_kill <= 0)
        {
            End();
            return;
        }

        if (DifficultyController.Instance.DifficultyValue > 0.1f)
        {
            CreateDud();
        }
    }

    private void End()
    {
        CleanupAndSpawnExp();
        Self.Kill();
    }

    private void CleanupAndSpawnExp()
    {
        foreach (var wall in walls)
        {
            // Spawn exp
            for (int i = 0; i < EXP_PER_WALL; i++)
            {
                var rnd = Random.insideUnitCircle * SIZE_WALL * 0.25f;
                var pos = wall.transform.position + rnd.ToVector3();
                var exp = ItemController.Instance.SpawnPlant(pos);
                exp.AnimateCollect();
            }

            wall.Kill();
        }
        walls.Clear();

        foreach (var pillar in pillars)
        {
            if (pillar != null)
            {
                pillar.Kill();
            }
        }
        pillars.Clear();
    }

    private float GetHealthPercentage()
    {
        return 1f - ((float)duds_to_kill / duds_max);
    }

    private void Update()
    {
        if (Player.Instance.IsDead) return;

        var t = GetHealthPercentage();
        if (Time.time > time_until_pillar)
        {
            CreatePillar();
            var cooldown = Mathf.Lerp(COOLDOWN_PILLAR_MAX, COOLDOWN_PILLAR_MIN, t);
            time_until_pillar = Time.time + cooldown;
        }
    }
}