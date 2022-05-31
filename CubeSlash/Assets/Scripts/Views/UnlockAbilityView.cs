using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnlockAbilityView : View
{
    [SerializeField] private RectTransform prefab_position;
    [SerializeField] private UIUnlockAbilityButton prefab_unlock;

    private List<UIUnlockAbilityButton> btns = new List<UIUnlockAbilityButton>();
    private List<RectTransform> poss = new List<RectTransform>();

    private void Start()
    {
        prefab_unlock.gameObject.SetActive(false);
        prefab_position.gameObject.SetActive(false);

        var unlocked_types = Player.Instance.AbilitiesUnlocked.Select(ability => ability.type).ToList();
        var abilities =
            System.Enum.GetValues(typeof(Ability.Type)).Cast<Ability.Type>()
            //.Where(type => !unlocked_types.Contains(type))
            .Select(type => Ability.GetPrefab(type)).ToList()
            .Random(2);
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
        yield return new WaitForSeconds(0.5f); // Wait for UI update
        for (int i = 0; i < btns.Count; i++)
        {
            var btn = btns[i];
            var pos = poss[i];
            btn.transform.position = pos.transform.position;
            var lerp = Lerp.Scale(btn.transform, 0.25f, Vector3.zero, Vector3.one)
                .Curve(Lerp.Curve.EASE_END);
            yield return new WaitForSeconds(0.1f);
        }

        yield return new WaitForSeconds(0.25f);

        btns[0].Select();
        btns.ToList().ForEach(b => b.Interactable = true);
    }

    private UIUnlockAbilityButton CreateAbilityButton(Ability ability)
    {
        var btn = Instantiate(prefab_unlock.gameObject, prefab_unlock.transform.parent).GetComponent<UIUnlockAbilityButton>();
        btn.gameObject.SetActive(true);
        btns.Add(btn);

        var pos = Instantiate(prefab_position.gameObject, prefab_position.transform.parent).GetComponent<RectTransform>();
        pos.gameObject.SetActive(true);
        poss.Add(pos);

        btn.Header = ability.name_ability;
        btn.Description = ability.desc_ability;
        btn.Icon = ability.sprite_icon;

        btn.OnClick.AddListener(() => ClickAbilityButton(ability));

        return btn;
    }

    private void ClickAbilityButton(Ability prefab)
    {
        Player.Instance.UnlockAbility(prefab.type);
        ViewController.Instance.ShowView<AbilityView>();
    }
}