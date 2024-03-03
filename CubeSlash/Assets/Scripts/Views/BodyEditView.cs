using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BodyEditView : View
{
    [SerializeField] private RadialMenu radial;
    [SerializeField] private UIInputLayout inputs;

    private Player Player { get { return Player.Instance; } }
    private PlayerBody Body { get { return Player.PlayerBody; } }

    private void Start()
    {
        ShowMainOptions();
    }

    private void Back()
    {
        ViewController.Instance.ShowView<PlayRadialView>(0);
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
        inputs.Clear();
        radial.Clear();

        var options = new List<RadialMenuOption>
        {
            new RadialMenuOption
            {
                Sprite = Icon.Get(IconType.customize_option_body),
                OnSubmitComplete = ShowBodySelect
            },

            new RadialMenuOption
            {
                Sprite = Icon.Get(IconType.customize_add_part),
                OnSubmitComplete = ShowBodypartSelect
            },
        };

        if (Body.Bodyparts.Count > 0)
        {
            options.Add(new RadialMenuOption
            {
                Sprite = Icon.Get(IconType.customize_move_part),
                OnSubmitComplete = () => SelectPartToMove(null)
            });

            options.Add(new RadialMenuOption
            {
                Sprite = Icon.Get(IconType.customize_remove_part),
                OnSubmitComplete = () => SelectPartToRemove(null)
            });
        }

        InsertBackOption(options, Back);

        radial.AddOptions(options);
        radial.AnimateShowElements(true, 0.05f);
        radial.SetCancelElement(radial.GetElement(0));
    }

    private void ShowBodySelect()
    {
        var db = Database.Load<PlayerBodyDatabase>();
        radial.Clear();

        var options = db.collection.Select(info => new RadialMenuOption
        {
            Sprite = info.prefab.GetBodySprite(),
            Title = Save.Game.unlocked_player_bodies.Contains(info.type) ? "" : "Play",
            Description = Save.Game.unlocked_player_bodies.Contains(info.type) ? "" : "to unlock",
            IsLocked = !Save.Game.unlocked_player_bodies.Contains(info.type),
            IsNew = Save.Game.new_player_bodies.Contains(info.type),
            OnSelect = () => Select(info),
            OnSubmitComplete = () => ShowBodySkinSelect(info)
        }).ToList();

        InsertBackOption(options, ShowMainOptions);

        radial.AddOptions(options);
        radial.AnimateShowElements(true, 0.05f);
        radial.SetCancelElement(radial.GetElement(0));

        void Select(PlayerBodyInfo info)
        {
            Save.Game.new_player_bodies.Remove(info.type);
            SaveDataController.Instance.Save<PlayerBodySaveData>();
        }
    }

    private void ShowBodySkinSelect(PlayerBodyInfo info)
    {
        radial.Clear();

        var options = info.skins.Select(skin => new RadialMenuOption
        {
            Sprite = skin,
            OnSubmitComplete = () => Submit(info, skin)
        }).ToList();

        InsertBackOption(options, ShowBodySelect);

        radial.AddOptions(options);
        radial.AnimateShowElements(true, 0.05f);
        radial.SetCancelElement(radial.GetElement(0));

        void Submit(PlayerBodyInfo info, Sprite skin)
        {
            Save.PlayerBody.body_type = info.type;
            Save.PlayerBody.body_skin = info.skins.IndexOf(skin);

            Player.SetPlayerBody(info);
            Body.SetBodySprite(skin);
            Player.UpdateBodyparts();
            SaveDataController.Instance.Save<PlayerBodySaveData>();
            ShowBodySelect();
        }
    }

    private void ShowBodypartSelect()
    {
        var db = Database.Load<BodypartDatabase>();
        radial.Clear();
        inputs.Clear();

        var options = db.collection
            .Where(info => !info.is_ability_part)
            .Select(info => new RadialMenuOption
            {
                Sprite = info.preview,
                Title = Save.Game.unlocked_bodyparts.Contains(info.type) ? "" : "Play",
                Description = Save.Game.unlocked_bodyparts.Contains(info.type) ? "" : "to unlock",
                IsLocked = !Save.Game.unlocked_bodyparts.Contains(info.type),
                IsNew = Save.Game.new_bodyparts.Contains(info.type),
                OnSelect = () => Select(info),
                OnSubmitComplete = () => Submit(info)
            }).ToList();

        InsertBackOption(options, ShowMainOptions);

        radial.AddOptions(options);
        radial.AnimateShowElements(true, 0.05f);
        radial.SetCancelElement(radial.GetElement(0));

        void Submit(BodypartInfo info)
        {
            var part = BodypartEditController.Instance.CreatePart(info);
            if (part != null)
            {
                part.SetPosition(0.5f);
                part.SetSize(0.5f);
                BodypartEditController.Instance.BeginMovingPart(part, ShowBodypartSelect, () => Cancel(part));

                ShowMoveBodypartInput();
            }
            else
            {
                ShowBodypartSelect();
            }
        }

        void Cancel(Bodypart part)
        {
            BodypartEditController.Instance.RemovePart(part);
            ShowBodypartSelect();
        }

        void Select(BodypartInfo info)
        {
            Save.Game.new_bodyparts.Remove(info.type);
            SaveDataController.Instance.Save<PlayerBodySaveData>();
        }
    }

    private void SelectPartToMove(Bodypart selected)
    {
        ShowSelectBodypartInput();
        BodypartEditController.Instance.BeginSelectingPart(selected, Select, Cancel);

        void Select(Bodypart part)
        {
            BodypartEditController.Instance.BeginMovingPart(part, () => SelectPartToMove(part), () => SelectPartToMove(part));
            ShowMoveBodypartInput();
        }

        void Cancel()
        {
            SaveDataController.Instance.Save<PlayerBodySaveData>();
            ShowMainOptions();
        }
    }

    private void SelectPartToRemove(Bodypart selected)
    {
        ShowRemoveBodypartInput();
        BodypartEditController.Instance.BeginSelectingPart(null, Select, Cancel);

        void Select(Bodypart part)
        {
            BodypartEditController.Instance.BeginRemovingPart(part, OnRemove);
        }

        void OnRemove(Bodypart part)
        {
            if (part != null)
            {
                SelectPartToRemove(part);
            }
            else
            {
                ShowMainOptions();
            }
        }

        void Cancel()
        {
            SaveDataController.Instance.Save<PlayerBodySaveData>();
            ShowMainOptions();
        }
    }

    private void ShowMoveBodypartInput()
    {
        inputs.Clear();
        inputs.AddInput(PlayerInput.UIButtonType.NAV_UP_DOWN, "Move");
        inputs.AddInput(PlayerInput.UIButtonType.NAV_LEFT_RIGHT, "Size");
        inputs.AddInput(PlayerInput.UIButtonType.SOUTH, "Set");
        inputs.AddInput(PlayerInput.UIButtonType.WEST, "Mirror");
        inputs.AddInput(PlayerInput.UIButtonType.EAST, "Cancel");
    }

    private void ShowSelectBodypartInput()
    {
        inputs.Clear();
        inputs.AddInput(PlayerInput.UIButtonType.NAV_UP_DOWN, "Move");
        inputs.AddInput(PlayerInput.UIButtonType.SOUTH, "Select");
        inputs.AddInput(PlayerInput.UIButtonType.EAST, "Cancel");
    }

    private void ShowRemoveBodypartInput()
    {
        inputs.Clear();
        inputs.AddInput(PlayerInput.UIButtonType.NAV_UP_DOWN, "Move");
        inputs.AddInput(PlayerInput.UIButtonType.SOUTH, "Remove");
        inputs.AddInput(PlayerInput.UIButtonType.EAST, "Cancel");
    }
}