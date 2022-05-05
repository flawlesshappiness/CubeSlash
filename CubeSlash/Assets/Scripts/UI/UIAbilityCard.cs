using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAbilityCard : MonoBehaviour
{
    [SerializeField] private TMP_Text tmp_name;
    [SerializeField] private TMP_Text tmp_desc;
    [SerializeField] private Image img_icon;
    [SerializeField] private UIInputButton button;

    public UIInputButton InputButton { get { return button; } }

    public void SetAbility(Ability ability)
    {
        var active = ability != null;
        tmp_name.text = active ? ability.name_ability : "None";
        tmp_desc.text = active ? ability.desc_ability : "No ability equipped...";
        img_icon.sprite = active ? ability.sprite_icon : null;
    }
}
