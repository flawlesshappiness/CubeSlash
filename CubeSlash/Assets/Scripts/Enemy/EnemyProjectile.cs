using System.Collections;
using UnityEngine;
using Flawliz.Lerp;

public class EnemyProjectile : Projectile
{
    [SerializeField] private SpriteRenderer spr;
    protected override void OnStart()
    {
        base.OnStart();

        onHit += c =>
        {
            var player = c.GetComponentInParent<Player>();
            if(player != null)
            {
                player.Damage(transform.position);
            }
        };
    }

    IEnumerator AnimateBounceLoopCr()
    {
        var start = Vector3.one;
        var end = Vector3.one * 1.5f;
        while (true)
        {
            yield return Lerp.LocalScale(spr.transform, 0.25f, start, end);
            yield return Lerp.LocalScale(spr.transform, 0.25f, end, start);
        }
    }
}