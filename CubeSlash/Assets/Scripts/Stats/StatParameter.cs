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
    public int value_int;
    public float value_float;
    public bool value_bool;

    public bool can_edit_name = true;
    public bool _editor_toggle_preview;

    public string GetValueString(bool isEffect)
    {
        var isPositiveInt = type_display == DisplayType.INT && value_int > 0;
        var isPositiveFloat = (type_display == DisplayType.FLOAT || type_display == DisplayType.PERCENT) && value_float > 0;
        var isPositive = isPositiveInt || isPositiveFloat;
        var sign = isPositive && isEffect ? "+" : "";

        return type_display switch
        {
            DisplayType.INT => $"{sign}{value_int}",
            DisplayType.PERCENT => $"{sign}{Mathf.RoundToInt(value_float * 100f)}%",
            DisplayType.FLOAT => $"{sign}{value_float.ToString("0.##")}",
            _ => ""
        };
    }

    public string GetDisplayString(bool isEffect)
    {
        return text_display.Replace("$", GetValueString(isEffect));
    }
}