using Flawliz.Lerp;
using PathCreation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AI_BossMaw : BossAI
{
    [SerializeField] private HealthDud template_dud;
    [SerializeField] private MawWall template_wall;
    [SerializeField] private Projectile template_projectile;

    private static readonly int[] HITPOINTS = new int[] { 4, 5, 6 };
    private const float RADIUS = 20;
    private const float RADIUS_PER_INDEX = 3;
    private const float RADIUS_MAX = 35;
    private const float SIZE_WALL = 5;

    private int duds_to_kill, duds_max;
    private int hits_taken;

    private List<Arena> arenas = new List<Arena>();

    private float T_HitsTaken { get { return Mathf.Clamp01((float)hits_taken / (duds_max - 1)); } }

    private class Arena
    {
        public List<MawWall> walls = new List<MawWall>();
    }

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);

        CreateArenas();

        EnemyController.Instance.EnemySpawnEnabled = false;

        var i_diff = DifficultyController.Instance.DifficultyIndex;
        duds_max = HITPOINTS[Mathf.Clamp(i_diff, 0, HITPOINTS.Length - 1)];
        duds_to_kill = duds_max;

        StartCoroutine(MainCr());
    }

    IEnumerator MainCr()
    {
        // init
        yield return AnimateAppear();

        CreateDud();

        // attack loop
        while (true)
        {
            Attack_Random();

            var cooldown = Mathf.Lerp(10f, 5f, T_HitsTaken);
            yield return new WaitForSeconds(cooldown);
        }
    }

    private void Attack_Random()
    {
        var r = new WeightedRandom<System.Action>();
        r.AddElement(() => Attack_EnemyGroup(), 1);
        r.AddElement(() => Attack_Projectiles(), 1);
        var a = r.Random();
        a?.Invoke();
    }

    private void Attack_EnemyGroup()
    {
        var is_fixed_position = Random.Range(0, 2) == 0;
        var fixed_position = CameraController.Instance.GetPositionOutsideCamera();
        var area_enemy_info = EnemyController.Instance.GetEnemiesUnlocked().Random();
        var count = Random.Range(6, 10);
        for (int i = 0; i < count; i++)
        {
            var position = GetPosition() + Random.insideUnitCircle.ToVector3().normalized;
            var e = EnemyController.Instance.SpawnEnemy(area_enemy_info.enemy, position);
            e.OnDeath += () => EnemyController.Instance.EnemyDeathSpawnMeat(e);
        }

        Vector3 GetPosition() => is_fixed_position ? fixed_position : CameraController.Instance.GetPositionOutsideCamera();
    }

    private void Attack_Projectiles()
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            var count = Random.Range(5, 10);
            var has_delay = Random.Range(0, 2) == 0;
            var delay = 0.2f;
            for (int i = 0; i < count; i++)
            {
                var position = CameraController.Instance.GetPositionOutsideCamera();
                var dir = Player.Instance.transform.position - position;
                var p = ProjectileController.Instance.CreateProjectile(template_projectile);
                p.transform.localScale = Vector3.one * 2;
                p.transform.position = position;
                p.Lifetime = 20f;
                p.Rigidbody.velocity = dir.normalized * 5;
                p.Piercing = -1;

                if (has_delay)
                {
                    yield return new WaitForSeconds(delay);
                }
            }
        }
    }

    private void Attack_FollowingProjectile()
    {
        // Spawn a projectile that follows the player around for a while
    }

    private void Attack_Beam()
    {
        // Beam attack
    }

    private Coroutine AnimateAppear()
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            var crs = new List<Coroutine>();
            foreach (var arena in arenas)
            {
                foreach (var wall in arena.walls)
                {
                    var cr = wall.AnimateAppear();
                    crs.Add(cr);
                }

                yield return new WaitForSeconds(0.5f);
            }

            foreach (var cr in crs)
            {
                yield return cr;
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

    private void CreateDud()
    {
        var player_position = Player.Instance.transform.position;
        var min_distance = RADIUS * 0.9f;
        var arena = arenas[0];
        var wall = arena.walls.Where(w => Vector3.Distance(w.transform.position, player_position) > min_distance).ToList().Random();
        CreateDud(wall);
    }

    private void CreateDud(MawWall wall)
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

    private void OnDudKilled()
    {
        duds_to_kill--;
        hits_taken++;

        if (duds_to_kill <= 0)
        {
            End();
            return;
        }

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(Random.Range(5f, 10f));
            CreateDud();
        }
    }

    private void End()
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            EnemyController.Instance.KillActiveEnemies(new List<Enemy> { Self });
            ProjectileController.Instance.ClearProjectiles();
            yield return AnimateDestroyWalls();
            Self.Kill();
        }
    }

    private Coroutine AnimateDestroyWalls()
    {
        return StartCoroutine(Cr());
        IEnumerator Cr()
        {
            var rev_arenas = arenas.ToList();
            rev_arenas.Reverse();

            foreach (var arena in rev_arenas)
            {
                foreach (var wall in arena.walls)
                {
                    wall.Kill();
                    yield return new WaitForSeconds(0.01f);
                }
            }
        }
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