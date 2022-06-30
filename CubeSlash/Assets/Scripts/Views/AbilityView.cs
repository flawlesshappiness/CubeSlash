using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;
using UnityEngine.InputSystem;

public class AbilityView : View
{
    [SerializeField] private UIAbilityCard prefab_card;
    [SerializeField] private Button btn_continue;
    [SerializeField] private TMP_Text tmp_desc;
    [SerializeField] private UIInputLayout layout_input;

    private void Start()
    {
        // Buttons
        btn_continue.onClick.AddListener(ClickContinue);

        // Cards
        InitializeAbilityCards();
        SetAbilitiesInteractable(true);

        // Audio
        AudioController.Instance.TransitionTo(AudioController.Snapshot.MENU, 0.5f);

        // Disable prefabs
        prefab_card.Interactable = false;
        prefab_card.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        GameController.Instance.PauseLock.AddLock(nameof(AbilityView));

        var input = PlayerInput.Controls.Player;
        input.West.performed += PressUnequip;
        input.Menu.performed += PressStart;
    }

    private void OnDisable()
    {
        GameController.Instance.PauseLock.RemoveLock(nameof(AbilityView));

        var input = PlayerInput.Controls.Player;
        input.West.performed -= PressUnequip;
        input.Menu.performed -= PressStart;
    }

    private void InitializeAbilityCards()
    {
        for (int i = 0; i < ConstVars.COUNT_ABILITY_BUTTONS; i++)
        {
            var card = CreateCard();
            card.Initialize(i);

            card.ability_main.OnSelected += () => OnAbilitySelected(card.ability_main, true);
            card.ability_main.OnDeselected += () => OnAbilitySelected(card.ability_main, false);
            card.ability_main.OnAbilityHighlighted += a => DisplayAbility(a);

            card.modifiers.ForEach(m =>
            {
                m.OnSelected += () => OnAbilitySelected(m, true);
                m.OnDeselected += () => OnAbilitySelected(m, false);
                m.OnAbilityHighlighted += a => DisplayAbility(a);
            });

            card.variables.ForEach(v =>
            {
                v.OnSelected += () => OnVariableSelected(v, true);
                v.OnDeselected += () => OnVariableSelected(v, false);
                v.OnHighlighted += v => DisplayVariable(v);
            });
        }

        cards[0].ability_main.SelectEventSystem();
    }

    private void OnAbilitySelected(UIAbilitySelect ability, bool selected)
    {
        SetAbilitiesInteractable(!selected);
        btn_continue.interactable = !selected;
        ability.Interactable = true;
        DisplayAbility(ability.Ability);

        // Input
        if (selected)
        {
            DisplayInputAbilitySelect();
        }
        else
        {
            DisplayInputNavigate();
        }
    }

    private void OnVariableSelected(UIAbilityVariable variable, bool selected)
    {
        SetAbilitiesInteractable(!selected);
        btn_continue.interactable = !selected;
        variable.Interactable = true;

        if (selected)
        {
            DisplayInputAbilityVariable();
        }
        else
        {
            DisplayInputNavigate();
        }
    }

    private void SetAbilitiesInteractable(bool interactable)
    {
        foreach(var card in cards)
        {
            card.ability_main.Interactable = interactable;

            var has_ability = card.ability_main.Ability != null;
            card.modifiers.Select((m, i) => (m, i))
                .ToList().ForEach(x =>
                {
                    x.m.Interactable = interactable && has_ability;
                });
            card.variables.Select((v, i) => (v, i))
                .ToList().ForEach(x =>
                {
                    x.v.Interactable = interactable && has_ability;
                });
        }
    }

    #region BUTTONS
    private void ClickContinue()
    {
        var p = Player.Instance;
        Close(0);
        GameController.Instance.ResumeLevel();
    }
    #endregion
    #region CARDS
    private List<UIAbilityCard> cards = new List<UIAbilityCard>();
    private void ClearCards()
    {
        foreach(var card in cards)
        {
            Destroy(card.gameObject);
        }
        cards.Clear();
    }

    private UIAbilityCard CreateCard()
    {
        var card = Instantiate(prefab_card.gameObject, prefab_card.transform.parent).GetComponent<UIAbilityCard>();
        card.gameObject.SetActive(true);
        cards.Add(card);
        return card;
    }
    #endregion
    #region DISPLAY
    private void DisplayAbility(Ability a)
    {
        if(a != null)
        {
            string s = a.name_ability;
            s += "\n" + a.desc_ability;
            tmp_desc.text = s;
        }
        else
        {
            tmp_desc.text = "";
        }
    }

    private void DisplayVariable(AbilityVariable v)
    {
        if(v != null)
        {
            string s = v.Name;
            s += "\n" + v.Description;
            tmp_desc.text = s;
        }
        else
        {
            tmp_desc.text = "";
        }
    }
    #endregion
    #region INPUT
    private void DisplayInputAbilitySelect()
    {
        layout_input.Clear();
        layout_input.AddInput(PlayerInput.UIButtonType.NAV_UP_DOWN, "Navigate");
        layout_input.AddInput(PlayerInput.UIButtonType.SOUTH, "Select");
        layout_input.AddInput(PlayerInput.UIButtonType.WEST, "Unequip");
        layout_input.AddInput(PlayerInput.UIButtonType.EAST, "Cancel");
    }

    private void DisplayInputAbilityVariable()
    {
        layout_input.Clear();
        layout_input.AddInput(PlayerInput.UIButtonType.NAV_LEFT_RIGHT, "Adjust");
        layout_input.AddInput(PlayerInput.UIButtonType.SOUTH, "Confirm");
        layout_input.AddInput(PlayerInput.UIButtonType.EAST, "Cancel");
    }

    private void DisplayInputNavigate()
    {
        layout_input.Clear();
        layout_input.AddInput(PlayerInput.UIButtonType.NAV_ALL, "Navigate");
        layout_input.AddInput(PlayerInput.UIButtonType.SOUTH, "Select");
        layout_input.AddInput(PlayerInput.UIButtonType.WEST, "Unequip");
    }
    #endregion

    private void PressUnequip(InputAction.CallbackContext context)
    {
        Unequip();
    }

    private void Unequip()
    {
        var selected = EventSystemController.Instance.EventSystem.currentSelectedGameObject;
        var ability_select = selected == null ? null : selected.GetComponentInParent<UIAbilitySelect>();
        ability_select.Unequip();
    }

    private void PressStart(InputAction.CallbackContext context)
    {
        var selected = EventSystemController.Instance.EventSystem.currentSelectedGameObject;
        var ability_select = selected == null ? null : selected.GetComponentInParent<UIAbilitySelect>();
        var ability_variable = selected == null ? null : selected.GetComponentInParent<UIAbilityVariable>();
        if (ability_select != null)
            ability_select.Cancel();
        if (ability_variable != null)
            ability_variable.Cancel();
        EventSystemController.Instance.EventSystem.SetSelectedGameObject(btn_continue.gameObject);
    }
}