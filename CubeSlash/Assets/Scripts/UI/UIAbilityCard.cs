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
    [SerializeField] private UIAbilityVariable prefab_variable;

    public List<UIAbilitySelect> modifiers { get; private set; } = new List<UIAbilitySelect>();
    public List<UIAbilityVariable> variables { get; private set; } = new List<UIAbilityVariable>();

    public bool Interactable { set { cvg.interactable = value; cvg.blocksRaycasts = value; } }

    public void Initialize(int idx_button)
    {
        var ability = Player.Instance.AbilitiesEquipped[idx_button];

        InitializeModifiers();
        InitializeVariables();

        // Events
        ability_main.OnAbilitySelected += a =>
        {
            Player.Instance.EquipAbility(a, idx_button);
            UpdateAbility(a);
        };

        for (int i = 0; i < modifiers.Count; i++)
        {
            var idx = i;
            modifiers[i].OnAbilitySelected += m =>
            {
                if(ability != null)
                {
                    ability.SetModifier(m, idx);
                }
            };
        }

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

    private void InitializeVariables()
    {
        for (int i = 0; i < ConstVars.COUNT_VARIABLES; i++)
        {
            var ui = Instantiate(prefab_variable.gameObject, prefab_variable.transform.parent)
                .GetComponent<UIAbilityVariable>();
            variables.Add(ui);
        }
        prefab_variable.Interactable = false;
        prefab_variable.gameObject.SetActive(false);
    }

    private void UpdateAbility(Ability ability)
    {
        for (int i = 0; i < modifiers.Count; i++)
        {
            modifiers[i].SetAbility(ability == null ? null : ability.Modifiers[i]);
        }
        for (int i = 0; i < variables.Count; i++)
        {
            variables[i].SetVariable(ability == null ? null : ability.Variables[i]);
        }
    }
}
