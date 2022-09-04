using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnlockUpgradeView : View
{
    [SerializeField] private UIUnlockedUpgradesLayout layout_unlocked_upgrades;
    [SerializeField] private UIIconButton temp_btn_upgrade;
    [SerializeField] private UIIconButton temp_btn_tree;
    [SerializeField] private TMP_Text tmp_upgrade;

    private List<UIIconButton> btns_upgrade = new List<UIIconButton>();
    private List<UIIconButton> btns_tree = new List<UIIconButton>();
    private List<TMP_Text> tmps_upgrade = new List<TMP_Text>();

    public event System.Action<Upgrade> OnUpgradeSelected;

    private void Start()
    {
        // Initialize
        temp_btn_upgrade.gameObject.SetActive(false);
        temp_btn_tree.gameObject.SetActive(false);
        tmp_upgrade.gameObject.SetActive(false);

        layout_unlocked_upgrades.OnUpgradeLevelSelected += level => DisplayUpgradeText(level);

        // Fetch upgrades
        var upgrades = UpgradeController.Instance.GetUnlockableUpgrades()
            .TakeRandom(4);

        // Create upgrade buttons
        ClearUpgradeButtons();
        foreach (var upgrade in upgrades)
        {
            var level = upgrade.GetCurrentLevel();
            var btn = CreateUpgradeButton();
            btn.Icon = level.icon;
            btn.Button.OnSelectedChanged += s => OnSelected(s, upgrade);
            btn.Button.onClick.AddListener(() => OnClick(btn, upgrade));
        }

        // Set default ui
        EventSystemController.Instance.SetDefaultSelection(btns_upgrade[0].Button.gameObject);

        void OnSelected(bool selected, Upgrade upgrade)
        {
            if (selected)
            {
                DisplayUpgradeText(upgrade.GetCurrentLevel());
                DisplayUpgradeTree(upgrade);
            }
        }

        void OnClick(UIIconButton btn, Upgrade upgrade)
        {
            CanvasGroup.interactable = false;
            CanvasGroup.blocksRaycasts = false;
            UpgradeController.Instance.IncrementUpgradeLevel(upgrade.data.type);
            OnUpgradeSelected?.Invoke(upgrade);
            Close(0);
        }
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

    private UIIconButton CreateTreeButton()
    {
        var btn = Instantiate(temp_btn_tree, temp_btn_tree.transform.parent);
        btn.gameObject.SetActive(true);
        btns_tree.Add(btn);
        return btn;
    }

    private void ClearTreeButtons()
    {
        btns_tree.ForEach(b => Destroy(b.gameObject));
        btns_tree.Clear();
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

    private void DisplayUpgradeText(UpgradeData.Level level)
    {
        ClearUpgradeTexts();
        CreateUpgradeText(level.name, ColorPalette.Main.Get(ColorPalette.Type.HIGHLIGHT));
        CreateUpgradeText("", ColorPalette.Main.Get(ColorPalette.Type.HIGHLIGHT));
        foreach(var desc in level.desc_positive)
        {
            var tmp = CreateUpgradeText(desc, ColorPalette.Main.Get(ColorPalette.Type.HIGHLIGHT));
        }
        foreach (var desc in level.desc_negative)
        {
            var tmp = CreateUpgradeText(desc, ColorPalette.Main.Get(ColorPalette.Type.WRONG));
        }
    }

    private void DisplayUpgradeTree(Upgrade upgrade)
    {
        ClearTreeButtons();
        foreach(var level in upgrade.data.levels)
        {
            var btn = CreateTreeButton();
            btn.Icon = level.icon;
            btn.Button.OnSelectedChanged += s => OnSelected(s, level);
        }

        void OnSelected(bool selected, UpgradeData.Level level)
        {
            if (selected)
            {
                DisplayUpgradeText(level);
            }
        }
    }
}