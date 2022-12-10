using UnityEngine;
using UnityEngine.UI;
using Flawliz.Lerp;

public class UIAbilitySlotMove : MonoBehaviour
{
    [SerializeField] private Image img_icon;
    [SerializeField] private CanvasGroup cvg;

    public Ability Ability { get; private set; }

    public void SetAbility(Ability ability)
    {
        Ability = ability;
        img_icon.sprite = ability == null ? null : ability.Info.sprite_icon;
        img_icon.enabled = ability != null;
        cvg.alpha = ability == null ? 0 : 1;
    }

    public void MoveToSlot(UIAbilitySlot slot)
    {
        var rt = slot.rectTransform;
        var rt_size = rt.rect.size;
        var pos = rt.transform.position.ToVector2() + rt_size * 0.5f;
        Lerp.Position(transform, 0.1f, pos).UnscaledTime();
    }
}