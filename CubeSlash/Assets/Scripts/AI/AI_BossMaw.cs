using Flawliz.Lerp;
using PathCreation;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AI_BossMaw : BossAI
{
    [SerializeField] private MawWall template_wall;

    private const float RADIUS = 20;
    private const float RADIUS_PER_INDEX = 3;
    private const float RADIUS_MAX = 35;
    private const float SIZE_WALL = 5;

    private List<Arena> arenas = new List<Arena>();

    private class Arena
    {
        public List<MawWall> walls = new List<MawWall>();
    }

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);

        CreateArenas();
        AnimateAppear();

        EnemyController.Instance.EnemySpawnEnabled = false;
    }

    private void AnimateAppear()
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            foreach (var arena in arenas)
            {
                foreach (var wall in arena.walls)
                {
                    wall.AnimateAppear();
                }

                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    private void CreateArenas()
    {
        var size_muls = new float[4] { 1f, 0.75f, 0.5f, 0.25f };
        for (int i = 0; i < size_muls.Length; i++)
        {
            var arena = new Arena();
            arenas.Add(arena);

            arena.walls = CreateArena(template_wall, size_muls[i]);
            var pivot = arena.walls[0].transform.parent;
            AnimateBreathing(pivot, i * 0.5f);
            AnimateSwaying(pivot, i * 0.5f);

            arena.walls.ForEach(wall =>
            {
                wall.SetLayer(i);
                wall.Hide();
            });
        }
    }

    private List<MawWall> CreateArena(MawWall template, float size_mul = 1f)
    {
        var walls = new List<MawWall>();

        var area_number = AreaController.Instance.AreaIndex + 1;
        var radius = Mathf.Clamp(RADIUS + RADIUS_PER_INDEX * area_number, 0, RADIUS_MAX) * size_mul;
        var points = CircleHelper.Points(radius, 10);
        var bezier = new BezierPath(points, true, PathSpace.xy);
        var path = new VertexPath(bezier, GameController.Instance.world);
        var offset = Player.Instance.transform.position;

        var parent = new GameObject("Maw Walls");
        parent.transform.parent = GameController.Instance.world;
        ObjectController.Instance.Add(parent);
        parent.transform.position = offset;

        var i_wall = 0;
        var wall_length = SIZE_WALL * 0.95f * size_mul;
        var wall_length_half = wall_length * 0.5f;
        var distance = 0f;
        while (distance < path.length)
        {
            var d = distance + wall_length_half;
            var point = path.GetPointAtDistance(d);
            var direction = path.GetDirectionAtDistance(d);
            var angle = Vector3.SignedAngle(Vector3.up, direction, Vector3.forward);
            var rotation = Quaternion.AngleAxis(angle, Vector3.forward);

            var wall = Instantiate(template, parent.transform);
            //wall.SetSortingOrder(i_wall);
            wall.transform.position = point + offset;
            wall.transform.rotation = rotation;
            walls.Add(wall);
            //wall.AnimateAppear();
            ObjectController.Instance.Add(wall.gameObject);

            distance += wall_length;
            i_wall++;
        }

        return walls;
    }

    private void AnimateBreathing(Transform pivot, float delay)
    {
        var size_delta = 0.05f;
        var curve = EasingCurves.EaseInOutQuad;
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(delay);
            while (true)
            {
                yield return LerpEnumerator.LocalScale(pivot, 4f, Vector3.one * (1f - size_delta)).Curve(curve);
                yield return LerpEnumerator.LocalScale(pivot, 2f, Vector3.one * (1f + size_delta)).Curve(curve);
            }
        }
    }

    private void AnimateSwaying(Transform pivot, float delay)
    {
        var curve = EasingCurves.EaseInOutQuad;
        var delta = 5;
        var min = Quaternion.AngleAxis(-delta, Vector3.forward);
        var max = Quaternion.AngleAxis(delta, Vector3.forward);
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(delay);
            while (true)
            {
                yield return LerpEnumerator.Rotation(pivot, 4f, min).Curve(curve);
                yield return LerpEnumerator.Rotation(pivot, 4f, max).Curve(curve);
            }
        }
    }
}