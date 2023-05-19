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
        InitializePlayer();
        ShowBodyOptions();
    }

    private void InitializePlayer()
    {
        Player.gameObject.SetActive(true);
        Player.Rigidbody.isKinematic = true;
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

    private void ShowBodyOptions()
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

        InsertBackOption(options, ShowBodyOptions);

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
            Player.SetPlayerBody(info);
            Body.SetBodySprite(skin);
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

        InsertBackOption(options, ShowBodyOptions);

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
            ShowBodyOptions();
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
                ShowBodyOptions();
            }
        }

        void Cancel()
        {
            ShowBodyOptions();
        }
    }
}