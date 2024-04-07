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
    [SerializeField] private ColorPaletteValue color_beam;

    private static readonly int[] HITPOINTS = new int[] { 6, 8, 10 };
    private const float RADIUS = 20;
    private const float RADIUS_PER_INDEX = 3;
    private const float RADIUS_MAX = 35;
    private const float SIZE_WALL = 5;

    private int duds_to_kill, duds_max;
    private int hits_taken;
    private string prev_attack;

    private List<Arena> arenas = new List<Arena>();

    private Coroutine cr_attack;
    private Coroutine cr_main;

    private float T_HitsTaken { get { return Mathf.Clamp01((float)hits_taken / (duds_max - 1)); } }

    private class Arena
    {
        public List<MawWall> walls = new List<MawWall>();
        public Transform pivot;
    }

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);

        var i_diff = DifficultyController.Instance.DifficultyIndex;
        duds_max = HITPOINTS[Mathf.Clamp(i_diff, 0, HITPOINTS.Length - 1)];
        duds_to_kill = duds_max;

        cr_main = StartCoroutine(MainCr());
    }

    IEnumerator MainCr()
    {
        BackgroundController.Instance.ClearObjects();

        yield return new WaitForSeconds(BackgroundController.OBJECT_FADE_TIME);

        // arena
        CreateArenas();

        // init
        yield return AnimateAppear();

        CreateDud();

        // Attack loop
        while (true)
        {
            Attack_Random();

            var cooldown = Mathf.Lerp(6f, 4f, T_HitsTaken);
            yield return new WaitForSeconds(cooldown);
        }
    }

    private void StopCurrentAttack()
    {
        if (cr_attack != null)
        {
            StopCoroutine(cr_attack);
        }
    }

    private void Attack_Random()
    {
        var r = new WeightedRandom<(string, System.Action)>();
        AddElement(nameof(Attack_Projectiles), 1, () => Attack_Projectiles());
        AddElement(nameof(Attack_Beam), 1, () => Attack_Beam());
        AddElement(nameof(Attack_Beams), 0.5f, () => Attack_Beams());
        var tuple = r.Random();
        tuple.Item2?.Invoke();
        prev_attack = tuple.Item1;

        void AddElement(string name, float weight, System.Action action)
        {
            if (name == prev_attack) return;
            r.AddElement((name, action), weight);
        }
    }

    private void Attack_Projectiles()
    {
        SoundController.Instance.Play(SoundEffectType.sfx_enemy_maw_attack);

        cr_attack = StartCoroutine(Cr());
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
        var t = T_HitsTaken;
        var center = arenas[0].pivot.position;
        var dir_to_player = Player.Instance.transform.position - center;
        var origin = center + dir_to_player.normalized * CameraController.Instance.Width * 2;
        var direction = -dir_to_player.normalized;
        var width = Mathf.Lerp(25f, 35f, t);
        cr_attack = StartCoroutine(ShootBeamCr(origin, direction, width, 4f));
    }

    private void Attack_Beams()
    {
        // Multi-beam attack
        cr_attack = StartCoroutine(Cr());
        IEnumerator Cr()
        {
            var t = T_HitsTaken;
            var width = 5f;
            var beam_delay_max = Mathf.Lerp(2.2f, 1.8f, t);
            var beam_delay_min = Mathf.Lerp(1.4f, 1f, t);
            var count_min = (int)Mathf.Lerp(4, 8, t);
            var count_max = (int)Mathf.Lerp(8, 12, t);
            var count = Random.Range(count_min, count_max);
            for (int i = 0; i < count; i++)
            {
                var t_count = Mathf.Clamp01((float)i / (count - 1));
                var delay = Mathf.Lerp(beam_delay_max, beam_delay_min, t_count);
                var delay_next = delay * 0.5f;
                var angle = Random.Range(0f, 360f);
                var direction = Quaternion.AngleAxis(angle, Vector3.forward) * Vector3.up;
                var origin = Player.Instance.transform.position + direction * CameraController.Instance.Width * 2;
                StartCoroutine(ShootBeamCr(origin, -direction, width, delay));
                yield return new WaitForSeconds(delay_next);
            }
        }
    }

    private IEnumerator ShootBeamCr(Vector3 origin, Vector3 direction, float width, float delay)
    {
        var angle = Vector3.SignedAngle(Vector3.up, direction, Vector3.forward);
        var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        var beam = ChargeBeam.Create();
        var length = CameraController.Instance.Width * 8;

        beam.transform.parent = GameController.Instance.world;
        beam.transform.position = origin;
        beam.transform.rotation = rotation;
        beam.SetColor(color_beam.GetColor());
        beam.SetWidth(width);
        beam.SetLength(length);
        beam.UpdateVisual();
        beam.AnimateShowPreview(true);

        var sfx_charge_start = SoundController.Instance.Play(SoundEffectType.sfx_charge_start)
            .SetPitch(-1)
            .StopWith(gameObject);

        yield return new WaitForSeconds(delay);

        sfx_charge_start.Stop();
        SoundController.Instance.Play(SoundEffectType.sfx_charge_shoot);

        Physics2D.CircleCastAll(origin, width * 0.25f, direction, length)
                .Select(hit => hit.collider.GetComponentInParent<Player>())
                .Distinct()
                .Where(p => p != null && !p.IsDead)
                .ToList().ForEach(p =>
                {
                    p.Damage(origin);
                });

        Self.Knockback(-direction * 200f, true, false);

        yield return beam.AnimateFire();

        Destroy(beam.gameObject);
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
            arena.pivot = arena.walls[0].transform.parent;
            AnimateBreathing(arena.pivot, i * 0.5f);
            AnimateSwaying(arena.pivot, i * 0.5f);

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
        dud.AnimateIdle();
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
            Player.Instance.InvincibilityLock.AddLock(nameof(AI_BossMaw));
            StopCoroutine(cr_main);
            StopCurrentAttack();
            EnemyController.Instance.EnemySpawnEnabled = false;
            EnemyController.Instance.KillActiveEnemies(new List<Enemy> { Self });
            ProjectileController.Instance.ClearProjectiles();
            yield return AnimateDestroyWalls();
            Player.Instance.InvincibilityLock.RemoveLock(nameof(AI_BossMaw));
            Self?.Kill();
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