using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public class AbilityView : View
{
    [SerializeField] private UIAbilityCard prefab_card;
    [SerializeField] private Button btn_continue;

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
    }

    private void OnDisable()
    {
        GameController.Instance.PauseLock.RemoveLock(nameof(AbilityView));
    }

    private void InitializeAbilityCards()
    {
        for (int i = 0; i < PlayerInputController.Instance.CountAbilityButtons; i++)
        {
            var card = CreateCard();
            card.Initialize(i);

            card.ability_main.OnSelected += () => OnAbilitySelected(card.ability_main);
            card.ability_main.OnDeselected += () => OnAbilityDeselected();

            card.modifiers.ForEach(m =>
            {
                m.OnSelected += () => OnAbilitySelected(m);
                m.OnDeselected += () => OnAbilityDeselected();
            });
        }

        cards[0].ability_main.SelectEventSystem();
    }

    private void OnAbilitySelected(UIAbilitySelect ability)
    {
        SetAbilitiesInteractable(false);
        btn_continue.interactable = false;
        ability.Interactable = true;
    }

    private void OnAbilityDeselected()
    {
        btn_continue.interactable = true;
        SetAbilitiesInteractable(true);
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
                    x.m.SetAbility(has_ability ? card.ability_main.Ability.Modifiers[x.i] : null);
                });
        }
    }

    #region BUTTONS
    private void ClickContinue()
    {
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
}