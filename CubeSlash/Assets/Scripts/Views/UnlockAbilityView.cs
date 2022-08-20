using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UnlockAbilityView : View
{
    [SerializeField] private UIIconButton temp_btn_ability;
    [SerializeField] private TMP_Text tmp_desc;

    public event System.Action OnAbilitySelected;

    private List<UIIconButton> btns_ability = new List<UIIconButton>();

    private void Start()
    {
        temp_btn_ability.gameObject.SetActive(false);
        DisplayAbility(null);

        var abilities = AbilityController.Instance.GetUnlockableAbilities()
            .TakeRandom(2);

        ClearButtons();
        foreach(var ability in abilities)
        {
            var btn = CreateButton();
            btn.Icon = ability.sprite_icon;
            btn.Button.OnSelectedChanged += s => OnSelected(s, ability);
            btn.Button.onClick.AddListener(() => Click(btn, ability));
        }

        void OnSelected(bool selected, Ability ability)
        {
            if (selected)
            {
                DisplayAbility(ability);
            }
        }

        void Click(UIIconButton btn, Ability ability)
        {
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
            text += ability.name_ability;
            text += "\n";
            text += ability.desc_ability;
        }

        tmp_desc.text = text;
    }
}