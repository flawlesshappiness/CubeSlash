using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockUpgradeView : View
{
    [SerializeField] private CanvasGroup cvg_background;
    [SerializeField] private RadialMenu radial;
    [SerializeField] private UIFloatingPanel floating_panel;
    [SerializeField] private UIFloatingPanelUpgrade floating_upgrade;

    public event System.Action<Upgrade> OnUpgradeSelected;

    private void Awake()
    {
        cvg_background.alpha = 0;
    }

    private void Start()
    {
        var upgrades = GetRandomUpgrades();

        HideFloatingPanel();
        radial.OnSelect += OnSelect;
        AddRadialMenuElements(upgrades);

        Lerp.Alpha(cvg_background, 0.5f, 0.5f).UnscaledTime();
    }

    private List<UpgradeInfo> GetRandomUpgrades()
    {
        var count_upgrades = 3;
        var upgrades = UpgradeController.Instance.GetUnlockableUpgrades()
            .TakeRandom(count_upgrades);
        return upgrades;
    }

    private void OnSelect(RadialMenuElement element)
    {
        if (element == null)
        {
            HideFloatingPanel();
        }
        else
        {
            floating_panel.SetTarget(element.transform, new Vector2(50, 50));
        }
    }

    private void HideFloatingPanel()
    {
        floating_panel.gameObject.SetActive(false);
    }

    private void AddRadialMenuElements(List<UpgradeInfo> upgrades)
    {
        radial.Clear();

        var ability_icon = AbilityController.Instance.GetAbilityPrefab(Save.PlayerBody.primary_ability).Info.sprite_icon;
        var difficulty_icon = DifficultyController.Instance.Difficulty.difficulty_sprite;

        var options = new List<RadialMenuOption>();

        foreach (var info in upgrades)
        {
            options.Add(new RadialMenuOption
            {
                Sprite = info.upgrade.icon,
                OnSelect = () => UpdateDisplayText(info),
                OnSubmitBegin = OnSubmitBegin,
                OnSubmitComplete = () => SelectUpgrade(info)
            });
        }

        radial.AddOptions(options);
        radial.AnimateShowElements(true, 0.05f);
        radial.SetCancelElement(radial.GetElement(0));
    }

    private void SelectUpgrade(UpgradeInfo info)
    {
        UpgradeController.Instance.UnlockUpgrade(info.upgrade.id);
        SoundController.Instance.Play(SoundEffectType.sfx_ui_unlock_upgrade);
        OnUpgradeSelected?.Invoke(info.upgrade);
        Close(0);
    }

    private void UpdateDisplayText(UpgradeInfo info)
    {
        floating_panel.gameObject.SetActive(true);
        floating_upgrade.Clear();

        var modifiers = info.upgrade.modifiers;
        foreach (var modifier in modifiers)
        {
            var attribute = GameAttributeController.Instance.GetAttribute(modifier.attribute_type);
            floating_upgrade.AddModifiedAttribute(attribute, modifier);
        }

        StartCoroutine(Cr());
        IEnumerator Cr()
        {
            floating_panel.CanvasGroup.alpha = 0;
            yield return null;
            floating_panel.ContentSizeFitterRefresh.RefreshContentFitters();
            floating_panel.CanvasGroup.alpha = 1;
        }
    }

    private void OnSubmitBegin()
    {
        Lerp.Alpha(cvg_background, 0.5f, 0).UnscaledTime();
        HideFloatingPanel();
    }
}