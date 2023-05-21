using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BodyEditView : View
{
    [SerializeField] private RadialMenu radial;

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

        if(Body.Bodyparts.Count > 0)
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
    }

    private void ShowBodySelect()
    {
        var db = Database.Load<PlayerBodyDatabase>();
        radial.Clear();

        var options = db.collection.Select(info => new RadialMenuOption
        {
            Sprite = info.prefab.GetBodySprite(),
            OnSubmitComplete = () => ShowBodySkinSelect(info)
        }).ToList();

        InsertBackOption(options, ShowMainOptions);

        radial.AddOptions(options);
        radial.AnimateShowElements(true, 0.05f);
    }

    private void ShowBodySkinSelect(PlayerBodyInfo info)
    {
        radial.Clear();

        var options = info.skins.Select(skin => new RadialMenuOption
        {
            Sprite = skin,
            OnSubmitComplete = () => SelectBody(info, skin)
        }).ToList();

        InsertBackOption(options, ShowBodySelect);

        radial.AddOptions(options);
        radial.AnimateShowElements(true, 0.05f);

        void SelectBody(PlayerBodyInfo info, Sprite skin)
        {
            Save.PlayerBody.body_type = info.type;
            Save.PlayerBody.body_skin = info.skins.IndexOf(skin);

            Player.SetPlayerBody(info);
            Body.SetBodySprite(skin);
            Player.UpdateBodyparts();
            ShowBodySelect();
        }
    }

    private void ShowBodypartSelect()
    {
        var db = Database.Load<BodypartDatabase>();
        radial.Clear();

        var options = db.collection
            .Where(info => !info.is_ability_part)
            .Select(info => new RadialMenuOption
        {
            Sprite = info.preview,
            OnSubmitComplete = () => SelectBodypart(info)
            }).ToList();

        InsertBackOption(options, ShowMainOptions);

        radial.AddOptions(options);
        radial.AnimateShowElements(true, 0.05f);

        void SelectBodypart(BodypartInfo info)
        {
            var part = BodypartEditController.Instance.CreatePart(info);
            part.SetPosition(0.5f);
            BodypartEditController.Instance.BeginMovingPart(part, ShowBodypartSelect, () => Cancel(part));
        }

        void Cancel(Bodypart part)
        {
            BodypartEditController.Instance.RemovePart(part);
            ShowBodypartSelect();
        }
    }

    private void SelectPartToMove(Bodypart selected)
    {
        BodypartEditController.Instance.BeginSelectingPart(selected, Select, Cancel);

        void Select(Bodypart part)
        {
            BodypartEditController.Instance.BeginMovingPart(part, () => SelectPartToMove(part), () => SelectPartToMove(part));
        }

        void Cancel()
        {
            ShowMainOptions();
        }
    }

    private void SelectPartToRemove(Bodypart selected)
    {
        BodypartEditController.Instance.BeginSelectingPart(null, Select, Cancel);

        void Select(Bodypart part)
        {
            BodypartEditController.Instance.BeginRemovingPart(part, OnRemove);
        }

        void OnRemove(Bodypart part)
        {
            if(part != null)
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
            ShowMainOptions();
        }
    }
}