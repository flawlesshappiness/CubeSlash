using UnityEngine;

[CreateAssetMenu(fileName = nameof(PlayerBodySettings), menuName = "Game/" + nameof(PlayerBodySettings), order = 1)]
public class PlayerBodySettings : ScriptableObject
{
    [Header("SETTINGS")]
    public int health;
    public int armor;
    public float body_size;
    public float mass;
    public float linear_velocity; 
    public float linear_acceleration; 
    public float linear_drag; 

    [Header("BODY")]
    public PlayerBody body;
    public Sprite body_sprite;

    [Header("ABILITY")]
    public Ability.Type ability_type;

    [Header("SHOP")]
    public InternalShopProduct shop_product;

    public bool IsUnlocked()
    {
        return shop_product == null || InternalShopController.Instance.IsPurchased(shop_product);
    }

    public AbilityInfo GetAbilityInfo()
    {
        var ability = Database.Load<AbilityDatabase>().GetAbility(ability_type);
        return ability.Info;
    }
}