using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnlockAbilityView : View
{
    [SerializeField] private UIUnlockAbilityButton prefab_unlock;
    [SerializeField] private TMP_Text tmp_desc;

    private List<UIUnlockAbilityButton> btns = new List<UIUnlockAbilityButton>();

    private void Start()
    {
        prefab_unlock.Interactable = false;
        prefab_unlock.gameObject.SetActive(false);

        DisplayAbility(null);

        var unlocked_types = Player.Instance.AbilitiesUnlocked.Select(ability => ability.type).ToList();
        var abilities =
            System.Enum.GetValues(typeof(Ability.Type)).Cast<Ability.Type>()
            .Where(type => !unlocked_types.Contains(type))
            .Select(type => Ability.GetPrefab(type)).ToList();
        abilities.Shuffle();
        abilities = abilities.Take(2).ToList();
        ShowUnlockableAbilities(abilities);
    }

    public void ShowUnlockableAbilities(List<Ability> abilities)
    {
        foreach(var ability in abilities)
        {
            var btn = CreateAbilityButton(ability);
            btn.Interactable = false;
            btn.transform.localScale = Vector3.zero;
        }

        StartCoroutine(ShowButtonsCr());
    }

    private IEnumerator ShowButtonsCr()
    {
        yield return new WaitForSecondsRealtime(0.5f); // Wait for UI update
        for (int i = 0; i < btns.Count; i++)
        {
            var btn = btns[i];
            var lerp = Lerp.Scale(btn.transform, 0.25f, Vector3.zero, Vector3.one)
                .Curve(Lerp.Curve.EASE_END)
                .UnscaledTime();
            yield return new WaitForSecondsRealtime(0.1f);
        }

        yield return new WaitForSecondsRealtime(0.25f);

        btns[0].Select();
        btns.ToList().ForEach(b => b.Interactable = true);
    }

    private UIUnlockAbilityButton CreateAbilityButton(Ability ability)
    {
        var btn = Instantiate(prefab_unlock.gameObject, prefab_unlock.transform.parent).GetComponent<UIUnlockAbilityButton>();
        btn.gameObject.SetActive(true);
        btns.Add(btn);

        btn.Icon = ability.sprite_icon;
        btn.Ability = ability;

        btn.OnClick.AddListener(() => ClickAbilityButton(btn));

        btn.OnHighlighted += a =>
        {
            DisplayAbility(a);
        };

        return btn;
    }

    private void ClickAbilityButton(UIUnlockAbilityButton button)
    {
        btns.ToList().ForEach(b => b.Interactable = false);
        Player.Instance.UnlockAbility(button.Ability.type);
        StartCoroutine(Cr());

        IEnumerator Cr()
        {
            var unpicked = btns.Where(b => b != button).ToList();
            StartCoroutine(HideButtonsCr(unpicked));
            yield return HighlightButtonCr(button);
            ViewController.Instance.ShowView<AbilityView>(tag: "Ability");
        }

        IEnumerator HideButtonsCr(List<UIUnlockAbilityButton> buttons)
        {
            for (int i = 0; i < buttons.Count; i++)
            {
                var btn = buttons[i];
                Lerp.Scale(btn.transform, 0.25f, Vector3.one, Vector3.zero)
                    .UnscaledTime();
                yield return new WaitForSecondsRealtime(0.05f);
            }
        }

        IEnumerator HighlightButtonCr(UIUnlockAbilityButton button)
        {
            yield return Lerp.Scale(button.transform, 1f, Vector3.one, Vector3.one * 1.25f)
                .UnscaledTime()
                .GetCoroutine();

            yield return Lerp.Scale(button.transform, 0.25f, Vector3.zero)
                .UnscaledTime()
                .GetCoroutine();
        }
    }

    private void DisplayAbility(Ability a)
    {
        if (a != null)
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
}