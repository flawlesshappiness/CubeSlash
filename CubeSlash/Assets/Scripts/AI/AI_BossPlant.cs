using UnityEngine;
using PathCreation;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class AI_BossPlant : EntityAI
{
    [SerializeField] private PlantWall template_wall;
    [SerializeField] private HealthDud template_dud;

    private List<PlantWall> walls = new List<PlantWall>();

    private const int HITPOINTS = 5;
    private const float RADIUS = 25;

    private int duds_to_kill;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);
        duds_to_kill = HITPOINTS;
        Self.Body.gameObject.SetActive(false);
        CreateArena();
        CreateDud();
    }

    private void CreateArena()
    {
        var points = CircleHelper.Points(RADIUS, 10);
        var bezier = new BezierPath(points, true, PathSpace.xy);
        var path = new VertexPath(bezier, GameController.Instance.world);
        var offset = Player.Instance.transform.position;

        var wall_length = 5f * 0.95f;
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
        dud.transform.position = wall.GetDudPosition();
        dud.transform.rotation = wall.GetDudRotation();
        dud.transform.localScale = wall.GetDudScale();
        dud.OnKilled += OnDudKilled;
        dud.SetDudActive(false, false);
        dud.SetDudActive(true, true);
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
            wall.Destroy();
        }

        Self.Kill();
    }

    private void Update()
    {
        // Launch attacks at player
    }
}