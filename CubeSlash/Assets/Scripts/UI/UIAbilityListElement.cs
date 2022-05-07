using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIAbilityListElement : MonoBehaviour
{
    [SerializeField] private TMP_Text tmp_name;
    [SerializeField] private Image img_icon;
    [SerializeField] private Button btn;

    public string Name { set { tmp_name.text = value; } }
    public Sprite Sprite { set { img_icon.sprite = value; } }
    public Button Button { get { return btn; } }
    public Button.ButtonClickedEvent OnClicked { get { return btn.onClick; } }

    public void SetAbility(Ability ability)
    {
        Name = ability.name_ability;
        Sprite = ability.sprite_icon;
    }
}
