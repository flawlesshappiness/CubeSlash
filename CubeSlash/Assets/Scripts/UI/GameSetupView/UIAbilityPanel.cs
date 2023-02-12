using UnityEngine;
using UnityEngine.EventSystems;

public class UIAbilityPanel : MonoBehaviour
{
    [SerializeField] private PlayerBodySettingsDatabase db_player_body_settings;
    [SerializeField] private LeftRightMenuItem menu;
    [SerializeField] private UIBodyPanel body_panel;
    [SerializeField] private UIBodyPartPanel part_panel;
    [SerializeField] private UILock uilock;
    [SerializeField] private GameObject icon_body, icon_ability;
    [SerializeField] private FMODEventReference sfx_move;

    private int idx_settings;
    private View parent_view;

    public PlayerBodySettings CurrentSettings { get; private set; }

    public System.Action onSettingsChanged;

    private void Start()
    {
        menu.onMove += OnMove;
        menu.onSubmit += OnClick;

        parent_view = GetComponentInParent<View>();

        SetSettings(0);
    }

    private void OnMove(int dir)
    {
        var idx_prev = idx_settings;
        idx_settings = Mathf.Clamp(idx_settings + dir, 0, db_player_body_settings.collection.Count - 1);
        SetSettings(idx_settings);

        if(idx_settings != idx_prev)
        {
            sfx_move.Play();
        }
    }

    private void SetSettings(int i)
    {
        idx_settings = i;

        var settings = db_player_body_settings.collection[i];
        body_panel.SetSettings(settings);
        part_panel.SetSettings(settings);
        CurrentSettings = settings;

        var is_locked = !settings.IsUnlocked();
        uilock.gameObject.SetActive(is_locked);
        icon_ability.SetActive(!is_locked);
        icon_body.SetActive(!is_locked);

        menu.submittable = is_locked;

        if (is_locked)
        {
            uilock.Price = settings.shop_product.price.amount;
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
        }
    }
}