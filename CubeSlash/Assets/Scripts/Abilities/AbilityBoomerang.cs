using UnityEngine;

public class AbilityBoomerang : Ability
{
    [Header("BOOMERANG")]
    [SerializeField] private Projectile prefab_projectile;

    private float Cooldown { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.boomerang_cooldown).ModifiedValue.float_value; } }
    private int CountProjectile { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.boomerang_count).ModifiedValue.int_value; } } // SPLIT
    private float SizeProjectile { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.boomerang_size).ModifiedValue.float_value; } }
    private float Distance { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.boomerang_distance).ModifiedValue.float_value; } }
    private float CatchCooldown { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.boomerang_catch_cooldown).ModifiedValue.float_value; } }

    private bool ChainLightning { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.boomerang_chain).ModifiedValue.bool_value; } } // CHAIN
    private bool ProjectileExplode { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.boomerang_explode).ModifiedValue.bool_value; } } // EXPLODE
    private int FragmentProjectile { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.boomerang_fragment).ModifiedValue.int_value; } } // MINES

    private const float PROJECTILE_SPEED = 15f;
    private const float PROJECTILE_LIFETIME = 999f;
    private const float FORCE_SELF = 300f;
    private const float FORCE_SELF_SIZE = 100f;

    public override float GetBaseCooldown() => Cooldown;

    public override void Trigger()
    {
        base.Trigger();

        TriggerShoot();
        StartCooldown();

        SoundController.Instance.Play(SoundEffectType.sfx_boomerang_shoot);
    }

    private void TriggerShoot()
    {
        var forward = Player.MoveDirection;

        if (CountProjectile > 1)
        {
            var angle_per = 360f / 8;
            var angle_max = angle_per * CountProjectile;
            var directions = AbilitySplit.GetSplitDirections(CountProjectile, angle_max, forward);
            foreach (var direction in directions)
            {
                var velocity = direction * PROJECTILE_SPEED;
                ShootProjectile(velocity);
            }
        }
        else
        {
            var velocity = forward * PROJECTILE_SPEED;
            ShootProjectile(velocity);
        }
    }

    public void ShootProjectile(Vector3 velocity)
    {
        var direction = velocity.normalized;
        var start_position = Player.transform.position;

        var p = ProjectileController.Instance.ShootPlayerProjectile(new ProjectileController.PlayerShootInfo
        {
            prefab = prefab_projectile,
            position_start = start_position,
            velocity = velocity,
            onKill = OnKill
        }) as BoomerangProjectile;

        p.StartPosition = start_position;
        p.Velocity = velocity;
        p.Distance = Distance;

        p.transform.localScale = Vector3.one * SizeProjectile;
        p.Piercing = -1;
        p.Lifetime = PROJECTILE_LIFETIME;
        p.onDestroy += () => OnDestroy(p);

        Player.Knockback(-direction * (FORCE_SELF + FORCE_SELF_SIZE * SizeProjectile), false, false);

        void OnDestroy(BoomerangProjectile p)
        {
            if (p.Caught)
            {
                AdjustCooldownFlat(CatchCooldown);
                SoundController.Instance.Play(SoundEffectType.sfx_boomerang_catch);
            }
        }

        void OnKill(Projectile p, IKillable k)
        {
            if (ProjectileExplode)
            {
                AbilityExplode.Explode(p.transform.position, 3f, 0);
            }
        }
    }
}