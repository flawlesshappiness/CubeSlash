using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UnlockView : View
{
    [SerializeField] private GameObject panel_abilities;
    [SerializeField] private GameObject panel_upgrades;
    [SerializeField] private UIUnlockButton prefab_ability;
    [SerializeField] private UIUnlockButton prefab_upgrade;
    [SerializeField] private TMP_Text tmp_desc;

    private List<UIUnlockButton> btns = new List<UIUnlockButton>();

    private bool initialized;

    private void Initialize()
    {
        if (initialized) return;
        initialized = true;

        prefab_ability.Interactable = false;
        prefab_ability.gameObject.SetActive(false);
        prefab_upgrade.Interactable = false;
        prefab_upgrade.gameObject.SetActive(false);
        panel_abilities.SetActive(false);
        panel_upgrades.SetActive(false);

        tmp_desc.text = "";
    }

    #region BUTTONS
    private UIUnlockButton CreateUnlockButton(UIUnlockButton prefab)
    {
        var btn = Instantiate(prefab.gameObject, prefab.transform.parent).GetComponent<UIUnlockButton>();
        btn.gameObject.SetActive(true);
        btns.Add(btn);
        return btn;
    }
    #endregion
    #region ANIMATION
    private IEnumerator ShowButtonsCr()
    {
        yield return null; // Wait for UI update
        for (int i = 0; i < btns.Count; i++)
        {
            var btn = btns[i];
            var lerp = Lerp.Scale(btn.transform, 0.15f, Vector3.zero, Vector3.one)
                .Curve(Lerp.Curve.EASE_END)
                .UnscaledTime();
            yield return new WaitForSecondsRealtime(0.05f);
        }

        yield return new WaitForSecondsRealtime(0.15f);

        btns[0].Select();
        btns.ToList().ForEach(b => b.Interactable = true);
    }

    private IEnumerator HideButtonsCr(List<UIUnlockButton> buttons)
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            var btn = buttons[i];
            Lerp.Scale(btn.transform, 0.15f, Vector3.one, Vector3.zero)
                .UnscaledTime();
            yield return new WaitForSecondsRealtime(0.05f);
        }
    }

    private IEnumerator HighlightButtonCr(UIUnlockButton button)
    {
        yield return Lerp.Scale(button.transform, 0.25f, button.transform.localScale * 1.25f)
            .UnscaledTime()
            .GetCoroutine();

        yield return Lerp.Scale(button.transform, 0.15f, Vector3.zero)
            .UnscaledTime()
            .GetCoroutine();
    }
    #endregion
    #region UPGRADES
    public static bool CanUnlockUpgrade()
    {
        return GetUnlockableUpgrades().Count > 0;
    }

    private static List<Upgrade> GetUnlockableUpgrades()
    {
        return Player.Instance.AbilitiesEquipped
            .Where(a => a != null)
            .SelectMany(a => a.Upgrades)
            .Select(u => UpgradeController.Instance.GetUpgrade(u.type))
            .Where(u => !u.IsMaxLevel)
            .ToList();
    }

    public void UnlockUpgrade()
    {
        Initialize();

        var upgrades = GetUnlockableUpgrades();
        upgrades.Shuffle();
        upgrades = upgrades.Take(4).ToList();
        ShowUnlockableUpgrades(upgrades);
    }

    private void ShowUnlockableUpgrades(List<Upgrade> upgrades)
    {
        panel_upgrades.SetActive(true);

        foreach (var upgrade in upgrades)
        {
            var btn = CreateUpgradeButton(upgrade);
            btn.Interactable = false;
            btn.transform.localScale = Vector3.zero;
        }

        StartCoroutine(ShowButtonsCr());
    }

    private UIUnlockButton CreateUpgradeButton(Upgrade upgrade)
    {
        var level = upgrade.data.levels[upgrade.level];
        var btn = CreateUnlockButton(prefab_upgrade);
        btn.Icon = level.icon;
        btn.Button.onClick.AddListener(() => ClickUpgradeButton(btn, upgrade));
        btn.Button.OnSelected += () => DisplayUpgradeLevel(level);
        return btn;
    }

    private void ClickUpgradeButton(UIUnlockButton button, Upgrade upgrade)
    {
        UpgradeController.Instance.IncrementUpgradeLevel(upgrade.data.type);
        btns.ToList().ForEach(b => b.Interactable = false);
        StartCoroutine(Cr());

        IEnumerator Cr()
        {
            var unpicked = btns.Where(b => b != button).ToList();
            StartCoroutine(HideButtonsCr(unpicked));
            yield return HighlightButtonCr(button);
            Close(0.5f);
            GameController.Instance.ResumeLevel();
        }
    }

    private void DisplayUpgradeLevel(UpgradeData.Level data)
    {
        if (data != null)
        {
            string s = data.name;
            s += "\n" + data.description;
            tmp_desc.text = s;
        }
    }
    #endregion
    #region ABILITIES
    public static bool CanUnlockAbility()
    {
        return GetUnlockableAbilities().Count > 0;
    }

    private static List<Ability> GetUnlockableAbilities()
    {
        var unlocked_types = Player.Instance.AbilitiesUnlocked.Select(ability => ability.type).ToList();
        var abilities =
            System.Enum.GetValues(typeof(Ability.Type)).Cast<Ability.Type>()
            .Where(type => !unlocked_types.Contains(type))
            .Select(type => Ability.GetPrefab(type)).ToList();
        return abilities;
    }

    public void UnlockAbility()
    {
        Initialize();

        var abilities = GetUnlockableAbilities().Take(2).ToList();
        abilities.Shuffle();
        ShowUnlockableAbilities(abilities);
    }

    private void ShowUnlockableAbilities(List<Ability> abilities)
    {
        panel_abilities.SetActive(true);

        foreach(var ability in abilities)
        {
            var btn = CreateAbilityButton(ability);
            btn.Interactable = false;
            btn.transform.localScale = Vector3.zero;
        }

        StartCoroutine(ShowButtonsCr());
    }

    private UIUnlockButton CreateAbilityButton(Ability ability)
    {
        var btn = CreateUnlockButton(prefab_ability);
        btn.Icon = ability.sprite_icon;

        btn.Button.onClick.AddListener(() => ClickAbilityButton(btn, ability));
        btn.Button.OnSelected += () => DisplayAbility(ability);

        return btn;
    }

    private void ClickAbilityButton(UIUnlockButton button, Ability ability)
    {
        Player.Instance.UnlockAbility(ability.type);
        btns.ToList().ForEach(b => b.Interactable = false);
        StartCoroutine(Cr());

        IEnumerator Cr()
        {
            var unpicked = btns.Where(b => b != button).ToList();
            StartCoroutine(HideButtonsCr(unpicked));
            yield return HighlightButtonCr(button);
            ViewController.Instance.ShowView<AbilityView>(tag: "Ability");
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
    }
    #endregion
}