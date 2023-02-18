using UnityEngine;

[System.Serializable]
public class StatParameter
{
    public string name;
    public string text_display = "";
    public StatValue.ValueType type_value;
    public StatValue.DisplayType type_display;
    public bool higher_is_positive;
    public int value_int;
    public float value_float;
    public bool value_bool;

    public bool can_edit_name = true;
    public bool _editor_toggle_preview;

    public static string GetValueString(int i, float f, bool b, StatValue.ValueType value, StatValue.DisplayType type, bool isEffect)
    {
        var isPositiveInt = type == StatValue.DisplayType.INT && i > 0;
        var isPositiveFloat = (type == StatValue.DisplayType.FLOAT || type == StatValue.DisplayType.PERCENT) && f > 0;
        var isPositive = isPositiveInt || isPositiveFloat;
        var sign = isPositive && isEffect ? "+" : "";

        return type switch
        {
            StatValue.DisplayType.INT => $"{sign}{i}",
            StatValue.DisplayType.PERCENT => $"{sign}{Mathf.RoundToInt(f * 100f)}%",
            StatValue.DisplayType.FLOAT => $"{sign}{f.ToString("0.##")}",
            _ => value switch
            {
                StatValue.ValueType.INT => i.ToString(),
                StatValue.ValueType.FLOAT => f.ToString("0.##"),
                StatValue.ValueType.BOOL => b.ToString(),
                _ => ""
            }
        };
    }

    public string GetValueString(bool isEffect) => GetValueString(value_int, value_float, value_bool, type_value, type_display, isEffect);

    public string GetDisplayString(bool isEffect)
    {
        return text_display.Replace("$", GetValueString(isEffect));
    }

    public bool ComparePositiveTo(StatParameter parameter)
    {
        switch (parameter.type_value)
        {
            case StatValue.ValueType.INT:
                return parameter.higher_is_positive ? value_int >= parameter.value_int : value_int < parameter.value_int;

            case StatValue.ValueType.FLOAT:
                return parameter.higher_is_positive ? value_float >= parameter.value_float : value_float < parameter.value_float;

            case StatValue.ValueType.BOOL:
                return parameter.higher_is_positive == parameter.value_bool;

            default: return false;
        }
    }
}