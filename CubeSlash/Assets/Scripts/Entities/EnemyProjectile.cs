using System.Collections;
using UnityEngine;

public class EnemyProjectile : Projectile
{
    [SerializeField] private SpriteRenderer spr;
    protected override void OnStart()
    {
        base.OnStart();
        Lerp.Scale(spr.transform, 0.25f, Vector3.one, Vector3.one * 1.5f)
            .Loop().Oscillate();

        OnHitPlayer += player => player.Damage(1, transform.position);
    }
}