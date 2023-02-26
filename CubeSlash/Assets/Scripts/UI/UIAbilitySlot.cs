using UnityEngine;
using UnityEngine.UI;

public class UIAbilitySlot : MonoBehaviour
{
    [SerializeField] public RectTransform rectTransform;
    [SerializeField] private SelectableMenuItem button;
    [SerializeField] private UIIconButton btn_icon;

    public bool IsWrong { get; private set; }

    public Ability Ability { get; private set; }
    public SelectableMenuItem Button { get { return button; } }
    public UIIconButton IconButton { get { return btn_icon; } }

    private void OnValidate()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    public void SetAbility(Ability ability)
    {
        this.Ability = ability;
        btn_icon.Icon = ability == null ? null : ability.Info.sprite_icon;
        btn_icon.IconEnabled = ability != null;
    }

    public void SetWrong(bool wrong)
    {
        IsWrong = wrong;
        btn_icon.IsWrong = wrong;
    }
}