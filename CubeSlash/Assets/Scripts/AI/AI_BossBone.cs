using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AI_BossBone : EnemyAI
{
    [SerializeField] private PlantWall template_wall;
    [SerializeField] private EnemyProjectile template_projectile;
    [SerializeField] private Vector2 speed_rotate_min_max;

    private BossBoneBody body_bone;
    private Vector3 destination;
    private Vector3 wall_area_direction;
    private Vector3 wall_area_normal;
    private Vector3 wall_area_center;
    private int wall_arm_count = 0;

    private const int COUNT_ARM_MAX = 3;
    private const float WIDTH_AREA = 15f;
    private const float LENGTH_AREA = 150f;

    private List<PlantWall> walls = new List<PlantWall>();

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);
        body_bone = enemy.Body.GetComponent<BossBoneBody>();

        Self.EnemyBody.OnDudKilled += dud => OnDudKilled();
    }

    private void FixedUpdate()
    {
        MoveTowardsPlayer();
    }

    private void LateUpdate()
    {
        RotateBodyUpdate();
    }

    private void RotateBodyUpdate()
    {
        var duds = Self.EnemyBody.Duds;
        var duds_dead = duds.Where(d => d.Dead);
        var t_duds = (float)duds_dead.Count() / duds.Count();
        var speed = Mathf.Lerp(speed_rotate_min_max.x, speed_rotate_min_max.y, t_duds);
        var body = Self.Body;
        var angle = body.transform.eulerAngles.z + speed;
        var angle_mod = angle % 360f;
        var q = Quaternion.AngleAxis(angle_mod, Vector3.forward);
        body.transform.rotation = Quaternion.Lerp(body.transform.rotation, q, Time.deltaTime * 10f);
    }

    private void MoveTowardsPlayer()
    {
        if (IsPlayerAlive())
        {
            destination = PlayerPosition;
        }

        var dir = (destination - Position).normalized;
        var offset = dir * Self.Settings.size * 0.7f;
        var t = (Vector3.Distance(Position + offset, destination)) / (CameraController.Instance.Width * 0.5f);
        Self.AngularAcceleration = Mathf.Lerp(Self.Settings.angular_acceleration * 0.25f, Self.Settings.angular_acceleration, t);
        Self.AngularVelocity = Mathf.Lerp(Self.Settings.angular_velocity * 0.25f, Self.Settings.angular_velocity, t);
        MoveTowards(PlayerPosition);
    }

    private void OnDudKilled()
    {
        wall_arm_count = Mathf.Min(wall_arm_count + 1, COUNT_ARM_MAX);
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            var time = Random.Range(2f, 3f);
            HideDuds();
            DestroyWalls();
            yield return new WaitForSeconds(time);
            TeleportHide();
            yield return new WaitForSeconds(0.5f);
            CreateWallArea();
            yield return GetAttackCr();
            DestroyWalls();
            yield return new WaitForSeconds(1f);
            TeleportAppear();
            yield return new WaitForSeconds(1f);
            ShowDuds();
            CreateWallArms(wall_arm_count);
        }
    }

    private void TeleportHide()
    {
        SoundController.Instance.Play(SoundEffectType.sfx_enemy_bone_teleport_disappear);
        PlayTeleportPS();
        Self.Body.gameObject.SetActive(false);
    }

    private void TeleportAppear()
    {
        var dir = Random.insideUnitCircle.normalized.ToVector3();
        Self.transform.position = Player.Instance.transform.position + dir * 25f;

        SoundController.Instance.Play(SoundEffectType.sfx_enemy_bone_teleport_appear);
        PlayTeleportPS();
        StartCoroutine(Cr());

        IEnumerator Cr()
        {
            yield return new WaitForSeconds(0.2f);
            Self.Body.gameObject.SetActive(true);
        }
    }

    private void PlayTeleportPS()
    {
        body_bone.ps_teleport.Duplicate()
            .Parent(GameController.Instance.world)
            .Scale(body_bone.ps_teleport.transform.lossyScale)
            .Play()
            .Destroy(5f);
    }

    private void CreateWallArms(int count)
    {
        // Clear
        foreach(var wall in walls)
        {
            Destroy(wall.gameObject);
        }
        walls.Clear();

        // Create
        foreach(var pivot in body_bone.pivots_wall)
        {
            for (int i = 0; i < count; i++)
            {
                var wall = Instantiate(template_wall, pivot);
                wall.transform.localScale = new Vector3(0.5f, 0.5f);
                wall.transform.localRotation = Quaternion.identity;
                wall.SetSortingOrder(i);

                var size = wall.transform.localScale.x * 0.8f;
                var dir = Vector3.up * size;
                wall.transform.localPosition += dir * 0.5f + dir * i;
                walls.Add(wall);
                wall.AnimateAppear(0.25f);
            }
        }
    }

    private void DestroyWalls()
    {
        SoundController.Instance.Play(SoundEffectType.sfx_enemy_bone_wall_disappear);

        foreach (var wall in walls)
        {
            wall.Kill();
        }
        walls.Clear();
    }

    private void CreateWallArea()
    {
        SoundController.Instance.Play(SoundEffectType.sfx_enemy_bone_wall_appear);
        wall_area_center = Player.Instance.transform.position;
        wall_area_direction = Random.insideUnitCircle.normalized;
        wall_area_normal = Vector3.Cross(wall_area_direction, Vector3.forward);
        var start = wall_area_center - wall_area_direction * LENGTH_AREA * 0.5f;
        var start_left = start - wall_area_normal * WIDTH_AREA;
        var start_right = start + wall_area_normal * WIDTH_AREA;
        CreateWallLine(start_left, wall_area_direction, LENGTH_AREA);
        CreateWallLine(start_right, wall_area_direction, LENGTH_AREA);
    }

    private void CreateWallLine(Vector3 start, Vector3 dir, float length)
    {
        var angle = Vector3.SignedAngle(Vector3.up, dir, Vector3.forward);
        var rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        var dist = 0f;
        var i = 0;
        while(dist < length)
        {
            var wall = Instantiate(template_wall, GameController.Instance.world);
            var size = 6f;
            wall.transform.localScale = new Vector3(size, size);
            wall.transform.position = start + dir.normalized * size * i;
            wall.transform.rotation = rotation;
            wall.SetHidden();
            wall.StartCoroutine(AnimateAppearWallCr(wall, 0.02f * i));
            wall.SetSortingOrder(i);
            walls.Add(wall);
            dist += size;
            i++;
        }

        IEnumerator AnimateAppearWallCr(PlantWall wall, float delay)
        {
            yield return new WaitForSeconds(delay);
            wall.AnimateAppear(0.25f);
        }
    }

    private void ShootProjectile(Vector3 start, Vector3 dir)
    {
        var p = ProjectileController.Instance.CreateProjectile(template_projectile);
        p.transform.localScale = Vector3.one * 3f;
        p.transform.position = start;
        p.Lifetime = 10f;
        p.Rigidbody.velocity = dir;
        p.Piercing = true;
    }

    IEnumerator GetAttackCr()
    {
        var i = Random.Range(0, 2);
        return i switch
        {
            0 => AttackProjectiles1(),
            1 => AttackProjectiles2(),
        };
    }

    IEnumerator AttackProjectiles1()
    {
        var start = wall_area_center - wall_area_direction * LENGTH_AREA * 0.5f;
        var start_left = start - wall_area_normal * WIDTH_AREA * 0.8f;
        var start_right = start + wall_area_normal * WIDTH_AREA * 0.8f;
        var count = 5;
        for (int i = 0; i < count; i++)
        {
            var t = (float)i / (count - 1);
            var position = Vector3.Lerp(start_left, start_right, t);
            ShootProjectile(position, wall_area_direction * 15f);
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(5f);
    }

    IEnumerator AttackProjectiles2()
    {
        var start = wall_area_center - wall_area_direction * LENGTH_AREA * 0.5f;
        var end = wall_area_center + wall_area_direction * LENGTH_AREA * 0.5f;
        var count = 5;
        for (int i = 0; i < count; i++)
        {
            var toggle = i % 2 == 0;
            var mul = toggle ? 1 : -1;
            var origin = toggle ? start : end;
            var start_left = origin - wall_area_normal * WIDTH_AREA * 0.8f;
            var start_right = origin + wall_area_normal * WIDTH_AREA * 0.8f;
            var t = (float)i / (count - 1);
            var position = Vector3.Lerp(start_left, start_right, t);
            ShootProjectile(position, wall_area_direction * 15f * mul);
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(5f);
    }
}