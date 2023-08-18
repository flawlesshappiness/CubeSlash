using UnityEngine;

public class AbilityBoomerang : Ability
{
    [Header("BOOMERANG")]
    [SerializeField] private Projectile prefab_projectile;

    private float Cooldown { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.split_cooldown).ModifiedValue.float_value; } }
    private float CountProjectile { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.split_cooldown).ModifiedValue.int_value; } } // SPLIT
    private float SizeProjectile { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.split_size).ModifiedValue.float_value; } }
    private float Distance { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.split_size).ModifiedValue.float_value; } }
    private float SuspendDuration { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.split_size).ModifiedValue.float_value; } }

    private bool ChainLightning { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.split_chain).ModifiedValue.bool_value; } } // CHAIN
    private bool ProjectileExplode { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.split_chain).ModifiedValue.bool_value; } } // EXPLODE
    private bool FragmentProjectile { get { return GameAttributeController.Instance.GetAttribute(GameAttributeType.split_chain).ModifiedValue.bool_value; } } // MINES

    public override float GetBaseCooldown() => Cooldown;

    public override void Trigger()
    {
        base.Trigger();
    }
}