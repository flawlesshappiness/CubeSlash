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
        LogController.LogMessage($"InternalShopPurchaseHandler.OnPurchase(): Purchased {purchase.id}");

        if (purchase.id == InternalShopProductID.Default)
        {
            // Do nothing
        }
    }
}