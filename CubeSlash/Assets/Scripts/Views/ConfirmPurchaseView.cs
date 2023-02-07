using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmPurchaseView : View
{
    [SerializeField] private TMP_Text tmp_title, tmp_currency_amount;
    [SerializeField] private Image img_currency_icon;
    [SerializeField] private SelectableMenuItem btn_confirm, btn_decline;
    [SerializeField] private UICurrencyBar currencybar;

    public event System.Action onConfirm, onDecline;

    public string Title { set { tmp_title.text = value.ToString(); } }

    private void Start()
    {
        btn_confirm.onSubmit += ClickConfirm;
        btn_decline.onSubmit += ClickDecline;
    }

    public void SetProduct(InternalShopProduct product)
    {
        tmp_currency_amount.text = CurrencyController.FormatCurrencyString(product.price.amount);

        var currency_info = CurrencyInfo.Load(product.price.type);
        if(currency_info != null)
        {
            img_currency_icon.sprite = currency_info.sprite;
        }

        if (!CurrencyController.Instance.CanAfford(product.price))
        {
            btn_confirm.interactable = false;
            btn_confirm.CanvasGroup.alpha = 0.25f;
        }

        currencybar.SetCurrencyType(product.price.type);
    }

    private void ClickConfirm()
    {
        onConfirm?.Invoke();
    }

    private void ClickDecline()
    {
        onDecline?.Invoke();
    }
}