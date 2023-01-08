using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnlockAbilityView : View
{
    [SerializeField] private UIIconButton temp_btn_ability;
    [SerializeField] private TMP_Text tmp_desc;
    [SerializeField] private FMODEventReference sfx_unlock_ability;

    public event System.Action OnAbilitySelected;

    private List<UIIconButton> btns_ability = new List<UIIconButton>();

    private void Start()
    {
        temp_btn_ability.gameObject.SetActive(false);
        DisplayAbility(null);

        var is_first = AbilityController.Instance.GetUnlockedAbilities().Count == 0;
        var abilities = is_first ?
            AbilityController.Instance.GetUnlockableAbilities()
            : AbilityController.Instance.GetUnlockableAbilities().TakeRandom(2);

        ClearButtons();
        foreach(var ability in abilities)
        {
            var btn = CreateButton();
            btn.Icon = ability.Info.sprite_icon;
            btn.Button.OnSelectedChanged += s => OnSelected(s, ability);
            btn.Button.onClick.AddListener(() => Click(btn, ability));
        }

        EventSystemController.Instance.SetDefaultSelection(btns_ability[0].Button.gameObject);

        void OnSelected(bool selected, Ability ability)
        {
            if (selected)
            {
                DisplayAbility(ability);
            }
        }

        void Click(UIIconButton btn, Ability ability)
        {
            FMODButtonEvent.PreviousSelected = null;
            AbilityController.Instance.UnlockAbility(ability.Info.type);
            sfx_unlock_ability.Play();
            OnAbilitySelected?.Invoke();
            Close(0);
        }
    }

    private UIIconButton CreateButton()
    {
        var btn = Instantiate(temp_btn_ability, temp_btn_ability.transform.parent);
        btn.gameObject.SetActive(true);
        btns_ability.Add(btn);
        return btn;
    }

    private void ClearButtons()
    {
        btns_ability.ForEach(b => Destroy(b.gameObject));
        btns_ability.Clear();
    }

    private void DisplayAbility(Ability ability)
    {
        var text = "";

        if(ability != null)
        {
            text += ability.Info.name_ability;
            text += "\n";
            text += ability.Info.desc_ability;
        }

        tmp_desc.text = text;
    }
}