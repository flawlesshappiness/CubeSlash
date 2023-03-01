using UnityEngine;

[CreateAssetMenu(fileName = nameof(Stat), menuName = "Game/" + nameof(Stat), order = 1)]
public class Stat : ScriptableObject
{
    public StatID id;
    public string description = "";
    public StatValue value = new StatValue();
    public bool high_is_positive = true;
    public bool display_has_plus = true;

    public string GetDisplayString()
    {
        if(value.type_value == StatValue.ValueType.BOOL)
        {
            return description;
        }
        else
        {
            var s_value = value.GetValueString();
            return description.Replace("$", s_value);
        }
    }
}