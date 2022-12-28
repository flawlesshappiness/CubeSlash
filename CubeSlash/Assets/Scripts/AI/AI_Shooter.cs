using System.Collections;
using UnityEngine;

public class AI_Shooter : EntityAI
{
    [SerializeField] private float dist_max_mul_shoot;
    [SerializeField] private float cooldown_shoot;

    [Header("PROJECTILE")]
    [SerializeField] private Projectile prefab_projectile;
    [SerializeField] private float velocity_projectile;

    private Transform t_eye;

    private Vector3 pos_player_prev;
    private float cd_shoot;

    public override void Initialize(Enemy enemy)
    {
        base.Initialize(enemy);
        t_eye = Self.Body.GetTransform("eye");
    }

    private void FixedUpdate()
    {
        pos_player_prev = IsPlayerAlive() ? Player.Instance.transform.position : pos_player_prev;
        MoveUpdate();
        ShootUpdate();
    }

    private void MoveUpdate()
    {
        var dist = DistanceToPlayer();
        var dist_max_shoot = CameraController.Instance.Width * dist_max_mul_shoot;
        if(dist < dist_max_shoot)
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

    private void ShootUpdate()
    {
        var dist = DistanceToPlayer();
        var dist_max = CameraController.Instance.Width * dist_max_mul_shoot;
        if (Time.time < cd_shoot) return;
        if (dist < dist_max)
        {
            StartCoroutine(Cr());
        }

        IEnumerator Cr()
        {
            cd_shoot = Time.time + 999;
            TelegraphShootShort(0);
            yield return new WaitForSeconds(0.5f);
            Shoot();
            cd_shoot = Time.time + cooldown_shoot;
        }
    }

    private void Shoot()
    {
        var dir = DirectionToPlayer();
        var p = Instantiate(prefab_projectile.gameObject).GetComponent<Projectile>();
        p.transform.position = Position;
        p.Rigidbody.velocity = dir.normalized * velocity_projectile;
        p.SetDirection(dir);
        p.Lifetime = 999f;

        Self.Rigidbody.AddForce(-dir * 50 * Self.Rigidbody.mass);
    }

    private void TelegraphShootShort(float angle)
    {
        Resources.Load<ParticleSystem>("particles/ps_telegraph_shoot_small")
            .Duplicate()
            .Parent(transform)
            .Scale(Vector3.one)
            .Position(t_eye.position)
            .Rotation(transform.rotation * Quaternion.AngleAxis(angle, Vector3.forward))
            .Destroy(3f);
    }
}