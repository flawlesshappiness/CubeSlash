using UnityEngine;
using UnityEngine.UI;

public class UIAbilitySlot : MonoBehaviour
{
    [SerializeField] private Image img_icon;
    [SerializeField] private Image img_wrong;
    [SerializeField] private ButtonExtended button;

    public bool IsWrong { get; private set; }

    public Ability Ability { get; private set; }
    public ButtonExtended Button { get { return button; } }

    public void SetAbility(Ability ability)
    {
        this.Ability = ability;
        img_icon.sprite = ability == null ? null : ability.Info.sprite_icon;
        img_icon.enabled = ability != null;
    }

    public void SetWrong(bool wrong)
    {
        img_wrong.enabled = wrong;
        IsWrong = wrong;
    }
}