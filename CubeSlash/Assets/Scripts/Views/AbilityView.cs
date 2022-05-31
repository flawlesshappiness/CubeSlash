using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityView : View
{
    [SerializeField] private UIAbilityCard prefab_card;
    [SerializeField] private RectTransform prefab_card_position;
    [SerializeField] private Button btn_continue;

    private void Start()
    {
        btn_continue.onClick.AddListener(ClickContinue);

        StartCoroutine(StartCr());

        // Audio
        AudioController.Instance.snapshot_menu.TransitionTo(0.5f);
    }

    private IEnumerator StartCr()
    {
        ClearPositions();
        for (int i = 0; i < PlayerInputController.Instance.CountAbilityButtons; i++)
        {
            AddPosition();
        }

        yield return null;

        ClearCards();
        for (int i = 0; i < rt_positions.Count; i++)
        {
            var card = CreateCard();
            card.Initialize();
            card.transform.position = rt_positions[i].position.AddY(-Screen.height);

            card.Index = i;
            card.UpdateUI();
            card.InputButton.SetJoystickButton(PlayerInputController.JoystickType.XBOX, PlayerInputController.Instance.GetJoystickButtonType(i));
            card.OnClickAbility.AddListener(() => ClickSelectAbility(card));
            card.OnClickModifier += i => ClickSelectModifier(card, i);

            Lerp.Position(card.transform, 0.5f, rt_positions[i].position)
                .Curve(Lerp.Curve.EASE_END);

            // Disable prefabs
            prefab_card.Interactable = false;
            prefab_card.InteractableList = false;
            prefab_card.gameObject.SetActive(false);
            prefab_card_position.gameObject.SetActive(false);

            yield return new WaitForSeconds(0.25f);
        }

        cards.ForEach(c =>
        {
            c.Interactable = true;
            c.InteractableList = false;
        });
        cards[0].SelectAbilityButton();
    }

    private IEnumerator EndCr()
    {
        CanvasGroup.blocksRaycasts = false;

        for (int i = rt_positions.Count-1; i >= 0; i--)
        {
            var card = cards[i];
            Lerp.Position(card.transform, 1f, rt_positions[i].position.AddX(Screen.width))
                .Curve(Lerp.Curve.EASE_END);

            yield return new WaitForSeconds(0.1f);
        }

        GameController.Instance.NextLevelTransition();
    }

    #region BUTTONS
    private void ClickContinue()
    {
        StartCoroutine(EndCr());
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
        prefab_card.gameObject.SetActive(true);
        var card = Instantiate(prefab_card.gameObject, prefab_card.transform.parent).GetComponent<UIAbilityCard>();
        cards.Add(card);
        prefab_card.gameObject.SetActive(false);
        return card;
    }
    #endregion
    #region POSITIONS
    private List<RectTransform> rt_positions = new List<RectTransform>();
    private void ClearPositions()
    {
        foreach(var rt in rt_positions)
        {
            Destroy(rt.gameObject);
        }
        rt_positions.Clear();
    }

    private void AddPosition()
    {
        prefab_card_position.gameObject.SetActive(true);
        var rt = Instantiate(prefab_card_position.gameObject, prefab_card_position.parent).GetComponent<RectTransform>();
        rt_positions.Add(rt);
        prefab_card_position.gameObject.SetActive(false);
    }
    #endregion
    #region SELECT ABILITY
    private UIAbilityCard card_select_ability;

    private void ClickSelectAbility(UIAbilityCard card)
    {
        if (card_select_ability != null) return;
        card_select_ability = card;
        var can_unequip = Player.Instance.AbilitiesEquipped[card.Index] != null;
        card.ShowSelectAbility(OnSelectAbility, HideSelectAbility, can_unequip);

        btn_continue.interactable = false;
        cards.ForEach(c =>
        {
            c.Interactable = false;
            c.InteractableList = c == card;
        });
    }

    private void OnSelectAbility(Ability ability)
    {
        Player.Instance.EquipAbility(ability, card_select_ability.Index);
        HideSelectAbility();
    }

    private void HideSelectAbility()
    {
        card_select_ability.HideSelectAbility();
        card_select_ability.UpdateUI();
        card_select_ability.SelectAbilityButton();
        card_select_ability = null;

        btn_continue.interactable = true;
        cards.ForEach(c =>
        {
            c.Interactable = true;
            c.InteractableList = false;
        });
    }
    #endregion
    #region SELECT MODIFIER
    private UIAbilityModifier modifier_select_ability;
    private void ClickSelectModifier(UIAbilityCard card, int idx_modifier)
    {
        if (card_select_ability) return;
        if (modifier_select_ability) return;

        var ability = Player.Instance.AbilitiesEquipped[card.Index];
        if (!ability) return;

        var modifier = card.Modifiers[idx_modifier];
        card_select_ability = card;
        modifier_select_ability = modifier;

        var can_unequip = ability.Modifiers[idx_modifier] != null;
        card.ShowSelectAbility(OnSelectModifier, HideSelectModifier, can_unequip);

        btn_continue.interactable = false;
        cards.ForEach(c =>
        {
            c.Interactable = false;
            c.InteractableList = c == card;
        });
    }

    private void OnSelectModifier(Ability ability)
    {
        var ability_equip = Player.Instance.AbilitiesEquipped[card_select_ability.Index];
        if (!ability_equip) return;

        ability_equip.SetModifier(ability, modifier_select_ability.Index);
        HideSelectModifier();
    }

    private void HideSelectModifier()
    {
        card_select_ability.HideSelectAbility();
        card_select_ability.UpdateUI();
        card_select_ability.SelectModifierButton(modifier_select_ability.Index);
        card_select_ability = null;
        modifier_select_ability = null;

        btn_continue.interactable = true;
        cards.ForEach(c =>
        {
            c.Interactable = true;
            c.InteractableList = false;
        });
    }
    #endregion
}