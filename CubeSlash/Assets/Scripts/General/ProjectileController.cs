using System;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : Singleton
{
    public static ProjectileController Instance { get { return Instance<ProjectileController>(); } }

    private List<Projectile> active_projectiles = new List<Projectile>();

    public Action OnClear;

    protected override void Initialize()
    {
        base.Initialize();
        GameController.Instance.onMainMenu += OnMainMenu;
    }

    private void OnMainMenu()
    {
        ClearProjectiles();
    }

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
        foreach (var p in active_projectiles)
        {
            if (p == null) continue;
            Destroy(p.gameObject);
        }
        active_projectiles.Clear();

        OnClear?.Invoke();
    }

    public class PlayerShootInfo
    {
        public Projectile prefab;
        public Vector3 position_start;
        public Vector3 velocity;
        public System.Action<Projectile, IKillable> onKill;
    }

    public Projectile ShootPlayerProjectile(PlayerShootInfo info)
    {
        var p = CreateProjectile(info.prefab);
        p.transform.position = info.position_start;
        p.Rigidbody.velocity = info.velocity;
        p.SetDirection(info.velocity);

        p.onHitEnemy += k =>
        {
            info.onKill?.Invoke(p, k);
        };

        return p;
    }
}