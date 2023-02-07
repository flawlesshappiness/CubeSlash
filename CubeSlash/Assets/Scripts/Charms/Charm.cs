using UnityEngine;

[CreateAssetMenu(fileName = nameof(Charm), menuName = "Game/" + nameof(Charm), order = 1)]
public class Charm : ScriptableObject
{
    public string charm_name;

    [TextArea]
    public string charm_description;

    public Sprite sprite;
    public Upgrade upgrade;
    public InternalShopProduct shop_product;

    public bool IsUnlocked()
    {
        return shop_product == null || InternalShopController.Instance.IsPurchased(shop_product);
    }
}