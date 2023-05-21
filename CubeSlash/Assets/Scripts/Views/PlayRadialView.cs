using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayRadialView : View
{
    [SerializeField] private RadialMenu radial;

    private Player Player { get { return Player.Instance; } }

    private void Start()
    {
        ShowMainOptions();

        CameraController.Instance.AnimateSize(1f, 6f, EasingCurves.EaseInOutQuad);
    }

    private void Back()
    {
        ViewController.Instance.ShowView<StartView>(0);
    }

    private void InsertBackOption(List<RadialMenuOption> options, System.Action OnBack)
    {
        options.Insert(0, new RadialMenuOption
        {
            Sprite = Icon.Get(IconType.arrow_back),
            OnSubmitComplete = OnBack
        });
    }

    private void ShowMainOptions()
    {
        radial.Clear();

        var ability_icon = AbilityController.Instance.GetAbilityPrefab(Save.PlayerBody.primary_ability).Info.sprite_icon;

        var options = new List<RadialMenuOption>
        {
            new RadialMenuOption
            {
                Sprite = Icon.Get(IconType.customize),
                OnSubmitComplete = () => ViewController.Instance.ShowView<BodyEditView>(0)
            },

            new RadialMenuOption
            {
                Sprite = Icon.Get(IconType.start_game),
                OnSubmitComplete = () => GameController.Instance.StartGame()
            },

            new RadialMenuOption
            {
                Sprite = ability_icon,
                OnSubmitComplete = ShowAbilityOptions
            },
        };

        InsertBackOption(options, Back);

        radial.AddOptions(options);
        radial.AnimateShowElements(true, 0.05f);
    }

    private void ShowAbilityOptions()
    {
        radial.Clear();

        var abilities = AbilityController.Instance.GetAvailableAbilities();

        var options = abilities
            .Select(a => new RadialMenuOption
            {
                Sprite = a.Info.sprite_icon,
                OnSubmitComplete = () => SelectAbility(a.Info)
            }).ToList();

        InsertBackOption(options, ShowMainOptions);

        radial.AddOptions(options);
        radial.AnimateShowElements(true, 0.05f);

        void SelectAbility(AbilityInfo info)
        {
            Player.Instance.SetPrimaryAbility(info.type);
            Player.Instance.UpdateBodyparts();
            ShowMainOptions();
        }
    }
}