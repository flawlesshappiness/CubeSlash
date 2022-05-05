using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityView : View
{
    [SerializeField] private UIAbilityCard prefab_card;
    [SerializeField] private RectTransform prefab_card_position;

    private void Start()
    {
        prefab_card.gameObject.SetActive(false);
        prefab_card_position.gameObject.SetActive(false);

        StartCoroutine(StartCr());
    }

    private IEnumerator StartCr()
    {
        ClearPositions();
        for (int i = 0; i < 4; i++)
        {
            AddPosition();
        }

        yield return null;

        ClearCards();
        for (int i = 0; i < rt_positions.Count; i++)
        {
            var card = AddCard();
            card.transform.position = rt_positions[i].position.AddY(-Screen.height);

            var ability = Player.Instance.AbilitiesEquipped[i];
            card.SetAbility(ability);

            Lerp.Position(card.transform, 0.5f, rt_positions[i].position)
                .Curve(Lerp.Curve.EASE_END);

            yield return new WaitForSeconds(0.25f);
        }
    }

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

    private UIAbilityCard AddCard()
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
}