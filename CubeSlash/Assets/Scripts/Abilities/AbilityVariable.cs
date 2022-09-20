using UnityEngine;

[System.Serializable]
public class AbilityVariable
{
    public string name;
    public string text_display = "";
    public enum ValueType { INT, FLOAT, BOOL }
    public ValueType type_value;
    public enum DisplayType { INT, FLOAT, PERCENT, TEXT }
    public DisplayType type_display;
    public int value_int;
    public float value_float;

    public string GetValueString()
    {
        return type_display switch
        {
            DisplayType.INT => value_int.ToString(),
            DisplayType.PERCENT => $"{Mathf.RoundToInt(value_float * 100f)}%",
            DisplayType.FLOAT => $"{value_float.ToString("0.##")}",
            _ => ""
        };
    }

    public string GetDisplayString()
    {
        return text_display.Replace("$", GetValueString());
    }
}