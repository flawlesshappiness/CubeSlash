using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIAbilityEquipment : MonoBehaviour
{
    public PlayerInput.ButtonType type_button;

    [SerializeField] private UIAbilitySlot slot;
    [SerializeField] private UIAbilitySlot template_modifier_slot;
    [SerializeField] private ImageInput img_input;

    public UIAbilitySlot Slot { get { return slot; } }
    public List<UIAbilitySlot> ModifierSlots { get; set; } = new List<UIAbilitySlot>();

    public void Initialize(Ability ability)
    {
        template_modifier_slot.gameObject.SetActive(false);
        for (int i = 0; i < ConstVars.COUNT_MODIFIERS; i++)
        {
            var slot = Instantiate(template_modifier_slot, template_modifier_slot.transform.parent);
            slot.gameObject.SetActive(true);
            slot.SetAbility(null);
            ModifierSlots.Add(slot);
        }

        SetAbility(ability);
        img_input.SetInputType(PlayerInput.ButtonToUI(type_button));
    }

    public void SetAbility(Ability ability)
    {
        slot.SetAbility(ability);

        for (int i = 0; i < ModifierSlots.Count; i++)
        {
            var slot = ModifierSlots[i];
            var m = ability == null ? null : ability.Modifiers[i];
            slot.SetAbility(m);
        }
    }
}