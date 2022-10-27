using UnityEngine;

[System.Serializable]
public class StatParameter
{
    public string name;
    public string text_display = "";
    public enum ValueType { INT, FLOAT, BOOL }
    public ValueType type_value;
    public enum DisplayType { INT, FLOAT, PERCENT, TEXT }
    public DisplayType type_display;
    public bool higher_is_positive;
    public int value_int;
    public float value_float;
    public bool value_bool;

    public bool can_edit_name = true;
    public bool _editor_toggle_preview;

    public static string GetValueString(int i, float f, bool b, ValueType value, DisplayType type, bool isEffect)
    {
        var isPositiveInt = type == DisplayType.INT && i > 0;
        var isPositiveFloat = (type == DisplayType.FLOAT || type == DisplayType.PERCENT) && f > 0;
        var isPositive = isPositiveInt || isPositiveFloat;
        var sign = isPositive && isEffect ? "+" : "";

        return type switch
        {
            DisplayType.INT => $"{sign}{i}",
            DisplayType.PERCENT => $"{sign}{Mathf.RoundToInt(f * 100f)}%",
            DisplayType.FLOAT => $"{sign}{f.ToString("0.##")}",
            _ => value switch
            {
                ValueType.INT => i.ToString(),
                ValueType.FLOAT => f.ToString("0.##"),
                ValueType.BOOL => b.ToString(),
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
            case ValueType.INT:
                return parameter.higher_is_positive ? value_int >= parameter.value_int : value_int < parameter.value_int;

            case ValueType.FLOAT:
                return parameter.higher_is_positive ? value_float >= parameter.value_float : value_float < parameter.value_float;

            case ValueType.BOOL:
                return parameter.higher_is_positive == parameter.value_bool;

            default: return false;
        }
    }
}