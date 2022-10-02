using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnlockUpgradeView : View
{
    [SerializeField] private UIUnlockedUpgradesLayout layout_unlocked_upgrades;
    [SerializeField] private UIIconButton temp_btn_upgrade;
    [SerializeField] private TMP_Text tmp_upgrade;
    [SerializeField] private RectTransform rt_upgrades, rt_desc;

    public event System.Action<Upgrade> OnUpgradeSelected;

    private List<UIIconButton> btns_upgrade = new List<UIIconButton>();
    private List<TMP_Text> tmps_upgrade = new List<TMP_Text>();

    private UIIconButton button_selected;
    private Upgrade upgrade_selected;

    private void Start()
    {
        // Initialize
        temp_btn_upgrade.gameObject.SetActive(false);
        tmp_upgrade.gameObject.SetActive(false);

        layout_unlocked_upgrades.OnUpgradeLevelSelected += level => DisplayUpgradeText(level);

        // Fetch upgrades
        var upgrades = UpgradeController.Instance.GetUnlockableUpgrades()
            .TakeRandom(4);

        // Create upgrade buttons
        ClearUpgradeButtons();
        foreach (var info in upgrades)
        {
            var btn = CreateUpgradeButton();
            btn.Icon = info.upgrade.icon;
            btn.Button.OnSelectedChanged += s => OnSelected(s, btn, info.upgrade);
            btn.Button.onClick.AddListener(() => OnClick(btn, info.upgrade));
        }

        // Set default ui
        EventSystemController.Instance.SetDefaultSelection(btns_upgrade[0].Button.gameObject);

        void OnSelected(bool selected, UIIconButton button, Upgrade upgrade)
        {
            if (selected)
            {
                button_selected = button;
                upgrade_selected = upgrade;
                DisplayUpgradeText(upgrade);
            }
        }

        void OnClick(UIIconButton btn, Upgrade upgrade)
        {
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            UpgradeController.Instance.UnlockUpgrade(upgrade.id);
            OnUpgradeSelected?.Invoke(upgrade);
            Close(0);
        }
    }

    private void OnEnable()
    {
        PlayerInput.Controls.Player.North.started += OnNorthPressed;
        PlayerInput.Controls.Player.North.canceled += OnNorthReleased;
    }

    private void OnDisable()
    {
        PlayerInput.Controls.Player.North.started -= OnNorthPressed;
        PlayerInput.Controls.Player.North.canceled -= OnNorthReleased;
    }

    private void OnNorthPressed(InputAction.CallbackContext context)
    {
        if(upgrade_selected != null)
        {
            ShowUpgradeTree();
        }
    }

    private void OnNorthReleased(InputAction.CallbackContext context)
    {
        HideUpgradeTree();
    }

    private UIIconButton CreateUpgradeButton()
    {
        var btn = Instantiate(temp_btn_upgrade, temp_btn_upgrade.transform.parent);
        btn.gameObject.SetActive(true);
        btns_upgrade.Add(btn);
        return btn;
    }

    private void ClearUpgradeButtons()
    {
        btns_upgrade.ForEach(b => Destroy(b.gameObject));
        btns_upgrade.Clear();
    }

    private TMP_Text CreateUpgradeText(string text, Color color)
    {
        var tmp = Instantiate(tmp_upgrade, tmp_upgrade.transform.parent);
        tmp.gameObject.SetActive(true);
        tmp.text = text;
        tmp.color = color;
        tmps_upgrade.Add(tmp);
        return tmp;
    }

    private void ClearUpgradeTexts()
    {
        tmps_upgrade.ForEach(t => Destroy(t.gameObject));
        tmps_upgrade.Clear();
    }

    private void DisplayUpgradeText(Upgrade upgrade)
    {
        ClearUpgradeTexts();
        CreateUpgradeText(upgrade.name, ColorPalette.Main.Get(ColorPalette.Type.HIGHLIGHT));
        foreach(var effect in upgrade.effects)
        {
            var color = effect.type_effect == Upgrade.Effect.TypeEffect.POSITIVE ? ColorPalette.Main.Get(ColorPalette.Type.HIGHLIGHT) : ColorPalette.Main.Get(ColorPalette.Type.WRONG);
            var tmp = CreateUpgradeText(effect.variable.GetDisplayString(true), color);
        }
    }

    private void ShowUpgradeTree()
    {
        Interactable = false;

        rt_upgrades.gameObject.SetActive(false);
        rt_desc.gameObject.SetActive(false);

        var view = ViewController.Instance.ShowView<UpgradeTreeView>(0, "UpgradeTree");
        var tree = UpgradeController.Instance.GetUpgradeTree(upgrade_selected.id);
        view.SetTree(tree);
    }

    private void HideUpgradeTree()
    {
        ViewController.Instance.CloseView(0, "UpgradeTree");

        rt_upgrades.gameObject.SetActive(true);
        rt_desc.gameObject.SetActive(true);

        Interactable = true;
        EventSystemController.Instance.SetDefaultSelection(btns_upgrade[0].Button.gameObject);
        EventSystemController.Instance.EventSystem.SetSelectedGameObject(button_selected.Button.gameObject);
    }
}