using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIAbilityList : MonoBehaviour
{
    [SerializeField] private UIAbilityListElement prefab_element;

    private List<UIAbilityListElement> elements = new List<UIAbilityListElement>();
    private System.Action<Ability> onSelect;
    private System.Action onCancel;

    private void Update()
    {
        if (Input.GetButtonDown("Cancel"))
        {
            onCancel?.Invoke();
        }
    }

    public void Show(bool has_unequip, System.Action<Ability> onSelect, System.Action onCancel)
    {
        this.onSelect = onSelect;
        this.onCancel = onCancel;

        // Clear
        prefab_element.gameObject.SetActive(false);
        ClearElements();

        // Create unequip
        if (has_unequip)
        {
            var element = CreateElement();
            element.Name = "Unequip";
            element.OnClicked.AddListener(() => onSelect(null));
        }

        // Create abilities
        var abilities = Player.Instance.AbilitiesUnlocked;
        for (int i = 0; i < abilities.Count; i++)
        {
            var ability = abilities[i];
            var element = CreateElement();
            element.SetAbility(ability);
            element.Button.interactable = !ability.Equipped;
            element.OnClicked.AddListener(() => onSelect(ability));
        }

        var first_selectable = elements.FirstOrDefault(element => element.Button.interactable) ?? elements[0];
        EventSystem.current.SetSelectedGameObject(first_selectable.Button.gameObject);
    }

    private void ClearElements()
    {
        foreach(var element in elements)
        {
            Destroy(element.gameObject);
        }
        elements.Clear();
    }

    private UIAbilityListElement CreateElement()
    {
        prefab_element.gameObject.SetActive(true);
        var inst = Instantiate(prefab_element.gameObject, prefab_element.transform.parent).GetComponent<UIAbilityListElement>();
        elements.Add(inst);
        prefab_element.gameObject.SetActive(false);
        return inst;
    }
}
