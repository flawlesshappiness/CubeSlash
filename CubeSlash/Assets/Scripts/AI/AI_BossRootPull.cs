using System.Collections;
using UnityEngine;

public class AI_BossRootPull : BossAI
{
    [SerializeField] private Projectile prefab_projectile;
    [SerializeField] private RootPullVine _vine;
    [SerializeField] private Transform _pivot_walls_rotation;
    [SerializeField] private Transform[] _pivot_walls;
    [SerializeField] private Obstacle[] _walls;

    private BossRootBody root_body;
    private Vector3 destination;
    private bool attached;
    private bool can_attach = true;

    private int hits_taken;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);

        root_body = enemy.Body.GetComponent<BossRootBody>();
        Body.OnDudKilled += dud => OnDudKilled();

        _pivot_walls_rotation.gameObject.SetActive(false);
    }

    private void Update()
    {
        VineUpdate();
    }

    private void FixedUpdate()
    {
        MoveTowardsPlayer();
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
        MoveTowards(destination);
        TurnTowards(destination);
    }

    private void VineUpdate()
    {
        var width = CameraController.Instance.Width;
        var max_dist = width * 0.4f;
        var dist = DistanceToPlayer();

        if (dist > max_dist)
        {
            AttachVine();
        }
        else
        {
            UnattachVine();
        }
    }

    private void AttachVine()
    {
        if (attached) return;
        if (!can_attach) return;
        attached = true;
        _vine.Attach();
        _vine.AnimateToTarget();

        var sfx = SoundController.Instance.Play(SoundEffectType.sfx_enemy_root);
        sfx.SetPitch(-1);
    }

    private void UnattachVine()
    {
        if (!attached) return;
        attached = false;
        _vine.AnimateFromTarget();
        _vine.Unattach();
    }

    private void OnDudKilled()
    {
        if (Self.IsDead) return;

        hits_taken++;

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            can_attach = false;
            UnattachVine();

            var time = Random.Range(2f, 3f);
            HideDuds();
            yield return new WaitForSeconds(time);
            TeleportHide();
            yield return new WaitForSeconds(3f);
            TeleportAppear();
            yield return new WaitForSeconds(1f);
            can_attach = true;
            yield return new WaitForSeconds(4f);

            ShowDuds();
        }
    }

    private void TeleportHide()
    {
        SoundController.Instance.Play(SoundEffectType.sfx_enemy_bone_teleport_disappear);
        PlayTeleportPS();
        Self.Body.gameObject.SetActive(false);

        ShootProjectiles();
    }

    private void TeleportAppear()
    {
        var dir = Random.insideUnitCircle.normalized.ToVector3();
        var width = CameraController.Instance.Width;
        Self.transform.position = Player.Instance.transform.position + dir * (width * 0.5f);

        SoundController.Instance.Play(SoundEffectType.sfx_enemy_bone_teleport_appear);
        PlayTeleportPS();
        ShootProjectiles();

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            yield return new WaitForSeconds(0.2f);
            Self.Body.gameObject.SetActive(true);
        }
    }

    private void PlayTeleportPS()
    {
        root_body.ps_teleport.Duplicate()
            .Parent(GameController.Instance.world)
            .Scale(root_body.ps_teleport.transform.lossyScale)
            .Play()
            .Destroy(5f);
    }

    private void ShootProjectiles()
    {
        var count = 6 + hits_taken * 2;
        var points = CircleHelper.Points(1, 10);
        foreach (var point in points)
        {
            var dir = Self.transform.rotation * point;
            Shoot(dir);
        }
    }

    private void Shoot(Vector3 dir)
    {
        var speed = 3;
        var p = ProjectileController.Instance.CreateProjectile(prefab_projectile);
        p.transform.position = Position;
        p.Rigidbody.velocity = dir.normalized * speed;
        p.SetDirection(dir);
        p.Lifetime = 999f;
        p.Piercing = -1;
    }
}