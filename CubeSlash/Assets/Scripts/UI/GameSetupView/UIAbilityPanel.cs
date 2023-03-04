using Flawliz.Lerp;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIAbilityPanel : MonoBehaviour
{
    [SerializeField] private PlayerBodySettingsDatabase db_player_body_settings;
    [SerializeField] private LeftRightMenuItem menu;
    [SerializeField] private UIBodyPanel body_panel;
    [SerializeField] private UIBodyPartPanel part_panel;
    [SerializeField] private UILock uilock;
    [SerializeField] private CanvasGroup cvg_icon_body, cvg_icon_ability;

    private int idx_settings;
    private View parent_view;
    private Coroutine cr_unlock;

    public PlayerBodySettings CurrentSettings { get; private set; }

    public System.Action onSettingsChanged;

    private void Start()
    {
        menu.onMove += OnMove;
        menu.onSubmit += OnClick;

        parent_view = GetComponentInParent<View>();

        SetSettings(Save.Game.idx_gamesetup_ability);
    }

    private void OnMove(int dir)
    {
        var idx_prev = idx_settings;
        SetSettings(idx_settings + dir);

        if(idx_settings != idx_prev)
        {
            SoundController.Instance.Play(SoundEffectType.sfx_ui_move);
        }
    }

    private void SetSettings(int i)
    {
        StopAnimateUnlock();

        var collection = db_player_body_settings.collection;
        idx_settings = Mathf.Clamp(i, 0, collection.Count - 1);
        Save.Game.idx_gamesetup_ability = idx_settings;

        var settings = collection[idx_settings];
        body_panel.SetSettings(settings);
        part_panel.SetSettings(settings);
        CurrentSettings = settings;

        var is_locked = !settings.IsUnlocked();
        cvg_icon_ability.alpha = is_locked ? 0 : 1;
        cvg_icon_body.alpha = is_locked ? 0 : 1;

        menu.submittable = is_locked;

        if (is_locked)
        {
            uilock.SetLocked();
            uilock.Price = settings.shop_product.price.amount;
        }
        else
        {
            uilock.SetUnlocked();
        }

        onSettingsChanged?.Invoke();
    }

    private void OnClick()
    {
        if (!CurrentSettings.IsUnlocked())
        {
            var selected_button = EventSystem.current.currentSelectedGameObject;
            parent_view.Interactable = false;

            var view = ViewController.Instance.ShowView<ConfirmPurchaseView>(0, nameof(ConfirmPurchaseView));
            view.SetProduct(CurrentSettings.shop_product);
            view.Title = CurrentSettings.GetAbilityInfo().name_ability;
            view.onConfirm += () => PurchaseBody(CurrentSettings);
            view.onConfirm += Close;
            view.onDecline += Close;

            void Close()
            {
                view.Close(0);
                parent_view.Interactable = true;
                EventSystem.current.SetSelectedGameObject(selected_button);
            }
        }
    }

    private void PurchaseBody(PlayerBodySettings settings)
    {
        if(InternalShopController.Instance.TryPurchaseProduct(settings.shop_product))
        {
            SetSettings(idx_settings);
            AnimateUnlock();
        }
    }

    private Coroutine AnimateUnlock()
    {
        StopAnimateUnlock();
        cr_unlock = StartCoroutine(Cr());
        return cr_unlock;
        IEnumerator Cr()
        {
            uilock.AnimateUnlock();
            yield return LerpEnumerator.Value(0.5f, f =>
            {
                var a = Mathf.Lerp(0f, 1f, f);
                cvg_icon_body.alpha = a;
                cvg_icon_ability.alpha = a;
            });
        }
    }

    private void StopAnimateUnlock()
    {
        if(cr_unlock != null)
        {
            StopCoroutine(cr_unlock);
        }
    }
}