using UnityEngine;
using UnityEngine.UI;

public class UIAbilitySlotMove : MonoBehaviour
{
    [SerializeField] private Image img_icon;
    [SerializeField] private CanvasGroup cvg;

    public Ability Ability { get; private set; }

    public void SetAbility(Ability ability)
    {
        Ability = ability;
        img_icon.sprite = ability == null ? null : ability.sprite_icon;
        img_icon.enabled = ability != null;
        cvg.alpha = ability == null ? 0 : 1;
    }

    public void MoveToSlot(UIAbilitySlot slot)
    {
        var corners = new Vector3[4];
        var rt = slot.GetComponent<RectTransform>();
        rt.GetWorldCorners(corners);
        var corner = corners[2];
        var dir = corner - slot.transform.position;
        var pos = slot.transform.position + dir.normalized * dir.magnitude * 1.1f;
        Lerp.Position(transform, 0.1f, pos)
            .UnscaledTime();
    }
}