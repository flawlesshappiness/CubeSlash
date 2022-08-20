using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIAbilityCard : MonoBehaviour
{
    [SerializeField] private CanvasGroup cvg;
    [SerializeField] public UIAbilitySelect ability_main;
    [SerializeField] private UIAbilitySelect prefab_modifier;

    public List<UIAbilitySelect> modifiers { get; private set; } = new List<UIAbilitySelect>();

    public bool Interactable { set { cvg.interactable = value; cvg.blocksRaycasts = value; } }

    public void Initialize(int idx_button)
    {
        var ability = Player.Instance.AbilitiesEquipped[idx_button];

        InitializeModifiers();

        // Events
        ability_main.OnAbilitySelected += a =>
        {
            Player.Instance.EquipAbility(a, idx_button);
            UpdateAbility(a);
        };

        for (int i = 0; i < modifiers.Count; i++)
        {
            var idx_modifier = i;
            var idx_ability = idx_button;
            modifiers[i].OnAbilitySelected += m =>
            {
                var a = Player.Instance.AbilitiesEquipped[idx_ability];
                if(a != null) a.SetModifier(m, idx_modifier);
            };

            modifiers[i].OnAbilityUnequipped += m =>
            {
                var a = Player.Instance.AbilitiesEquipped[idx_ability];
                if (a != null) a.SetModifier(null, idx_modifier);
            };
        }

        ability_main.OnAbilityUnequipped += a =>
        {
            Player.Instance.UnequipAbility(idx_button);
            UpdateAbility(null);
        };

        // Ability
        ability_main.SetAbility(ability);
        UpdateAbility(ability);
    }

    private void InitializeModifiers()
    {
        for (int i = 0; i < ConstVars.COUNT_MODIFIERS; i++)
        {
            var modifier = Instantiate(prefab_modifier.gameObject, prefab_modifier.transform.parent)
                .GetComponent<UIAbilitySelect>();
            modifiers.Add(modifier);
        }
        prefab_modifier.Interactable = false;
        prefab_modifier.gameObject.SetActive(false);
    }

    private void UpdateAbility(Ability ability)
    {
        for (int i = 0; i < modifiers.Count; i++)
        {
            modifiers[i].SetAbility(ability == null ? null : ability.Modifiers[i]);
        }
    }
}
