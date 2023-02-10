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

        // Info
        infos.CreateVariable().SetText("Name");
        main.CreateVariable().SetText(ability.Info.name_ability);

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

        dividers[dividers.Count - 1].gameObject.SetActive(false);

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