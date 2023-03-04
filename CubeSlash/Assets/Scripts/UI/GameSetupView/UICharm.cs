using Flawliz.Lerp;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UICharm : MonoBehaviour
{
    [SerializeField] private UILock uilock;
    [SerializeField] private Image img_charm;
    [SerializeField] private Color c_selected, c_deselected;
    [SerializeField] private float s_selected, s_deselected;

    public Charm Info { get; private set; }
    public bool Activated { get; private set; }

    public void Initialize(Charm info)
    {
        Info = info;
        var unlocked = info.shop_product == null || info.shop_product.IsUnlocked();

        uilock.gameObject.SetActive(!unlocked);
        img_charm.enabled = unlocked;

        if (!unlocked)
        {
            uilock.Price = info.shop_product.price.amount;
        }

        img_charm.sprite = info.sprite;
        img_charm.color = c_deselected;
        img_charm.transform.localScale = Vector3.one * s_deselected;
        Activated = false;
    }

    public void Activate()
    {
        SetActivated(true);
    }

    public void Deactivate()
    {
        SetActivated(false);
    }

    private void SetActivated(bool selected)
    {
        Activated = selected;
        AnimateActivate(selected);
    }

    private Lerp AnimateActivate(bool selected)
    {
        var c_start = img_charm.color;
        var c_end = selected ? c_selected : c_deselected;
        var s_start = img_charm.transform.localScale;
        var s_end = Vector3.one * (selected ? s_selected : s_deselected);
        return Lerp.Value("anim_select_" + GetInstanceID(), 0.25f, f =>
        {
            img_charm.color = Color.Lerp(c_start, c_end, f);
            img_charm.transform.localScale = Vector3.Lerp(s_start, s_end, f);
        }).UnscaledTime().Curve(EasingCurves.EaseOutQuad);
    }

    public void AnimateUnlock()
    {
        uilock.AnimateUnlock();
        Lerp.Alpha(img_charm, 0.5f, 1);
    }
}