using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayRadialView : View
{
    [SerializeField] private RadialMenu radial;
    [SerializeField] private UIFloatingPanel floating_panel;
    [SerializeField] private UIFloatingPanelText floating_text;

    private Player Player { get { return Player.Instance; } }

    private void Start()
    {
        ShowMainOptions();

        radial.OnSelect += OnRadialSelect;
        radial.OnSubmitBegin += _ => HideFloatingPanel();

        CameraController.Instance.AnimateSize(1f, 6f, EasingCurves.EaseInOutQuad);
    }

    private void Back()
    {
        ViewController.Instance.ShowView<StartView>(0);
    }

    private void HideFloatingPanel()
    {
        floating_panel.gameObject.SetActive(false);
    }

    private void OnRadialSelect(RadialMenuElement element)
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

    private void InsertBackOption(List<RadialMenuOption> options, System.Action OnBack)
    {
        options.Insert(0, new RadialMenuOption
        {
            Sprite = Icon.Get(IconType.arrow_back),
            OnSelect = HideFloatingPanel,
            OnSubmitComplete = OnBack
        });
    }

    private void ShowMainOptions()
    {
        radial.Clear();

        var ability_icon = AbilityController.Instance.GetAbilityPrefab(Save.PlayerBody.primary_ability).Info.sprite_icon;
        var difficulty_icon = DifficultyController.Instance.Difficulty.difficulty_sprite;

        var options = new List<RadialMenuOption>
        {
            new RadialMenuOption
            {
                Title = "Customize",
                Description = "Body",
                Sprite = Icon.Get(IconType.customize),
                OnSubmitComplete = () => ViewController.Instance.ShowView<BodyEditView>(0)
            },

            new RadialMenuOption
            {
                Title = "Start",
                Sprite = Icon.Get(IconType.start_game),
                OnSubmitComplete = () => GameController.Instance.StartGame()
            },

            new RadialMenuOption
            {
                Title = "Ability",
                Sprite = ability_icon,
                OnSubmitComplete = ShowAbilityOptions
            },

            new RadialMenuOption
            {
                Title = "Difficulty",
                Sprite = difficulty_icon,
                OnSubmitComplete = ShowDifficultyOptions
            },
        };

        InsertBackOption(options, Back);

        radial.AddOptions(options);
        radial.AnimateShowElements(true, 0.05f);
        radial.SetCancelElement(radial.GetElement(0));
    }

    private void ShowAbilityOptions()
    {
        radial.Clear();

        var abilities = AbilityController.Instance.GetAbilities();

        var options = abilities
            .Select(a => new RadialMenuOption
            {
                Title = AbilityController.Instance.IsAbilityUnlocked(a.Info.type) ? null : "Play",
                Description = AbilityController.Instance.IsAbilityUnlocked(a.Info.type) ? null : "to unlock",
                Sprite = a.Info.sprite_icon,
                IsLocked = !AbilityController.Instance.IsAbilityUnlocked(a.Info.type),
                IsNew = Save.Game.new_abilities.Contains(a.Info.type),
                OnSelect = () => Select(a.Info),
                OnSubmitComplete = () => Submit(a.Info)
            }).ToList();

        InsertBackOption(options, ShowMainOptions);

        radial.AddOptions(options);
        radial.AnimateShowElements(true, 0.05f);
        radial.SetCancelElement(radial.GetElement(0));

        void Submit(AbilityInfo info)
        {
            Player.Instance.SetPrimaryAbility(info.type);
            Player.Instance.UpdateBodyparts();
            ShowMainOptions();
        }

        void Select(AbilityInfo info)
        {
            Save.Game.new_abilities.Remove(info.type);

            floating_text.Clear();
            floating_text.AddText(info.desc_ability);
            floating_text.gameObject.SetActive(true);
            floating_panel.Refresh();
        }
    }

    private void ShowDifficultyOptions()
    {
        radial.Clear();

        var infos = DifficultyController.Instance.DifficultyInfos;

        var options = infos
            .Select(info => new RadialMenuOption
            {
                Title = info.difficulty_name,
                Sprite = info.difficulty_sprite,
                IsLocked = infos.IndexOf(info) > (Save.Game.idx_difficulty_completed + 1),
                OnSubmitComplete = () => SelectDifficulty(info)
            }).ToList();

        InsertBackOption(options, ShowMainOptions);

        radial.AddOptions(options);
        radial.AnimateShowElements(true, 0.05f);
        radial.SetCancelElement(radial.GetElement(0));

        void SelectDifficulty(DifficultyInfo info)
        {
            DifficultyController.Instance.SetDifficulty(info);
            ShowMainOptions();
        }
    }
}