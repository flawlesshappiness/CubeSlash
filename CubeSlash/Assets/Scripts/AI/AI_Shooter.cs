using System.Collections;
using UnityEngine;

public class AI_Shooter : EnemyAI
{
    [SerializeField] private float dist_min_shoot;
    [SerializeField] private float cooldown_shoot_min;
    [SerializeField] private float cooldown_shoot_max;

    [Header("PROJECTILE")]
    [SerializeField] private Projectile prefab_projectile;
    [SerializeField] private float velocity_projectile;

    private Vector3 pos_player_prev;
    private bool shooting;

    private TripleyeBody EyeBody => Body as TripleyeBody;
    private bool IsInRange => DistanceToPlayer() < dist_min_shoot;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);
        StartShootCoroutine();
    }

    private void FixedUpdate()
    {
        pos_player_prev = IsPlayerAlive() ? Player.Instance.transform.position : pos_player_prev;
        MoveUpdate();
    }

    private void MoveUpdate()
    {
        var dist = DistanceToPlayer();
        var dist_max_shoot = CameraController.Instance.Width * dist_min_shoot;
        if (IsInRange || shooting)
        {
            MoveToStop(0.5f);
            TurnTowards(pos_player_prev);
        }
        else
        {
            MoveTowards(pos_player_prev);
            TurnTowards(pos_player_prev);
        }
    }

    private void StartShootCoroutine()
    {
        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            while (true)
            {
                while (!IsInRange)
                {
                    yield return null;
                }

                shooting = true;
                yield return new WaitForSeconds(Random.Range(0f, 8f)); // Wait for a brief random amount of time
                TelegraphAll();
                yield return new WaitForSeconds(0.5f);
                ShootAll();
                shooting = false;
                yield return new WaitForSeconds(Random.Range(cooldown_shoot_min, cooldown_shoot_max)); // Cooldown
            }
        }
    }

    private void ShootAll()
    {
        foreach (var t in EyeBody.EyeTransforms)
        {
            Shoot(t);
        }
    }

    private void Shoot(Transform t)
    {
        var dir = t.up;
        var p = ProjectileController.Instance.CreateProjectile(prefab_projectile);
        p.transform.position = t.position;
        p.Rigidbody.velocity = dir.normalized * velocity_projectile;
        p.SetDirection(dir);
        p.Lifetime = 999f;
        p.Piercing = -1;

        Self.Knockback(-dir.normalized * 200, true, false);

        SoundController.Instance.Play(SoundEffectType.sfx_enemy_shoot).SetPitch(5);
    }

    private void TelegraphAll()
    {
        foreach (var t in EyeBody.EyeTransforms)
        {
            TelegraphShootShort(t);
        }
    }

    private void TelegraphShootShort(Transform t)
    {
        Resources.Load<ParticleSystem>("particles/ps_telegraph_shoot_small")
            .Duplicate()
            .Parent(transform)
            .Scale(Vector3.one)
            .Position(t.position)
            .Rotation(t.rotation)
            .Destroy(3f);
    }
}