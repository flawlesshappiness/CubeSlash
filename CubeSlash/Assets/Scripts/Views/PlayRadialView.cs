using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayRadialView : View
{
    [SerializeField] private RadialMenu radial;
    [SerializeField] private UIFloatingPanel floating_panel;
    [SerializeField] private UIFloatingPanelText floating_text;

    private void Start()
    {
        HideFloatingPanel();
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
        var gamemode_icon = GamemodeController.Instance.SelectedGameMode.icon;

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
                Title = "Ability",
                Sprite = ability_icon,
                OnSubmitComplete = ShowAbilityOptions
            },

            new RadialMenuOption
            {
                Title = "Start",
                Sprite = Icon.Get(IconType.start_game),
                OnSubmitComplete = () => GameController.Instance.StartGame()
            },

            new RadialMenuOption
            {
                Title = "Gamemode",
                Sprite = gamemode_icon,
                OnSubmitComplete = ShowGamemodeOptions
            },

             new RadialMenuOption
            {
                Title = "Credits",
                Sprite = Icon.Get(IconType.credits),
                OnSubmitComplete = ShowCredits
            },
        };

        InsertBackOption(options, Back);

        radial.AddOptions(options);
        radial.AnimateShowElements(true, 0.05f);
        radial.SetCancelElement(radial.GetElement(0));
    }

    private void ShowCredits()
    {
        ViewController.Instance.ShowView<CreditsView>(0.5f);
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
            SaveDataController.Instance.Save<GameSaveData>();
            ShowMainOptions();
        }

        void Select(AbilityInfo info)
        {
            Save.Game.new_abilities.Remove(info.type);

            var unlocked = AbilityController.Instance.IsAbilityUnlocked(info.type);
            if (unlocked)
            {
                floating_text.Clear();
                floating_text.AddText(info.desc_ability);
                floating_text.gameObject.SetActive(true);
                floating_panel.Refresh();
            }
            else
            {
                HideFloatingPanel();
            }
        }
    }

    private void ShowGamemodeOptions()
    {
        radial.Clear();

        var infos = GamemodeController.Instance.DB.collection;

        var options = infos
            .Select(info => new RadialMenuOption
            {
                Title = info.gamemode_name,
                Description = info.gamemode_desc,
                Sprite = info.icon,
                IsLocked = !GamemodeController.Instance.IsGamemodeUnlocked(info.type),
                OnSubmitComplete = () => Select(info)
            }).ToList();

        InsertBackOption(options, ShowMainOptions);

        radial.AddOptions(options);
        radial.AnimateShowElements(true, 0.05f);
        radial.SetCancelElement(radial.GetElement(0));

        void Select(GamemodeSettings info)
        {
            GamemodeController.Instance.SetGamemode(info);
            ShowMainOptions();
        }
    }
}