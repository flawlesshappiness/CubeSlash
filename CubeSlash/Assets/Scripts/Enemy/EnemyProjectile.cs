using UnityEngine;

public class EnemyProjectile : Projectile
{
    [SerializeField] private SpriteRenderer spr;

    public bool rotates;

    private void Start()
    {
        if (rotates)
        {
            AnimateRotation();
        }
    }
}