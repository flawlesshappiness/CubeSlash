using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using static Upgrade;

public class AbilityStatView : View
{
    [SerializeField] private UIAbilityStatColumn template_column;
    [SerializeField] private RectTransform template_divider;

    private List<UIAbilityStatColumn> columns = new List<UIAbilityStatColumn>();
    private List<RectTransform> dividers = new List<RectTransform>();

    private void Start()
    {
        template_divider.gameObject.SetActive(false);
        template_column.gameObject.SetActive(false);
    }

    public void SetAbility(Ability ability)
    {
        // Create columns
        var infos = CreateColumn();
        var main = CreateColumn();
        var modifiers = new Dictionary<Ability, UIAbilityStatColumn>();
        foreach(var modifier in ability.Modifiers.Where(m => m != null))
        {
            var column = CreateColumn();
            modifiers.Add(modifier, column);
        }

        // Info
        infos.CreateVariable().SetText("Name");
        main.CreateVariable().SetText(ability.Info.name_ability);
        modifiers.ToList().ForEach(kvp => kvp.Value.CreateVariable().SetText(kvp.Key.Info.name_ability + " (modifier)"));

        CreateSpace();

        var display_stats = ability.Stats.stats.Where(stat => stat.type_display != StatParameter.DisplayType.TEXT);
        foreach (var stat in display_stats)
        {
            var stat_current = ability.GetValue(stat.name);
            infos.CreateVariable().SetText(stat.name);
            var positive = stat_current.ComparePositiveTo(stat);
            var color = ColorPalette.Main.Get(positive ? ColorPalette.Type.HIGHLIGHT : ColorPalette.Type.WRONG);
            main.CreateVariable().SetFromToText(stat.GetValueString(false), stat_current.GetValueString().Color(color));
        }

        // Modifiers
        var modifier_kvps = modifiers.ToList();
        foreach(var kvp in modifier_kvps)
        {
            var upgrade = ability.ModifierUpgrades.GetModifier(kvp.Key.Info.type).upgrade;

            if(upgrade != null)
            {
                // Current stats
                var used_effects = new List<string>();
                foreach (var stat in display_stats)
                {
                    if (upgrade != null)
                    {
                        var effect = upgrade.effects.FirstOrDefault(e => e.variable.name == stat.name && stat.type_display != StatParameter.DisplayType.TEXT);
                        if (effect == null)
                        {
                            kvp.Value.CreateSpace();
                        }
                        else
                        {
                            used_effects.Add(effect.variable.name);
                            var positive = effect.type_effect == Upgrade.Effect.TypeEffect.POSITIVE;
                            var color = ColorPalette.Main.Get(positive ? ColorPalette.Type.HIGHLIGHT : ColorPalette.Type.WRONG);
                            kvp.Value.CreateVariable().SetText(effect.variable.GetValueString(true).Color(color));
                        }
                    }
                }

                // Extra effects
                foreach (var effect in upgrade.effects.Where(e => !used_effects.Contains(e.variable.name)))
                {
                    kvp.Value.CreateVariable().SetText(effect.variable.GetDisplayString(true));
                }
            }
        }

        void CreateSpace() => columns.ForEach(c => c.CreateSpace());
    }

    private UIAbilityStatColumn CreateColumn()
    {
        var column = Instantiate(template_column, template_column.transform.parent);
        column.gameObject.SetActive(true);
        column.transform.SetAsLastSibling();
        columns.Add(column);
        CreateDivider();
        return column;
    }

    private RectTransform CreateDivider()
    {
        var div = Instantiate(template_divider, template_divider.parent);
        div.gameObject.SetActive(true);
        div.SetAsLastSibling();
        dividers.Add(div);
        return div;
    }
}