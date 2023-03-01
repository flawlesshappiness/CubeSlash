using UnityEngine;
using System.Linq;

[System.Serializable]
public class UpgradeStat
{
    public StatID id;
    public StatValue value = new StatValue();

    public string GetDisplayString()
    {
        var db_color = Database.Load<ColorDatabase>();
        var db = Database.Load<StatDatabase>();
        var stat = db.collection.FirstOrDefault(stat => stat.id == id);
        if (stat == null) return "";

        var s_value = value.GetValueString();
        var is_value_positive = value.IsPositive();
        var is_effect_positive = !(stat.high_is_positive ^ is_value_positive);
        var color = is_effect_positive ? db_color.text_normal.GetColor() : db_color.text_wrong.GetColor();
        var sign = (is_value_positive && stat.display_has_plus) ? "+" : "";
        return stat.description.Replace("$", $"{sign}{s_value}").Color(color);
    }
}