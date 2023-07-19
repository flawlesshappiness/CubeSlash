using Flawliz.Lerp;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockAbilityView : View
{
    [SerializeField] private CanvasGroup cvg_background;
    [SerializeField] private RadialMenu radial;
    [SerializeField] private UIFloatingPanel floating_panel;
    [SerializeField] private UIFloatingPanelUpgrade floating_upgrade;

    public event System.Action OnAbilitySelected;
    public event System.Action OnSkip;

    private void Awake()
    {
        cvg_background.alpha = 0;
    }

    private void Start()
    {
        var is_first = AbilityController.Instance.GetGainedAbilities().Count == 0;
        var abilities = AbilityController.Instance.GetAvailableAbilities().TakeRandom(2);

        HideFloatingPanel();
        radial.OnSelect += OnSelect;
        AddRadialMenuElements(abilities);

        Lerp.Alpha(cvg_background, 0.5f, 0.5f).UnscaledTime();
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

    private void AddRadialMenuElements(List<Ability> abilities)
    {
        radial.Clear();

        var ability_icon = AbilityController.Instance.GetAbilityPrefab(Save.PlayerBody.primary_ability).Info.sprite_icon;
        var difficulty_icon = DifficultyController.Instance.Difficulty.difficulty_sprite;

        var options = new List<RadialMenuOption>();

        foreach (var ability in abilities)
        {
            options.Add(new RadialMenuOption
            {
                Sprite = ability.Info.sprite_icon,
                OnSelect = () => UpdateDisplayText(ability),
                OnSubmitBegin = OnSubmitBegin,
                OnSubmitComplete = () => SelectAbility(ability)
            });
        }

        if (options.Count < 2)
        {
            options.Add(new RadialMenuOption
            {
                IsLocked = true,
                OnSelect = HideFloatingPanel
            });
        }

        options.Insert(0, new RadialMenuOption
        {
            Sprite = Icon.Get(IconType.arrow_back),
            OnSelect = HideFloatingPanel,
            OnSubmitComplete = Skip
        });

        radial.AddOptions(options);
        radial.AnimateShowElements(true, 0.05f);
        radial.SetCancelElement(radial.GetElement(0));
    }

    private void OnSubmitBegin()
    {
        Lerp.Alpha(cvg_background, 0.5f, 0).UnscaledTime();
        HideFloatingPanel();
    }

    private void SelectAbility(Ability ability)
    {
        AbilityController.Instance.GainAbility(ability.Info.type);
        AbilityController.Instance.AddModifier(ability.Info.type);
        SoundController.Instance.Play(SoundEffectType.sfx_ui_unlock_ability);
        OnAbilitySelected?.Invoke();
        Close(0);
    }

    private void UpdateDisplayText(Ability ability)
    {
        floating_panel.gameObject.SetActive(true);
        floating_upgrade.Clear();
        var upgrade_id = AbilityController.Instance.GetEquippedAbility().Info.modifiers.GetModifier(ability.Info.type).id;
        var upgrade = UpgradeController.Instance.GetUpgradeInfo(upgrade_id).upgrade;
        var modifiers = upgrade.modifiers;
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

    private void Skip()
    {
        OnSkip?.Invoke();
        Close(0);
    }
}