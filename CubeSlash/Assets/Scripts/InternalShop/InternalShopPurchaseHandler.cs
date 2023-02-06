using UnityEngine;

public class InternalShopPurchaseHandler : Singleton
{
    public static InternalShopPurchaseHandler Instance { get { return Instance<InternalShopPurchaseHandler>(); } }

    protected override void Initialize()
    {
        base.Initialize();
        InternalShopController.Instance.onPurchase += OnPurchase;
    }

    private void OnPurchase(InternalShopPurchase purchase)
    {
        Debug.Log($"Product purchased: {purchase.id}");

        if(purchase.id == InternalShopProductID.Default)
        {
            // Do nothing
        }
    }
}