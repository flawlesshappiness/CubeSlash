using System.Collections.Generic;
using UnityEngine;

public class UIStats : MonoBehaviour
{
    [SerializeField] private UITextObject temp_ability_text;
    [SerializeField] private UITextObject temp_ability_value;
    [SerializeField] private UITextObject temp_player_text;
    [SerializeField] private UITextObject temp_player_value;

    private List<UITextObject> texts = new List<UITextObject>();

    private void Start()
    {
        temp_ability_text.gameObject.SetActive(false);
        temp_ability_value.gameObject.SetActive(false);
        temp_player_text.gameObject.SetActive(false);
        temp_player_value.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        Clear();

        var ability = AbilityController.Instance.GetPrimaryAbility();
        if (ability != null)
        {
            var stats = ability.GetStats();
            foreach (var kvp in stats)
            {
                CreateAbilityText(kvp.Key, kvp.Value);
            }
        }

        var player_stats = Player.Instance.GetStats();
        foreach (var kvp in player_stats)
        {
            CreatePlayerText(kvp.Key, kvp.Value);
        }
    }

    private void Clear()
    {
        foreach (var text in texts)
        {
            Destroy(text);
        }

        texts.Clear();
    }

    private void CreateAbilityText(string text, string value)
    {
        // text
        var o_text = Instantiate(temp_ability_text, temp_ability_text.transform.parent);
        o_text.Text = text;

        // value
        var o_value = Instantiate(temp_ability_value, temp_ability_value.transform.parent);
        o_value.Text = value;

        texts.Add(o_text);
        texts.Add(o_value);
    }

    private void CreatePlayerText(string text, string value)
    {
        // text
        var o_text = Instantiate(temp_player_text, temp_player_text.transform.parent);
        o_text.Text = text;

        // value
        var o_value = Instantiate(temp_player_value, temp_player_value.transform.parent);
        o_value.Text = value;

        texts.Add(o_text);
        texts.Add(o_value);
    }
}