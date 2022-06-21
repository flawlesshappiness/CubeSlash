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

    public List<UIAbilitySelect> modifiers = new List<UIAbilitySelect>();

    public bool Interactable { set { cvg.interactable = value; cvg.blocksRaycasts = value; } }

    public void Initialize(int idx_button)
    {
        var ability = Player.Instance.AbilitiesEquipped[idx_button];
        ability_main.SetAbility(ability);
        InitializeModifiers(ability);

        // Events
        ability_main.OnAbilitySelected += a => Player.Instance.EquipAbility(a, idx_button);
        for (int i = 0; i < modifiers.Count; i++)
        {
            var idx = i;
            modifiers[i].OnAbilitySelected += a =>
            {
                if(a != null)
                {
                    Player.Instance.AbilitiesEquipped[idx_button].SetModifier(a, idx);
                }
            };
        }
    }

    private void InitializeModifiers(Ability ability)
    {
        for (int i = 0; i < ConstVars.COUNT_MODIFIERS; i++)
        {
            var modifier = Instantiate(prefab_modifier.gameObject, prefab_modifier.transform.parent)
                .GetComponent<UIAbilitySelect>();
            modifiers.Add(modifier);
            modifier.SetAbility(ability == null ? null : ability.Modifiers[i]);
        }
        prefab_modifier.Interactable = false;
        prefab_modifier.gameObject.SetActive(false);
    }
}
