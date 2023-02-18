using UnityEngine;
using System.Linq;

[System.Serializable]
public class UpgradeStat
{
    public StatID id;
    public StatValue value = new StatValue();

    public string GetDisplayString()
    {
        var db = Database.Load<StatDatabase>();
        var stat = db.collection.FirstOrDefault(stat => stat.id == id);
        if (stat == null) return "";

        var s_value = value.GetValueString();
        var is_positive = value.IsPositive();
        var color = is_positive ? Color.white : Color.red;
        var sign = is_positive ? "+" : "";
        return stat.description.Replace("$", $"{sign}{s_value}").Color(color);
    }
}