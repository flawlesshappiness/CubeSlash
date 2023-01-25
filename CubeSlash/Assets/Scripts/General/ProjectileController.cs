using System.Collections.Generic;
using System.Drawing;
using UnityEngine;

public class ProjectileController : Singleton
{
    public static ProjectileController Instance { get { return Instance<ProjectileController>(); } }

    private List<Projectile> active_projectiles = new List<Projectile>();

    public T CreateProjectile<T>(T projectile) where T : Projectile
    {
        var p = Instantiate(projectile);
        active_projectiles.Add(p);
        p.onDeath += () => RemoveProjectile(p);
        return p;
    }

    public void RemoveProjectile(Projectile p)
    {
        active_projectiles.Remove(p);
    }

    public void ClearProjectiles()
    {
        foreach(var p in active_projectiles)
        {
            Destroy(p.gameObject);
        }
        active_projectiles.Clear();
    }

    public class PlayerShootInfo
    {
        public Projectile prefab;
        public Vector3 position_start;
        public Vector3 velocity;
        public System.Action<Projectile, IKillable> onHit;
    }

    public Projectile ShootPlayerProjectile(PlayerShootInfo info)
    {
        var p = CreateProjectile(info.prefab);
        p.transform.position = info.position_start;
        p.Rigidbody.velocity = info.velocity;
        p.SetDirection(info.velocity);

        p.onHit += c =>
        {
            var k = c.GetComponentInParent<IKillable>();
            if (k != null)
            {
                info.onHit?.Invoke(p, k);
                if (k.CanKill())
                {
                    Player.Instance.KillEnemy(k);
                }
                if (!p.Piercing) p.Kill();
            }
        };

        return p;
    }
}